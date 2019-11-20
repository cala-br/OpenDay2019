﻿using ChatroomUWP.Classes;
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
        public static ElementTheme AppTheme => _instance.RequestedTheme;

        #endregion


        #region Constructor
        public MainPage()
        {
            InitializeComponent();

            _instance = this;
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

            var item = args
                .SelectedItem as NavigationViewItem;

            _contentFrame.Navigate(item.Tag switch
            {
                "home#page" => _client.IsLoggedIn 
                            ? typeof(HomePage) 
                            : typeof(LoginPage),

                _ => throw 
                    new Exception($"Page doesn't exist: {item.Tag}")
            });
        }
        #endregion
    }
}
