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

        #endregion


        #region Constructor
        public ChatMessage()
        {
            InitializeComponent();
        }
        #endregion
    }
}
