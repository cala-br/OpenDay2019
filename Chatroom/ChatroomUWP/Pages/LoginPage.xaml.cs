using ChatroomUWP.Classes;
using System;
using System.Text.RegularExpressions;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


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
            _client.Connected += ConnectedHandler;
            _client.ConnectionError += ConnectionErrorHandler;
        }
        #endregion


        #region Try join
        /// <summary>
        /// Tries to join the chat.
        /// </summary>
        private async void TryJoin(object sender, RoutedEventArgs e)
        {
            string username = _usernameBox.Text;
            bool isInvalid  =
                string.IsNullOrWhiteSpace(username);

            if (!isInvalid)
            {
                _usernameBox.IsEnabled = false;
                await _client.Join(username);
            }
        }
        #endregion


        #region Show error
        /// <summary>
        /// Displays an error.
        /// </summary>
        private void ShowError(string error)
        {
            new Flyout
            {
                Content = new TextBlock
                {
                    Text    = error,
                    Opacity = 0.8,
                    Style   = Resources["CaptionTextBlockStyle"] as Style
                }
            }
            .ShowAt(_contentPanel);
        }
        #endregion

        #region Duplicate name handler
        /// <summary>
        /// Re-asks the username.
        /// </summary>
        private async void DuplicateNameHandler()
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _usernameBox.IsEnabled = true;
                ShowError("Questo nome esiste già");
            });
        }
        #endregion

        #region Connection error handler
        /// <summary>
        /// Handles connection errors.
        /// </summary>
        private async void ConnectionErrorHandler()
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _usernameBox.IsEnabled = true;
                ShowError("Errore di connessione al server.");
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
                    .Navigate(
                        typeof(ChatPage),
                        ChatroomClient.GENERAL_ROOM_TOPIC);
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
            _client.Connected -= ConnectedHandler;
            _client.ConnectionError -= ConnectionErrorHandler;
        }
        #endregion


        #region Username box - key up
        /// <summary>
        /// Enables or disables the login button.
        /// </summary>
        private void UsernameBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                TryJoin(null, null);

            _joinButton.IsEnabled =
                Regex.IsMatch(_usernameBox.Text, @".+");
        }
        #endregion
    }
}
