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

namespace Nymphicus.Controls.Facebook
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class CommentBoxFacebook : UserControl
    {
        class externalServiceParameter
        {
            public Nymphicus.ExternalServices.IStore storeService { get; set; }
            public string link { get; set; }
            public TwitterItem item { get; set; }
        }

        public CommentBoxFacebook()
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
                
                    Clipboard.SetText(item.Text);
                
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

        private void GridMainItem_MouseLeave(object sender, MouseEventArgs e)
        {
            Storyboard story;
            story = (Storyboard)this.FindResource("FadeAway");
            if (story != null)
            {
                story.Begin();
            }
            else
            {
                this.wrapPanelAvatarOverlay.Opacity = 0;
            }
        }

        private void buttonComment_Click(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                UserInterface.Facebook.ComposeNewComment commentWindow = new UserInterface.Facebook.ComposeNewComment(item);
                commentWindow.Show();
            }
        }

        private void buttonLike_Click(object sender, RoutedEventArgs e)
        {
            FacebookComment comment = this.DataContext as FacebookComment;
            if (comment != null)
            {
                comment.LikeThisComment();
            }
        }

        private void buttonApplication_Click(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                if (item.Application != null)
                {
                    if (!string.IsNullOrEmpty(item.Application.Link))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(item.Application.Link);
                        }
                        catch (Exception exp)
                        {
                            AppController.Current.Logger.writeToLogfile(exp);
                        }
                    }
                }
            }

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                if (item.PictureLink != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(item.PictureLink);
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
            }
        }

    }
}
