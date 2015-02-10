using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Nymphicus.Model;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for ShowUser.xaml
    /// </summary>
    public partial class ShowUser : Window
    {
        public Person MyPerson { get; set; }

        public ShowUser()
        {
            InitializeComponent();
            userInfoBoxTwitter.Visibility = Visibility.Collapsed;
            userInfoBoxQuoteFm.Visibility = System.Windows.Visibility.Collapsed;

            if (AppController.Current.mainWindow != null)
            {
                this.Top = AppController.Current.mainWindow.Top;
                if (AppController.Current.mainWindow.Left > this.Width + 5)
                {
                    this.Left = AppController.Current.mainWindow.Left - this.Width - 5;
                }
                else
                {
                    this.Left = AppController.Current.mainWindow.Left + AppController.Current.mainWindow.Width + 5;

                }
            }
        }

        public void setPerson(Person person, AccountTwitter account)
        {
            if (person.isSearchUser)
            {
                TweetSharp.GetUserProfileForOptions options = new TweetSharp.GetUserProfileForOptions();
                options.ScreenName = person.Username;
                TweetSharp.TwitterUser user = account.twitterService.GetUserProfileFor(options);

                
                if (user != null)
                {
                    person = API.TweetSharpConverter.getPersonFromUser(user, account);
                }
            }
            MyPerson = person;
            userInfoBoxTwitter.Visibility = System.Windows.Visibility.Visible;
            userInfoBoxTwitter.DataContext = MyPerson;
            if (person != null)
            {

                userInfoBoxTwitter.createFriendshipOverview(person.Username);

                MyPerson.UpdateRecentTweets();
                listBoxItems.listView_Tweets.ItemsSource = MyPerson.RecentTweets;

                this.Title = person.NameAndLogin;
            }
        }

        public void setPerson(QuoteFmUser person)
        {
  
            userInfoBoxQuoteFm.DataContext = person;
            userInfoBoxQuoteFm.Visibility = System.Windows.Visibility.Visible;
            gridLeft.Width = new GridLength(280.0);
            if (person != null)
            {
                person.updateRecommendations();
                listBoxItems.listView_Tweets.ItemsSource = person.Recommendations;

                if(!string.IsNullOrEmpty(person.Fullname)) {
                    this.Title = person.Fullname;
                }
                else
                {
                    this.Title = person.username;
                }
            }
        }


        void image_DownloadCompleted(object sender, EventArgs e)
        {
            try
            {
                BitmapImage bitmapImage = (BitmapImage)sender;
                bitmapImage.Freeze();
                bitmapImage.DecodePixelHeight = 32;
                
                if (bitmapImage != null)
                {
                    
                    this.Icon = bitmapImage;
                }
            }
            catch
            {
                // Dann halt nicht...
            }
        }



    }
}
