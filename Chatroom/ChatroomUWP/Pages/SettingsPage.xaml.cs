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


namespace ChatroomUWP.Pages
{
    public sealed partial class SettingsPage : Page
    {
        #region Constructor
        public SettingsPage()
        {
            InitializeComponent();
            InitializeThemeOptions();
        }
        #endregion


        #region Theme
        private void InitializeThemeOptions()
        {
            var appTheme = MainPage.AppTheme;
            var buttons  = _themePanel
                .Children
                .Cast<RadioButton>()
                .ToList();

            switch (appTheme)
            {
                case ElementTheme.Light:   buttons[0].IsChecked = true; break;
                case ElementTheme.Dark:    buttons[1].IsChecked = true; break;
                case ElementTheme.Default: buttons[2].IsChecked = true; break;
            }

            buttons.ForEach(btn =>
            {
                btn.Checked += SetTheme; 
            });
        }
        #endregion

        #region Set theme
        /// <summary>
        /// Sets the app theme.
        /// </summary>
        private void SetTheme(object sender, RoutedEventArgs e)
        {
            var themeButton = sender as RadioButton;

            MainPage.AppTheme = themeButton.Tag switch
            {
                "light"   => ElementTheme.Light,
                "dark"    => ElementTheme.Dark,
                "default" => ElementTheme.Default
            };
        }
        #endregion


        #region On navigated from
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _themePanel
                .Children
                .Cast<RadioButton>()
                .ToList()
                .ForEach(btn => btn.Checked -= SetTheme);
        }
        #endregion
    }
}
