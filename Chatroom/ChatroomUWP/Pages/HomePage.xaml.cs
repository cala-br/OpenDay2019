﻿using ChatroomUWP.Classes;
using ChatroomUWP.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234238

namespace ChatroomUWP.Pages
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        #region Private fields

        private ChatroomClient _client = 
            ChatroomClient.GetInstance();

        #endregion


        #region Constructor
        public HomePage()
        {
            InitializeComponent();

            _client.MessageReceived += MessageReceivedHandler;
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
                ChatMessage cMsg = message;
                cMsg.Position =
                    message.Username == _client.Username
                    ? Position.Right
                    : Position.Left;

                DisplayChatMessage(cMsg);
            });
        }
        #endregion


        #region Send box - key pressed
        /// <summary>
        /// Sends the message when enter is pressed.
        /// </summary>
        private void SendBoxKeyPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                SendMessage(null, null);
        }
        #endregion

        #region Send message
        /// <summary>
        /// Sends the message.
        /// </summary>
        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            var text = _sendBox.Text;

            if (string.IsNullOrWhiteSpace(text))
                return;

            var msg = new ChatroomMessage
            {
                Contents = text,
                Timestamp = DateTime.Now
            };

            await _client.PublishAsync(
                ChatroomClient.GENERAL_ROOM_TOPIC, msg);

            _sendBox.Text = string.Empty;
        }
        #endregion


        #region Display chat message
        /// <summary>
        /// Displays a message.
        /// </summary>
        private void DisplayChatMessage(ChatMessage message)
        {
            var lastMessageSender = _chatMessages
                .Children
                .LastOrDefault(msg =>
                {
                    return (msg as ChatMessage)
                        .Username == message.Username;
                });

            message.HeaderVisible = 
                lastMessageSender == default;

            _chatMessages
                .Children
                .Add(message);
        }
        #endregion
    }
}
