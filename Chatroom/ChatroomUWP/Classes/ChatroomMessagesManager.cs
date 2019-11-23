using ChatroomUWP.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ChatroomUWP.Classes
{
    public enum SectionCreationInfo
    {
        Existed,
        Created
    };

    static class ChatroomMessagesManager
    {
        #region Fields

        private static NavigationView
            _navView = MainPage.NavigationView;

        private static Dictionary<
            string,
            List<ChatroomMessage>> _messages = new Dictionary<string, List<ChatroomMessage>>();

        #endregion

        #region Properties

        /// <summary>
        /// Returns the Dispatcher used for cross-thread
        /// interface updates.
        /// </summary>
        private static CoreDispatcher Dispatcher
        {
            get => (Window.Current == null)
                ? CoreApplication.MainView.CoreWindow.Dispatcher
                : CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        }

        #endregion


        #region Dispatch
        /// <summary>
        /// Dispatches a message to the right chat
        /// based on a topic.
        /// Creates the chat section if it doesn't exist.
        /// </summary>
        public static async Task Dispatch(string topic, ChatroomMessage message)
        {
            SectionCreationInfo si = // Section Info
                await CreateSectionAsync(topic, message);

            if (si == SectionCreationInfo.Created)
                _messages.Add(topic, new List<ChatroomMessage>());

            _messages[topic].Add(message);
        }
        #endregion

        #region Get messages
        /// <summary>
        /// Returns the messages that have been
        /// received for the given topic.
        /// </summary>
        public static List<ChatroomMessage> GetMessages(string topic)
        {
            _messages.TryGetValue(
                topic,
                out List<ChatroomMessage> messages);

            return messages;
        }
        #endregion


        #region Section exists
        /// <summary>
        /// Checks if a section exists based on its tag.
        /// </summary>
        /// <param name="topic">The section's tag.</param>
        private static bool SectionExists(string topic)
        {
            NavigationViewItem section = _navView
                .MenuItems
                .Cast<NavigationViewItem>()
                .FirstOrDefault(i =>
                    (string)i.Tag == topic);

            return section != default;
        }
        #endregion

        #region Create section
        /// <summary>
        /// Creates a new section from a message.
        /// </summary>
        private static async Task<SectionCreationInfo> CreateSectionAsync(
            string topic,
            ChatroomMessage message
        )
        {
            if (SectionExists(topic))
                return SectionCreationInfo.Existed;

            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _navView
                    .MenuItems
                    .Append(new NavigationViewItem
                    {
                        Tag     = topic,
                        Content = message.Username,
                        Icon    = new SymbolIcon(Symbol.OtherUser)
                    });
            });

            return SectionCreationInfo.Created;
        }
        #endregion
    }
}
