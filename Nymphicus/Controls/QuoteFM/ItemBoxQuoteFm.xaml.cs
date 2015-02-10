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
    public partial class ItemBoxQuoteFm : UserControl
    {
        class externalServiceParameter
        {
            public Nymphicus.ExternalServices.IStore storeService { get; set; }
            public string link { get; set; }
            public TwitterItem item { get; set; }
        }

        public ItemBoxQuoteFm()
        {
            InitializeComponent();

            ContextMenu contextMenuItem = new ContextMenu();

            MenuItem menuItemCopyTweet = new MenuItem();
            menuItemCopyTweet.Header = "Copy text";
            menuItemCopyTweet.Click += new RoutedEventHandler(menuItemCopyTweet_Click);

            contextMenuItem.Items.Add(menuItemCopyTweet);

        
            
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

            contextMenuItem.Items.Add(contextExternalServices);

            this.GridMainItem.ContextMenu = contextMenuItem;

 
            
        }

        void menuItemCopyTweet_Click(object sender, RoutedEventArgs e)
        {
            TwitterItem item = this.DataContext as TwitterItem;
            if (item != null)
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
            try {
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
            catch {
                 AppController.Current.Logger.writeToLogfile("wrapPanelAvatar failed");
            }

        }

        private void GridMainItem_MouseLeave(object sender, MouseEventArgs e)
        {
            try {
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
    

        private void buttonFacebook_Click(object sender, RoutedEventArgs e)
        {
            QuoteFmItem item = this.DataContext as QuoteFmItem;
            if (item != null)
            {
                UserInterface.Facebook.ComposeNewStatus newFbWindows = new UserInterface.Facebook.ComposeNewStatus(AppController.Current.AllFacebookAccounts.First());
                newFbWindows.textBoxMessage.Text = item.QuotedText;
                newFbWindows.textBoxLink.Text = item.ArticleLink;
                newFbWindows.Show();
            }
        }

        private void buttonTweetIt_Click(object sender, RoutedEventArgs e)
        {
            QuoteFmItem item = this.DataContext as QuoteFmItem;
            if (item != null)
            {
                UserInterface.ComposeNewTweet newTweetWindow = new ComposeNewTweet();
                newTweetWindow.textBoxTweet.textBoxContent.Text = item.QuotedText + " " + item.ArticleLink;
                newTweetWindow.Show();
            }
        }





        private void ButtonReadArticle_Click(object sender, RoutedEventArgs e)
        {
            QuoteFmItem item = this.DataContext as QuoteFmItem;
            if (item != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(item.ArticleLink);
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
        }

        private void ButtonAuthor_Click(object sender, RoutedEventArgs e)
        {
            QuoteFmItem item = this.DataContext as QuoteFmItem;
            if (item != null)
            {
                UserInterface.ShowUser showUser = new ShowUser();
                showUser.setPerson(item.Author);
                showUser.Show();
            }
        }

    }
}
