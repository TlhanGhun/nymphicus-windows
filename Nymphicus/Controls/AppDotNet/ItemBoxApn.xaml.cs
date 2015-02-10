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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nymphicus.Model;
using Nymphicus.UserInterface;
using Nymphicus.ExternalServices;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class ItemBoxApn : UserControl
    {
        class externalServiceParameter
        {
            public Nymphicus.ExternalServices.IStore storeService { get; set; }
            public string link { get; set; }
            public IItem item { get; set; }
        }

        ApnItem shown_item
        {
            get
            {
                return this.DataContext as ApnItem;
            }
        }

        AppNetDotNet.Model.Post parent_post
        {
            get
            {
                if (this.shown_item != null)
                {
                    if (this.shown_item.apnItem != null)
                    {
                        if (this.shown_item.apnItem.repost_of != null)
                        {
                            return this.shown_item.apnItem.repost_of;
                        }
                        else
                        {
                            return this.shown_item.apnItem;
                        }
                    }
                }
                return null;
            }
        }

        public ItemBoxApn()
        {
            InitializeComponent();

            ContextMenu contextMenuItem = new ContextMenu();

            MenuItem menuItemCopyTweet = new MenuItem();
            menuItemCopyTweet.Header = "Copy text";
            menuItemCopyTweet.Click += new RoutedEventHandler(menuItemCopyTweet_Click);

            contextMenuItem.Items.Add(menuItemCopyTweet);

            MenuItem contextRepostItem = new MenuItem();
            contextRepostItem.Header = "Repost with...";

            MenuItem contextStarItem = new MenuItem();
            contextStarItem.Header = "Star with...";

            foreach (Model.AccountAppDotNet apnAccount in AppController.Current.AllApnAccounts)
            {
                BitmapImage accountImage = new BitmapImage(new Uri(apnAccount.Avatar));
                accountImage.DecodePixelWidth = 16;
                accountImage.CacheOption = BitmapCacheOption.OnLoad;

                MenuItem menuItemRepostEntry = new MenuItem();
                menuItemRepostEntry.Header = "@" + apnAccount.username;
                menuItemRepostEntry.CommandParameter = apnAccount;
                //menuItemRepostEntry.Icon = new System.Windows.Controls.Image { Source = accountImage };
                menuItemRepostEntry.Click += menuItemRepostEntry_Click;
                contextRepostItem.Items.Add(menuItemRepostEntry);

                MenuItem menuItemStarEntry = new MenuItem();
                menuItemStarEntry.Header = "@" + apnAccount.username;
                menuItemStarEntry.CommandParameter = apnAccount;
                //menuItemStarEntry.Icon = new System.Windows.Controls.Image { Source = accountImage };
                menuItemStarEntry.Click += menuItemStarEntry_Click;
                contextStarItem.Items.Add(menuItemStarEntry);
            }

            MenuItem menuItemQuotedRepostEntry = new MenuItem();
            menuItemQuotedRepostEntry.Header = "Quoted repost";
            menuItemQuotedRepostEntry.CommandParameter = null;
            menuItemQuotedRepostEntry.Click += menuItemRepostEntry_Click;
            contextRepostItem.Items.Add(menuItemQuotedRepostEntry);
            
            contextMenuItem.Items.Add(contextRepostItem);
            contextMenuItem.Items.Add(contextStarItem);

            MenuItem contextExternalServices = new MenuItem();
            contextExternalServices.Header = "Send to...";

            foreach (Nymphicus.ExternalServices.IStore service in AppController.Current.AllIStores)
            {
                MenuItem menuItemIStoreEntry = new MenuItem();
                menuItemIStoreEntry.Header = service.Name;
                externalServiceParameter parameter = new externalServiceParameter();
                parameter.storeService = service;

                parameter.item = this.DataContext as ApnItem;
                menuItemIStoreEntry.CommandParameter = parameter;
                BitmapImage serviceImage = new BitmapImage(new Uri(service.ServiceIcon));
                serviceImage.DecodePixelWidth = 16;
                serviceImage.CacheOption = BitmapCacheOption.OnLoad;
                menuItemIStoreEntry.Icon = new System.Windows.Controls.Image { Source = serviceImage };

                menuItemIStoreEntry.Click += new RoutedEventHandler(menuItemIStoreEntry_Click);
                contextExternalServices.Items.Add(menuItemIStoreEntry);
            }

            contextMenuItem.Items.Add(contextExternalServices);

            this.GridMainItem.ContextMenu = contextMenuItem;

        }

        void menuItemStarEntry_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if(menuItem != null) {
                AccountAppDotNet apnAccount = menuItem.CommandParameter as AccountAppDotNet;
                if (apnAccount != null && parent_post != null)
                {
                    AppNetDotNet.ApiCalls.Posts.star(apnAccount.accessToken, parent_post.id);
                }
            }
        }

        void menuItemRepostEntry_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                AccountAppDotNet apnAccount = menuItem.CommandParameter as AccountAppDotNet;
                if (parent_post != null)
                {
                    if (apnAccount != null)
                    {
                        AppNetDotNet.ApiCalls.Posts.repost(apnAccount.accessToken, parent_post.id);
                    }
                    else
                    {
                        UserInterface.ComposeNewApnPost composeWindow = new ComposeNewApnPost();
                        composeWindow.autoCompeteTextbox_post.textBoxContent.Text = " >> \"" + parent_post.text + "\" via @" + parent_post.user.username;
                        composeWindow.autoCompeteTextbox_post.textBoxContent.CaretIndex = 0;
                        composeWindow.Show();
                    }
                }
            }
        }

        void menuItemCopyTweet_Click(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                if (item.isReposted && item.apnItem != null)
                {
                    Clipboard.SetText(item.apnItem.repost_of.text);
                }
                else
                {
                    Clipboard.SetText(item.Text);
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







        private void menuItemList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GridMainItem_MouseEnter(object sender, MouseEventArgs e)
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
                AppController.Current.Logger.writeToLogfile("wrapPanelAvatar failed");
            }

        }

        private void GridMainItem_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Storyboard story;
                story = (Storyboard)this.FindResource("FadeAway");
                if (story != null)
                {
                    try
                    {
                        story.Begin();
                    }
                    catch
                    {
                        this.wrapPanelAvatarOverlay.Opacity = 0;
                    }
                }
                else
                {
                    this.wrapPanelAvatarOverlay.Opacity = 0;
                }
            }
            catch
            {
                AppController.Current.Logger.writeToLogfile("wrapPanelAvatar failed");
            }
        }






        private void ButtonAuthor_Click(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                AppNetDotNet.Model.User user;
                if (item.isPrivateMessage)
                {
                    user = item.apnMessage.user;
                }
                else
                {
                    user = item.apnItem.user;
                }
                UserInterface.Apn.UserInfo userInfo = new UserInterface.Apn.UserInfo(item.receivingAccount, user);
                userInfo.Show();
            }
        }

        private void buttonSource_Click_1(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.clientUrl))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(item.clientUrl);
                    }
                    catch { }
                }
            }
        }

        private void buttonStar_Click_1(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                if (item.receivingAccount != null)
                {
                    string itemId = item.apnItem.id;
                    bool isRepostedByYou = item.apnItem.you_reposted;
                    if (item.apnItem.repost_of != null)
                    {
                        itemId = item.apnItem.repost_of.id;
                        isRepostedByYou = item.apnItem.repost_of.you_reposted;
                    }
                    if (!item.apnItem.you_starred)
                    {
                        Tuple<AppNetDotNet.Model.Post, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Posts.star(item.receivingAccount.accessToken, itemId);
                        if (response.Item2.success)
                        {
                            item.apnItem.you_starred = true;
                        }
                        else
                        {
                            MessageBox.Show(response.Item2.errorDescription, response.Item2.errorMessage);
                        }

                    }
                    else
                    {
                        Tuple<AppNetDotNet.Model.Post, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Posts.unstar(item.receivingAccount.accessToken, itemId);
                        if (response.Item2.success)
                        {
                            item.apnItem.you_starred = false;
                        }
                        else
                        {
                            MessageBox.Show(response.Item2.errorDescription, response.Item2.errorMessage);
                        }
                    }
                }
            }
        }

        private void buttonReply_Click_1(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                if (item.isPrivateMessage)
                {
                    buttonPrivateMessage_Click_1(sender, e);
                }
                else
                {
                    UserInterface.ComposeNewApnPost composeWindow = new ComposeNewApnPost();
                    composeWindow.isReplyToItem(item);
                    composeWindow.Show();
                }
            }
        }

        private void buttonRepost_Click_1(object sender, RoutedEventArgs e)
        {
              ApnItem item = this.DataContext as ApnItem;
              if (item != null)
              {
                  if (item.receivingAccount != null)
                  {
                      string itemId = item.apnItem.id;
                      bool isRepostedByYou = item.apnItem.you_reposted;
                      if(item.apnItem.repost_of != null) {
                          itemId = item.apnItem.repost_of.id;
                          isRepostedByYou = item.apnItem.repost_of.you_reposted;
                      }
                      if (!isRepostedByYou)
                      {
                          Tuple<AppNetDotNet.Model.Post, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Posts.repost(item.receivingAccount.accessToken, itemId);
                          if (response.Item2.success)
                          {
                              item.apnItem.you_reposted = true;
                          }
                          else
                          {
                              MessageBox.Show(response.Item2.errorDescription, response.Item2.errorMessage);
                          }

                      }
                      else
                      {
                          Tuple<AppNetDotNet.Model.Post, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Posts.unrepost(item.receivingAccount.accessToken, itemId);
                          if (response.Item2.success)
                          {
                              item.apnItem.you_reposted = false;
                          }
                          else
                          {
                              MessageBox.Show(response.Item2.errorDescription, response.Item2.errorMessage);
                          }
                      }
                  }
              }
        }

        private void ButtonConversation_Click_1(object sender, RoutedEventArgs e)
        {
             Button button = sender as Button;
             if (button != null)
             {
                 ApnItem item = button.CommandParameter as ApnItem;
                 if (item != null)
                 {
                     UserInterface.Apn.ShowConversationWindow window = new UserInterface.Apn.ShowConversationWindow(item);
                 }
             }
        }

        private void ButtonRepostedAuthor_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ApnItem item = button.CommandParameter as ApnItem;
                if (item != null)
                {
                    AppNetDotNet.Model.Post repostedPost = item.apnItem.repost_of;
                    if (repostedPost != null)
                    {
                        UserInterface.Apn.UserInfo userInfo = new UserInterface.Apn.UserInfo(item.receivingAccount, repostedPost.user);
                        userInfo.Show();
                    }
                }
            }
        }


        #region Client

        private void contextMenuFilterClient_Click_1(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.ClientName))
                {
                    try
                    {
                        if (AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == item.ClientName.ToLower() && f.FilterClient).Count() > 0)
                        {
                            Filter existingFilter = AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == item.ClientName.ToString().ToLower() && f.FilterClient).First();
                            if (existingFilter != null)
                            {
                                if (!AppController.Current.CurrentView.subscribedFilter.Contains(existingFilter))
                                {
                                    AppController.Current.CurrentView.subscribeToFilter(existingFilter.Id);
                                }
                            }
                        }
                        else
                        {
                            Filter filter = new Filter();
                            filter.Name = "Mute client " + item.ClientName;
                            filter.FilterString = item.ClientName;
                            filter.FilterAuthor = false;
                            filter.FilterClient = true;
                            filter.FilterText = false;
                            filter.IsExcludeFilter = true;
                            filter.FilterRetweeter = false;
                            AppController.Current.AllFilters.Add(filter);
                            AppController.Current.CurrentView.subscribeToFilter(filter.Id);
                        }
                    }
                    catch { }
                }
            }
        }

        private void contextMenuShowClientHomepage_Click_1(object sender, RoutedEventArgs e)
        {
            ApnItem item = this.DataContext as ApnItem;
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.clientUrl))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(item.clientUrl);
                    }
                    catch { }
                }
            }
        }


        #endregion

        private void buttonPrivateMessage_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ApnItem item = button.CommandParameter as ApnItem;
                if (item != null)
                {
                    UserInterface.ComposeNewApnPost composeWindow = new ComposeNewApnPost();
                    string username = "";
                    if(item.isPrivateMessage) {
                        username = item.apnMessage.user.username;
                    }
                    else {
                        if(item.isReposted) {
                            username = item.apnItem.repost_of.user.username;
                        }
                        else
                        {
                            username = item.apnItem.user.username;
                    }
                    }
                    composeWindow.isPrivateMessage(username);
                    composeWindow.Show();
                }
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
