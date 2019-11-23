using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ChatroomUWP.Classes
{
    /// <summary>
    /// A chatroom client.
    /// </summary>
    class ChatroomClient : IDisposable
    {
        #region Singleton instance

        private static ChatroomClient _instance;

        /// <summary>
        /// Returns the current instance of the <see cref="ChatroomClient"/>.
        /// </summary>
        public static ChatroomClient GetInstance()
        {
            _instance ??= new ChatroomClient();

            return _instance;
        }

        #endregion


        #region Private fields

        private string _clientId;
        private IMqttClient _mqttClient;
        private IMqttClientOptions _mqttClientOptions;
        private bool _canReconnect;
        private CancellationTokenSource _connectionTokenSource;

        private HttpClient _httpClient;

        private static ToastNotifier _notifier =
            ToastNotificationManager.CreateToastNotifier();

        #endregion

        #region Public properties

        /// <summary>
        /// Tells whether the client is logged in or not.
        /// </summary>
        public bool IsLoggedIn =>
            _mqttClient == null
            ? false
            : _mqttClient.IsConnected;

        /// <summary>
        /// The client's username.
        /// </summary>
        public string Username => _clientId;

        #endregion

        #region Consts/readonly fields

        /// <summary>
        /// The server's hostname.
        /// </summary>
        public const string
            SERVER_HOSTNAME = "broker.hivemq.com"; 

        public const string
            PRIVATE_ROOMS_TOPIC_PREFIX = "chatroom/private-messages";

        public const string
            GENERAL_ROOM_TOPIC = "chatroom";

        public const string
            REGISTRATION_SERVER = "registration-server.fermi.mo.it:40000";

        #endregion

        #region Events

        /// <summary>
        /// Raised when the client disconnects.
        /// </summary>
        public event Action Disconnected;

        /// <summary>
        /// Raised when the client reconnects.
        /// </summary>
        public event Action Reconnected;

        /// <summary>
        /// Raised when a connection error arises.
        /// </summary>
        public event Action ConnectionError;

        /// <summary>
        /// Raised when the clientId already exists.
        /// </summary>
        public event Action DuplicateName;

        /// <summary>
        /// Raised when a new message is received.
        /// </summary>
        public event Action<string, ChatroomMessage> MessageReceived;

        /// <summary>
        /// Raised when the client connects.
        /// </summary>
        public event Action Connected;

        #endregion


        #region Begin
        /// <summary>
        /// Initializes the client and 
        /// starts the connection if the given username
        /// is not duplicated.
        /// </summary>
        public async Task Join(string username)
        {
            _clientId = username;
            _canReconnect = true;

            if (!await RegisterUsernameAsync(username))
                return;

            InitializeMqttClientAsync();
        }
        #endregion


        #region Initialize Mqtt client
        /// <summary>
        /// Instantiates and connects the mqtt client.
        /// </summary>
        private async void InitializeMqttClientAsync()
        {
            MqttFactory factory = new MqttFactory();
            _mqttClient = factory
                .CreateMqttClient()
                .UseDisconnectedHandler(ClientDisconnectedHandler)
                .UseApplicationMessageReceivedHandler(MessageReceivedHandler);

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_clientId)
                .WithCommunicationTimeout(TimeSpan.FromSeconds(10))
                .WithTcpServer(SERVER_HOSTNAME)
                .Build();

            await EstablishConnectionAsync();
        }
        #endregion

        #region Establish connection async
        /// <summary>
        /// Connects and subscribes to the topics.
        /// </summary>
        private async Task EstablishConnectionAsync()
        {
            _connectionTokenSource =
                new CancellationTokenSource();

            using (_connectionTokenSource)
            {
                await _mqttClient.ConnectAsync
                (
                    _mqttClientOptions,
                    _connectionTokenSource.Token
                )
                .ContinueWith(async _ => // Subscribing
                {
                    MqttClientSubscribeOptions subOpts = new MqttClientSubscribeOptionsBuilder()
                        .WithTopicFilter(GENERAL_ROOM_TOPIC)
                        .WithTopicFilter($"{PRIVATE_ROOMS_TOPIC_PREFIX}/{_clientId}")
                        .Build();

                    await _mqttClient.SubscribeAsync(
                        subOpts, _connectionTokenSource.Token);
                })
                .ContinueWith(_ => Connected?.Invoke());
            }
        }
        #endregion

        #region Client disconnected handler
        /// <summary>
        /// Handles reconnection and raises the 
        /// <see cref="Disconnected"></see> event.
        /// </summary>
        private async Task ClientDisconnectedHandler(MqttClientDisconnectedEventArgs e)
        {
            if (e.ClientWasConnected)
            {
                Disconnected?.Invoke();

                if (_canReconnect)
                {
                    await _mqttClient.ReconnectAsync();
                    Reconnected?.Invoke();
                }

                return;
            }
            else ConnectionError?.Invoke();

            _connectionTokenSource =
                new CancellationTokenSource();

            using (_connectionTokenSource)
            {
                await Task.Delay(5000);
                await _mqttClient.ConnectAsync(
                    _mqttClientOptions,
                    _connectionTokenSource.Token);
            }
        }
        #endregion


        #region Message received handler
        /// <summary>
        /// Handles the reception of messages.
        /// </summary>
        private async void MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            MqttApplicationMessage msg = e.ApplicationMessage;

            try
            {
                ChatroomMessage chatMsg =
                    JsonSerializer.Deserialize<ChatroomMessage>(
                        msg.Payload, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                bool isInvalid =
                    string.IsNullOrWhiteSpace(chatMsg.Username) ||
                    string.IsNullOrWhiteSpace(chatMsg.Contents);

                if (isInvalid)
                    throw new JsonException();

                MessageReceived?.Invoke(
                    msg.Topic,
                    chatMsg);

                ShowNotification(chatMsg);

                await ChatroomMessagesManager
                    .Dispatch(msg.Topic, chatMsg);
            }
            catch (JsonException) { /*Bad message*/ }
        }
        #endregion

        #region Show notification
        private void ShowNotification(ChatroomMessage chatMsg)
        {
            var notificationContent = new XmlDocument();
            notificationContent.LoadXml($@"
                <toast displayTimestamp={$"\"{chatMsg.Timestamp}\""}>
                    <visual>
                        <binding template={"\"ToastGeneric\""}>
                            <text>{$"\"{chatMsg.Username}\""}</text>
                            <text>{$"\"{chatMsg.Contents}\""}</text>
                        </binding>
                    </visual>
                    <audio src={"\"ms-winsoundevent:Notification.Reminder\""}/>
                </toast>");

            _notifier.Show(new ToastNotification(notificationContent));
        }
        #endregion


        #region Publish async
        /// <summary>
        /// Publishes a message asynchronously.
        /// </summary>
        public async Task<MqttClientPublishResult> PublishAsync(
            string topic,
            ChatroomMessage message)
        {
            return await _mqttClient.PublishAsync(new MqttApplicationMessage
            {
                Topic = topic,
                Payload = JsonSerializer
                    .SerializeToUtf8Bytes(new
                    {
                        username = _clientId,
                        contents = message.Contents,
                        timestamp = message.Timestamp
                    })
            });
        }
        #endregion


        #region Register username async
        /// <summary>
        /// Raises the <see cref="DuplicateName"/> event if 
        /// the clientId is already registered.
        /// </summary>
        private async Task<bool> RegisterUsernameAsync(string username)
        {
            _httpClient ??= new HttpClient();

            _connectionTokenSource = new CancellationTokenSource();

            try
            {
                using (_connectionTokenSource)
                {
                    using StringContent content =
                        new StringContent(username);

                    HttpResponseMessage resp = await
                        _httpClient.PostAsync(
                            REGISTRATION_SERVER, 
                            content, 
                            _connectionTokenSource.Token);

                    if (!resp.IsSuccessStatusCode)
                        DuplicateName?.Invoke();

                    return resp.IsSuccessStatusCode;
                }
            }
            catch
            {
                ConnectionError?.Invoke();
                return false;
            }
        }
        #endregion

        #region Deregister username async
        /// <summary>
        /// Deregisters the client.
        /// The mqtt connection is stopped.
        /// </summary>
        public async Task DeregisterUsernameAsync()
        {
            _httpClient ??= new HttpClient();

            _canReconnect = false;
            if (_mqttClient != null)
                await _mqttClient.DisconnectAsync();

            _connectionTokenSource = new CancellationTokenSource();

            try
            {
                using (_connectionTokenSource)
                {
                    using StringContent content =
                        new StringContent(_clientId);

                    await _httpClient
                        .PostAsync(
                            REGISTRATION_SERVER, 
                            content,
                            _connectionTokenSource.Token);
                }
            }
            catch
            {
                ConnectionError?.Invoke();
                return;
            }
        }
        #endregion


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual async void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _instance = null;

                if (disposing)
                {
                    _clientId = null;
                    _mqttClientOptions = null;
                }

                _httpClient?.Dispose();
                _connectionTokenSource?.Dispose();

                if (_mqttClient != null)
                {
                    using (_mqttClient)
                        await _mqttClient.DisconnectAsync();
                }

                _disposedValue = true;
            }
        }

        ~ChatroomClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
