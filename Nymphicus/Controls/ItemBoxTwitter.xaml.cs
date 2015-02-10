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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Nymphicus.Model;
using Nymphicus.UserInterface;
using Nymphicus.ExternalServices;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class ItemBoxTwitter : UserControl
    {
        class externalServiceParameter
        {
            public Nymphicus.ExternalServices.IStore storeService { get; set; }
            public string link { get; set; }
            public TwitterItem item { get; set; }
        }

        public ItemBoxTwitter()
        {
            InitializeComponent();

            ButtonGeoLocation.Cursor = Cursors.Hand;
            ButtonUsernameAuthorRetweet.Cursor = Cursors.Hand;
            ButtonUsernameAuthor.Cursor = Cursors.Hand;
            ButtonConversation.Cursor = Cursors.Hand;
            ButtonDMConversation.Cursor = Cursors.Hand;
            ButtonDeleteTweet.Cursor = Cursors.Hand;

            ContextMenu contextMenuItem = new ContextMenu();

            MenuItem menuItemCopyTweet = new MenuItem();
            menuItemCopyTweet.Header = "Copy tweet text";
            menuItemCopyTweet.Click += new RoutedEventHandler(menuItemCopyTweet_Click);

            contextMenuItem.Items.Add(menuItemCopyTweet);

            MenuItem menuItemRetweetMenu = new MenuItem();
            menuItemRetweetMenu.Header = "Retweet...";
            BitmapImage retweetIcon = new BitmapImage(new Uri(@"/Nymphicus;component/Images/16px/retweet.png", UriKind.Relative));;
            menuItemRetweetMenu.Icon = new System.Windows.Controls.Image { Source = retweetIcon };

            MenuItem menuItemTraditionalRetweet = new MenuItem();
            menuItemTraditionalRetweet.Header = "Traditional retweet";
            menuItemTraditionalRetweet.CommandParameter = this.DataContext;
            menuItemTraditionalRetweet.Click += new RoutedEventHandler(menuItemTraditionalRetweet_Click);
            menuItemRetweetMenu.Items.Add(menuItemTraditionalRetweet);
            contextMenuItem.Items.Add(menuItemRetweetMenu);

            foreach (AccountTwitter account in AppController.Current.AllTwitterAccounts)
            {
                MenuItem menuItemRetweet = new MenuItem();
                menuItemRetweet.Header = "Retweet with @" + account.Login.Username;
                menuItemRetweet.CommandParameter = account;
                BitmapImage imageAvatar = new BitmapImage(new Uri(account.Login.Avatar, UriKind.Absolute));
                imageAvatar.DecodePixelWidth = 16;
                imageAvatar.DecodePixelHeight = 16;
                imageAvatar.CacheOption = BitmapCacheOption.OnDemand;
                System.Windows.Controls.Image imageAvatarFinal = new System.Windows.Controls.Image { Source = imageAvatar };
                imageAvatarFinal.Width = 16.0;
                imageAvatarFinal.Height = 16.0;  
                menuItemRetweet.Icon = imageAvatarFinal;
                menuItemRetweet.Click += new RoutedEventHandler(menuItemRetweet_Click);
                menuItemRetweetMenu.Items.Add(menuItemRetweet);
            }
            
            /*
            MenuItem contextExternalServices = new MenuItem();
            contextExternalServices.Header = "Send to...";

            foreach (Nymphicus.ExternalServices.IStore service in AppController.Current.AllIStores)
            {
                MenuItem menuItemIStoreEntry = new MenuItem();
                menuItemIStoreEntry.Header = service.Name;
                externalServiceParameter parameter = new externalServiceParameter();
                parameter.storeService = service;

                parameter.item = this.DataContext as TwitterItem;
                menuItemIStoreEntry.CommandParameter = parameter;
                BitmapImage serviceImage = new BitmapImage(new Uri(service.ServiceIcon));
                serviceImage.DecodePixelWidth = 16;
                serviceImage.CacheOption = BitmapCacheOption.OnLoad;
                menuItemIStoreEntry.Icon = new System.Windows.Controls.Image { Source = serviceImage };

                menuItemIStoreEntry.Click += new RoutedEventHandler(menuItemIStoreEntry_Click);
                contextExternalServices.Items.Add(menuItemIStoreEntry);
            }

            contextMenuItem.Items.Add(contextExternalServices); */

            this.GridMainItem.ContextMenu = contextMenuItem;
        }

        void menuItemCopyTweet_Click(object sender, RoutedEventArgs e)
        {
            TwitterItem item = this.DataContext as TwitterItem;
            if (item != null)
            {
                try
                {
                    if (item.RetweetedItem != null)
                    {
                        Clipboard.SetText(item.RetweetedItem.Text);
                    }
                    else
                    {
                        Clipboard.SetText(item.Text);
                    }
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile("Coping to clipboard failed");
                    AppController.Current.Logger.writeToLogfile(exp, true);
                }
            }
        }

        void menuItemIStoreEntry_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                externalServiceParameter parameter = menuItem.CommandParameter as externalServiceParameter;
                parameter.storeService.SendNow(this.DataContext as TwitterItem, null);
            }
        }

        void menuItemRetweet_Click(object sender, RoutedEventArgs e)
        {
            TwitterItem item = this.DataContext as TwitterItem;
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                AccountTwitter account = menuItem.CommandParameter as AccountTwitter;
                if (account != null)
                {
                    AppController.Current.retweet(account, item);
                }
            }
        }

        void menuItemTraditionalRetweet_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = this.DataContext as TwitterItem;
                if (item != null)
                {
                    UserInterface.ComposeNewTweet composeWindow = new UserInterface.ComposeNewTweet();
                    composeWindow.textBoxTweet.Text = "RT @" + item.Author.Username + " " + item.Text;
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    if (account != null)
                    {
                        composeWindow.selectAccount(account);
                    }
                    composeWindow.Show();
                }
            }
        }

        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Storyboard story;
                story = (Storyboard)this.FindResource("FadeIn");
                if (story != null)
                {
                    story.Begin();
                }
                else
                {
                    this.wrapPanelAvatarOverlay.Opacity = 0.75;
                }
            }
            catch
            {
                try
                {
                    this.wrapPanelAvatarOverlay.Opacity = 0.75;
                }
                catch
                {
                    AppController.Current.Logger.writeToLogfile("wrapPanelAvatar failed");
                }
            }
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Storyboard story;
                story = (Storyboard)this.FindResource("FadeAway");
                if (story != null)
                {
                    story.Begin();
                }
                else
                {
                    try
                    {
                        this.wrapPanelAvatarOverlay.Opacity = 0;
                    }
                    catch
                    {
                        AppController.Current.Logger.writeToLogfile("wrapPanelAvatar failed");
                    }
                }
            }
            catch
            {
                this.wrapPanelAvatarOverlay.Opacity = 0;
            }
        }

        private void buttonRetweet_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    if (AppController.Current.AllTwitterAccounts.Count == 1)
                    {
                        AppController.Current.retweet(AppController.Current.AllTwitterAccounts[0] as AccountTwitter, item);
                    }
                    else
                    {
                        AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                        if (account == null)
                        {
                            AppController.Current.retweet(AppController.Current.AllTwitterAccounts[0] as AccountTwitter, item);
                        }
                        AppController.Current.retweet(account, item);
                    }
                }
            }
        }

        private void buttonReply_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    if (!item.isDirectMessage)
                    {
                        AppController.Current.openComposeNewTweet(account, item);
                    }
                    else
                    {
                        AppController.Current.openComposeNewTweet(account, item.Author.Username);
                    }
                }
            }
        }

        private void buttonDM_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    AppController.Current.openComposeNewTweet(account, item.Author.Username);
                }
            }
        }

        private void buttonFavorit_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AppController.Current.favoriteItem(item);
                }
            }
        }

        private void ButtonUsernameAuthor_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    ShowUser userInfo = new ShowUser();
                    userInfo.setPerson(item.Author, item.RetrievingAccount);
                    userInfo.Show();
                }
            }
        }

        private void ButtonUsernameAuthorRetweet_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    if (item.RetweetedItem != null)
                    {
                        ShowUser userInfo = new ShowUser();
                        userInfo.setPerson(item.RetweetedItem.Author, item.RetrievingAccount);
                        userInfo.Show();
                    }
                }
            }
        }

        private void ButtonGeoLocation_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    try
                    {
                        if (Properties.Settings.Default.LocationMapService == "OpenStreetMap")
                        {
                            System.Diagnostics.Process.Start("http://open.mapquestapi.com/nominatim/v1/search.php?q=" + System.Web.HttpUtility.UrlEncode(item.Place.FullLocation));
                        }
                        else
                        {
                            System.Diagnostics.Process.Start("http://maps.google.de/maps?q=" + System.Web.HttpUtility.UrlEncode(item.Place.FullLocation));
                        }
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
            }
        }

        private void ButtonUsernameDMReceipient_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    if (item.DMReceipient != null)
                    {
                        ShowUser userInfo = new ShowUser();
                        userInfo.setPerson(item.DMReceipient, item.RetrievingAccount);
                        userInfo.Show();
                    }
                }
            }
        }


        private void ImageAvatar_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                BitmapImage defaultAvatar = new BitmapImage(new Uri(@"/Nymphicus;component/Images/32px/user.png", UriKind.Relative));
                if (defaultAvatar != null)
                {
                    ImageAvatar.Source = defaultAvatar;
                    ImageAvatar.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    ImageAvatar.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                }
                else
                {
                    AppController.Current.Logger.writeToLogfile("Null default Twitter avatar");
                }

            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
        }

        private void MenuItemBlockUser_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;
                if (item != null)
                {
                    if (item.Author != null)
                    {
                        Filter filter = new Filter();
                        filter.Name = "@" + item.Author.Username;
                        filter.IsExcludeFilter = true;
                        filter.FilterAuthor = true;
                        filter.FilterText = false;
                        filter.FilterString = item.Author.Username;
                        AppController.Current.AllFilters.Add(filter);
                        View view = AppController.Current.mainWindow.comboBoxViews.SelectedItem as View;
                        if (view != null)
                        {
                            view.subscribeToFilter(filter);
                        }
                    }
                }
            }
        }

        private void MenuItemTwitterBlockUser_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    if (item.Author != null && account != null)
                    {
                        AppController.Current.BlockUser(account, item.Author);
                    }
                }
            }
        }


        private void MenuItemReportUser_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    if (item.Author != null && account != null)
                    {
                        AppController.Current.ReportUser(account, item.Author);
                    }
                }
            }
        }

        private void menuItemList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonConversation_Click(object sender, RoutedEventArgs e)
        {
             Button button = sender as Button;
             if (button != null)
             {
                 TwitterItem item = button.CommandParameter as TwitterItem;
                 if (item != null)
                 {
                     UserInterface.Twitter.ShowConversationWindow window = new UserInterface.Twitter.ShowConversationWindow(item);
                 }
             }
        }

        private void ButtonDeleteTweet_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    if (item.IsOwnTweet)
                    {
                        item.DeleteThisTweet();
                    }
                }
            }
        }

        private void ButtonEditTweet_Click(object sender, RoutedEventArgs e)
        {
               Button button = sender as Button;
               if (button != null)
               {
                   TwitterItem item = button.CommandParameter as TwitterItem;
                   if (item != null)
                   {
                       UserInterface.ComposeNewTweet editTweetWindow = new ComposeNewTweet();
                       editTweetWindow.setToBeEditedItem(item);
                       editTweetWindow.Show();
                   }
               }
        }

        private void buttonToggleReadState_Click(object sender, RoutedEventArgs e)
        {
               Button button = sender as Button;
               if (button != null)
               {
                   TwitterItem item = button.CommandParameter as TwitterItem;
                   if (item != null)
                   {
                       if (item.IsRead)
                       {
                           item.markUnread();
                       }
                       else
                       {
                           item.markRead();
                       }
                   }
               }
        }

        private void contextMarkReadAllOlderInView_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;
                if (item != null)
                {
                    ThreadSaveObservableCollection<TwitterItem> AllTwitterItemsInView = new ThreadSaveObservableCollection<TwitterItem>();
                    foreach (IItem iitem in AppController.Current.mainWindow.ListOfItems.listView_Tweets.Items)
                    {
                        if (iitem != null)
                        {
                            if (iitem.GetType() == typeof(TwitterItem))
                            {
                                AllTwitterItemsInView.Add(iitem as TwitterItem);
                            }
                        }
                    }
                    foreach (TwitterItem twitterItem in AllTwitterItemsInView)
                    {
                        if (twitterItem != null)
                        {
                            if (twitterItem.Id <= item.Id) 
                            {
                                twitterItem.markRead();
                                GeneralFunctions.ReadStates.AddTweetAsBeingRead(twitterItem.Id);
                            }
                        }
                    }
                }
            }
        }

        private void contextMarkReadAllOlderInAccount_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    if (account != null)
                    {
                        foreach (TwitterItem twitterItem in account.AllItems)
                        {
                            if (twitterItem != null)
                            {
                                if (twitterItem.Id <= item.Id)
                                {
                                    twitterItem.markRead();
                                    GeneralFunctions.ReadStates.AddTweetAsBeingRead(twitterItem.Id);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void contextMarkUnReadAllInView_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;

                    ThreadSaveObservableCollection<TwitterItem> AllTwitterItemsInView = new ThreadSaveObservableCollection<TwitterItem>();
                    foreach (IItem iitem in AppController.Current.mainWindow.ListOfItems.listView_Tweets.Items)
                    {
                        if (iitem.GetType() == typeof(TwitterItem))
                        {
                            AllTwitterItemsInView.Add(iitem as TwitterItem);
                        }
                    }
                    foreach (TwitterItem twitterItem in AllTwitterItemsInView)
                    {
                        if (twitterItem != null)
                        {
                            twitterItem.markUnread();
                                GeneralFunctions.ReadStates.RemoveTweetAsBeingRead(twitterItem.Id);
                        }
                    }
                
            }
        }

        private void contextMarkUnReadAllInAccount_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = menuItem.CommandParameter as TwitterItem;
                if (item != null)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(item.accountId);
                    if (account != null)
                    {
                        foreach (TwitterItem twitterItem in account.AllItems)
                        {
                            if (twitterItem != null)
                            {
                                twitterItem.markUnread();
                                    GeneralFunctions.ReadStates.RemoveTweetAsBeingRead(twitterItem.Id);
                            }
                        }
                    }
                }
            }

        }

        private void buttonHumanReadableAgo_Click_1(object sender, RoutedEventArgs e)
        {
            TwitterItem item = this.DataContext as TwitterItem;
            if (item != null)
            {
                try
                {

                    System.Diagnostics.Process.Start(item.itemPermaLink);
                }
                catch { }
            }
        }

        private void button_embeddedImage_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                try
                {
                    string url = button.CommandParameter as string;
                    System.Diagnostics.Process.Start(url);
                }
                catch { }
            }
        }

    }
}
