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
    public partial class ItemBoxFacebook : UserControl
    {
        class externalServiceParameter
        {
            public Nymphicus.ExternalServices.IStore storeService { get; set; }
            public string link { get; set; }
            public TwitterItem item { get; set; }
        }

        public ItemBoxFacebook()
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

            CommentsCount.Cursor = Cursors.Hand;
            ButtonUsername.Cursor = Cursors.Hand;
            ButtonUsernameTo.Cursor = Cursors.Hand;
            imageInPost.Cursor = Cursors.Hand;
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
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                item.LikeThisItem();
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

        private void ButtonUsername_Click(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                UserInterface.Facebook.ShowUser showUser = new UserInterface.Facebook.ShowUser();
                showUser.setUser(item.User);
                showUser.Show();
            }
        }

        private void ButtoShowComments_Click(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                item.ShowComments = !item.ShowComments;
            }
        }

        private void listView_Comments_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ListBox listbox = sender as ListBox;
            if (listbox != null)
            {
                if (listbox.Visibility == System.Windows.Visibility.Visible)
                {
                    listbox.Items.Refresh();
                }
            }
        }

        private void ButtonUsernameTo_Click(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                if (item.To != null)
                {
                    UserInterface.Facebook.ShowUser showUser = new UserInterface.Facebook.ShowUser();
                    showUser.setUser(item.To);
                    showUser.Show();
                }
            }
        }

        private void imageInPost_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           
        }

     

        private void textblockCaption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.Link))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(item.Link);
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
            }
        }

        private void contextMenuFilterClient_Click_1(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                if (item.HasApplication)
                {
                    try {
                    if (AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == item.ApplicationName.ToLower() && f.FilterClient).Count() > 0)
                    {
                        Filter existingFilter = AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == item.ApplicationName.ToString().ToLower() && f.FilterClient).First();
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
                        filter.Name = "Mute client " + item.ApplicationName;
                        filter.FilterString = item.ApplicationName;
                        filter.FilterAuthor = false;
                        filter.FilterClient = true;
                        filter.FilterText = false;
                        filter.IsExcludeFilter = true;
                        filter.FilterRetweeter = false;
                        AppController.Current.AllFilters.Add(filter);
                        AppController.Current.CurrentView.subscribeToFilter(filter.Id);
                    }
                    }
                    catch {}
                }
            }
        }

        private void contextMenuShowClientHomepage_Click_1(object sender, RoutedEventArgs e)
        {
            FacebookItem item = this.DataContext as FacebookItem;
            if (item != null)
            {
                if (item.HasApplication)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(item.ApplicationLink);
                    }
                    catch { }
                }
            }
        }


    }
}
