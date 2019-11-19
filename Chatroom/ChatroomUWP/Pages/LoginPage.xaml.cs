using ChatroomUWP.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// Manages the login to the chatroom.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        #region Private fields

        private ChatroomClient _client;

        #endregion

        #region Constructor
        public LoginPage()
        {
            InitializeComponent();

            _client = ChatroomClient.GetInstance();
            _client.DuplicateName += DuplicateNameHandler;
            _client.Connected     += ConnectedHandler;
        }
        #endregion


        #region Try login
        /// <summary>
        /// Tries to login.
        /// </summary>
        private void TryLogin(object sender, RoutedEventArgs e)
        {
            var username  = _usernameBox.Text;
            var isInvalid =
                string.IsNullOrWhiteSpace(username);

            if (!isInvalid)
            {
                _usernameBox.IsEnabled = false;
                _client.Begin(username);
            }
        }
        #endregion

        #region Duplicate name
        /// <summary>
        /// Re-asks the username.
        /// </summary>
        private async void DuplicateNameHandler()
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _usernameBox.IsEnabled = true;
                _client = ChatroomClient.GetInstance();
            });
        }
        #endregion

        #region Connected
        /// <summary>
        /// Continues.
        /// </summary>
        private async void ConnectedHandler()
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MainPage
                    .NavigationFrame
                    .Navigate(typeof(HomePage));
            });
        }
        #endregion


        #region On navigated from
        /// <summary>
        /// Unbinds the handlers.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _client.DuplicateName -= DuplicateNameHandler;
            _client.Connected     -= ConnectedHandler;
            base.OnNavigatedFrom(e);
        }
        #endregion
    }
}
