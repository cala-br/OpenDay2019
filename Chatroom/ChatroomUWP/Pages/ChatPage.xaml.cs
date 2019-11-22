using ChatroomUWP.Classes;
using ChatroomUWP.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


namespace ChatroomUWP.Pages
{
    public sealed partial class ChatPage : Page
    {
        #region Private fields

        private ChatroomClient _client =
            ChatroomClient.GetInstance();

        #endregion

        #region Properties

        /// <summary>
        /// The topic of this page.
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// The other client's username.
        /// </summary>
        public string Sender { get; private set; }

        #endregion


        #region Constructor
        /// <summary>
        /// Adds the MQTT event handlers.
        /// </summary>
        public ChatPage()
        {
            InitializeComponent();

            _client.MessageReceived += MessageReceivedHandler;
            _client.Disconnected += ClientDisconnectedHandler;
            _client.Reconnected += ClientReconnectedHandler;
        }
        #endregion

        #region On navigated to
        /// <summary>
        /// Loads the messages stored in the manager.
        /// </summary>
        /// <param name="e">Contains the sender's topic</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Topic  = e.Content as string;
            Sender = Topic switch
            {
                ChatroomClient.GENERAL_ROOM_TOPIC 
                    => "Global chat",

                { } => Topic.Split('/').Last()
            };

            List<ChatroomMessage> messages =
                ChatroomMessagesManager.GetMessages(Topic);

            messages.ForEach(msg =>
            {
                DisplayChatMessage(msg);
            });
        }
        #endregion


        #region Client disconnected handler
        private void ClientDisconnectedHandler()
        {
            _sendBox.IsEnabled = false;
            _sendButton.IsEnabled = false;
        }
        #endregion

        #region Client reconnected handler
        private void ClientReconnectedHandler()
        {
            _sendBox.IsEnabled = true;
            _sendButton.IsEnabled = true;
        }
        #endregion


        #region Message set position
        /// <summary>
        /// Sets the position of the message.
        /// </summary>
        private void MessageSetPosition(ChatroomMessageControl message)
        {
            message.Position =
                message.Username == _client.Username
                ? Position.Right
                : Position.Left;
        }
        #endregion

        #region Message received
        /// <summary>
        /// Handles the received messages.
        /// </summary>
        private async void MessageReceivedHandler(string topic, ChatroomMessage message)
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Topic != ChatroomClient.GENERAL_ROOM_TOPIC)
                {
                    if (message.Username != Sender)
                        return;
                }

                DisplayChatMessage(message);
                return;
            });
        }
        #endregion


        #region Send box - ctrl + enter pressed
        /// <summary>
        /// Sends the message when ctrl + enter is pressed.
        /// </summary>
        private void SendBoxCtrlEnterPressed(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            SendMessage(null, null);
        }
        #endregion

        #region Send message
        /// <summary>
        /// Sends the message.
        /// </summary>
        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            string text = _sendBox.Text;

            if (string.IsNullOrWhiteSpace(text))
                return;

            ChatroomMessage msg = new ChatroomMessage
            {
                Contents = text,
                Timestamp = DateTime.Now
            };

            try
            {
                await _client.PublishAsync(
                    ChatroomClient.GENERAL_ROOM_TOPIC, msg);
            }
            catch { ClientDisconnectedHandler(); }

            _sendBox.Text = string.Empty;
        }
        #endregion


        #region Display chat message
        /// <summary>
        /// Displays a message.
        /// </summary>
        private void DisplayChatMessage(ChatroomMessageControl message)
        {
            MessageSetPosition(message);

            ChatroomMessageControl lastMessage = _chatMessages
                .Children
                .LastOrDefault() as ChatroomMessageControl;

            if (lastMessage != default)
            {
                message.HeaderVisible =
                    lastMessage.Username != message.Username;
            }

            _chatMessages
                .Children
                .Add(message);
        }
        #endregion


        #region On navigated from 
        /// <summary>
        /// Removes the handlers.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _client.MessageReceived -= MessageReceivedHandler;
            _client.Disconnected -= ClientDisconnectedHandler;
            _client.Reconnected -= ClientReconnectedHandler;
        }
        #endregion
    }
}
