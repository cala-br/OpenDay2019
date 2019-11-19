using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Subscribing;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        private string             _clientId;
        private IMqttClient        _mqttClient;
        private IMqttClientOptions _mqttClientOptions;

        private CancellationTokenSource _connectionTokenSource;

        #endregion

        #region Public properties

        /// <summary>
        /// Tells whether the client is logged in or not.
        /// </summary>
        public bool IsLoggedIn =>
            _mqttClient == null
            ? false
            : _mqttClient.IsConnected;

        #endregion

        #region Consts/readonly fields

        /// <summary>
        /// The server's hostname.
        /// </summary>
        public const string 
            SERVER_HOSTNAME = "localhost";

        public const string
            USERNAMES_TOPIC = "chatroom/usernames";

        #endregion

        #region Events

        /// <summary>
        /// Raised when the client disconnects.
        /// </summary>
        public event Action Disconnected;

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
        /// starts the connection.
        /// </summary>
        public void Begin(string clientId)
        {
            _clientId = clientId;

            InitializeMqttClientAsync();
        }
        #endregion


        #region Initialize Mqtt client
        /// <summary>
        /// Instantiates and connects the mqtt client.
        /// </summary>
        private async void InitializeMqttClientAsync()
        {
            var factory = new MqttFactory();
            _mqttClient = factory
                .CreateMqttClient()
                .UseDisconnectedHandler(ClientDisconnectedHandler)
                .UseApplicationMessageReceivedHandler(MessageReceivedHandler);

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_clientId)
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
                    var subOpts = new MqttClientSubscribeOptionsBuilder()
                        .WithTopicFilter( "chatroom/",            retainAsPublished: true)
                        .WithTopicFilter($"chatroom/{_clientId}", retainAsPublished: true)
                        .WithTopicFilter(USERNAMES_TOPIC,         retainAsPublished: true)
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
                Disconnected?.Invoke();

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
            var msg = e.ApplicationMessage;

            if (msg.Topic == USERNAMES_TOPIC)
            {
                var payload = JsonSerializer
                    .Deserialize<List<string>>(msg.Payload);

                await CheckForDuplicateNameAsync(payload);
                return;
            }

            try
            {
                var chatMsg =
                    JsonSerializer.Deserialize<ChatroomMessage>(msg.Payload);

                MessageReceived?.Invoke(msg.Topic, chatMsg);
            }
            catch (JsonException) { /*Bad message*/ }
        }
        #endregion

        #region Check for duplicate name async
        /// <summary>
        /// Raises the <see cref="DuplicateName"/> event if 
        /// the clientId is already registered.
        /// </summary>
        /// <returns></returns>
        private async Task CheckForDuplicateNameAsync(List<string> usernames)
        {
            var userExists = 
                usernames.Contains(_clientId);

            if (userExists)
            {
                DuplicateName?.Invoke();
                Dispose(true);
                return;
            }

            await _mqttClient
                .UnsubscribeAsync(USERNAMES_TOPIC);

            usernames.Add(_clientId);
            await _mqttClient.PublishAsync(new MqttApplicationMessage
            {
                Topic   = USERNAMES_TOPIC,
                Payload = JsonSerializer.SerializeToUtf8Bytes(usernames),
                Retain  = true
            });
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
                    _clientId          = null;
                    _mqttClientOptions = null;
                }

                _connectionTokenSource?.Dispose();
                using (_mqttClient)
                    await _mqttClient.DisconnectAsync();

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
