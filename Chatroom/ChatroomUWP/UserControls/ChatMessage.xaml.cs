using ChatroomUWP.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace ChatroomUWP.UserControls
{
    public enum Position
    {
        Left, 
        Right
    };

    /// <summary>
    /// Represents a chat message.
    /// </summary>
    public sealed partial class ChatMessage : UserControl
    {
        #region Public properties

        /// <summary>
        /// The name of the user that sent the
        /// message.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The body of the message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The time at which the message was sent.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Tells whether the header is visible or not.
        /// </summary>
        public bool HeaderVisible
        {
            get;
            set;
        } = true;

        private Position _position;
        /// <summary>
        /// The control's position.
        /// </summary>
        public Position Position
        {
            get => _position;
            set
            {
                _position = value;

                HorizontalAlignment =
                    value == Position.Left
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Right;

                _contentGrid.CornerRadius = value switch
                {
                    Position.Left => new CornerRadius(12)
                    {
                        TopLeft = 0
                    },
                    Position.Right => new CornerRadius(12)
                    {
                        TopRight = 0
                    },
                    _ => throw new Exception("Wrong position")
                };
            }
        }

        #endregion


        #region Constructor
        public ChatMessage()
        {
            InitializeComponent();
        }
        #endregion


        #region Conversions

        public static implicit operator ChatMessage(ChatroomMessage message) => new ChatMessage
        {
            Username  = message.Username,
            Body      = message.Contents,
            Timestamp = message
                .Timestamp
                .ToShortTimeString()
        };

        #endregion
    }
}
