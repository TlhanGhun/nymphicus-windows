using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TweetSharp;
using System.ComponentModel;
using System.Windows.Threading;



    namespace Nymphicus.Model
    {
        public interface IAccount
        {
   
            ObservableCollection<SubscribableItemsCollection> subscribableItemCollections { get; set; }
            decimal Id { get; }
            string username { get; }
            Color accountColor { get; set; }
            System.Windows.Media.SolidColorBrush accountBrush { get; }
            
            bool LoginSuccessfull { get; set; }
            string Avatar { get; set; }
            List<string> AvailableNotificationClasses { get; }
            string AccountType { get; }
            string DebugText { get; }
            
            void registerAccount();

            void UpdateItems();
            string getStorableSettings();
            void readStorableSettings(string storedSettingsString);
            bool verifyCredentials();
        }
    }
