using ChatroomUWP.Classes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace ChatroomUWP.Pages
{
    /// <summary>
    /// Handles the navigation.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Private fields

        private ChatroomClient _client =
            ChatroomClient.GetInstance();

        #endregion

        #region Public properties

        /// <summary>
        /// The instance of this page.
        /// </summary>
        private static MainPage _instance { get; set; }

        /// <summary>
        /// Used to navigate between pages.
        /// </summary>
        public static Frame NavigationFrame => _instance._contentFrame;

        /// <summary>
        /// The application's theme.
        /// </summary>
        public static ElementTheme AppTheme
        {
            get => _instance.RequestedTheme;
            set => _instance.RequestedTheme = value;
        }

        /// <summary>
        /// The main navigation view.
        /// </summary>
        public static NavigationView NavigationView => _instance._navView;

        #endregion


        #region Constructor
        public MainPage()
        {
            InitializeComponent();

            _instance = this;
            _firstItem.Tag =
                ChatroomClient.GENERAL_ROOM_TOPIC;
        }
        #endregion


        #region Display selected page
        /// <summary>
        /// Displays the requested page.
        /// </summary>
        private void DisplaySelectedPage(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                _contentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            NavigationViewItem item = args
                .SelectedItem as NavigationViewItem;

            string tag = item
                .Tag as string;

            if (tag.StartsWith(ChatroomClient.GENERAL_ROOM_TOPIC))
            {
                _contentFrame.Navigate(
                    _client.IsLoggedIn
                    ? typeof(ChatPage)
                    : typeof(LoginPage), tag);
            }
        }
        #endregion
    }
}
