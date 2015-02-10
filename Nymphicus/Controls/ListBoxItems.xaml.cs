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
using Nymphicus.Model;
using Nymphicus.UserInterface;
using System.ComponentModel;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for ListBoxItems.xaml
    /// </summary>
    public partial class ListBoxItems : UserControl
    {
        public ListBoxItems()
        {
            InitializeComponent();
            listView_Tweets.Items.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Descending));
        }

        
        public ListBoxItem CurrentlyTopMostShownItem { get; set; }

        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = sender as StackPanel;
            foreach (UIElement element in panel.Children)
            {
                if (element.GetType() == typeof(System.Windows.Controls.WrapPanel))
                {
                    element.Visibility = Visibility.Visible;
                }
            }
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            var panel = sender as StackPanel;
            foreach (UIElement element in panel.Children)
            {
                if (element.GetType() == typeof(System.Windows.Controls.WrapPanel))
                {
                    element.Visibility = Visibility.Hidden;
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
                    AppController.Current.openComposeNewTweet(null, item);
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
                    AppController.Current.openComposeNewTweet(null, item.Author.Username);
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

        private void listView_Tweets_MouseEnter(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null)
            {
                ScrollViewer scrollViewer = GetScrollViewer(listBox);
                if (scrollViewer != null)
                {
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                    if (listView_Tweets.HasItems)
                    {
                        try
                        {
                            if (Properties.Settings.Default.UseSmoothScrolling && scrollViewer.CanContentScroll == true)
                            {
                                scrollViewer.CanContentScroll = false;
                            }
                            else if (!Properties.Settings.Default.UseSmoothScrolling && scrollViewer.CanContentScroll == false)
                            {
                                scrollViewer.CanContentScroll = true;
                            } 
                        }
                        catch {
                            // wenn nicht, dann eben nicht... 
                        }
                    }
                }
            }
        }

        private ScrollViewer GetScrollViewer(ListBox listBox)
        {
            Border scroll_border = VisualTreeHelper.GetChild(listBox, 0) as Border;
            if (scroll_border is Border)
            {
                ScrollViewer scroll = scroll_border.Child as ScrollViewer;
                if (scroll is ScrollViewer)
                {
                    return scroll;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void listView_Tweets_MouseLeave(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null)
            {
                ScrollViewer scrollViewer = GetScrollViewer(listBox);
                if (scrollViewer != null)
                {
                    if (!Properties.Settings.Default.HideScrollbar)
                    {
                        scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    }
                    else
                    {
                        scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    }
                }
            }
        }

        private void listView_Tweets_SizeChanged(object sender, SizeChangedEventArgs e)
        {


        }

        private void ButtonUsernameAuthorRetweet_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                TwitterItem item = button.CommandParameter as TwitterItem;
                if (item != null)
                {
                    ShowUser userInfo = new ShowUser();
                    userInfo.setPerson(item.RetweetedItem.Author, item.RetweetedItem.RetrievingAccount);
                    userInfo.Show();
                }
            }
        }

        private void ContextTraditionalRetweet_Click(object sender, RoutedEventArgs e)
        {
            /// ttt
         /*   if (listView_Tweets.SelectedItems.Count == 1)
            {
                TwitterItem item = listView_Tweets.SelectedItem as TwitterItem;
                if (item != null)
                {
                    ComposeNewTweet composeWindows = new ComposeNewTweet();
                    composeWindows.textBoxTweet.Text = "RT @" + item.Author.Username + " " +  item.Text;
                    composeWindows.Show();
                }
            }
          * */
        }

        private void ContextReply_Click(object sender, RoutedEventArgs e)
        {
            //ttt
            /*
            if (listView_Tweets.SelectedItems.Count == 1)
            {
                TwitterItem item = listView_Tweets.SelectedItem as TwitterItem;
                if (item != null)
                {
                    AppController.Current.openComposeNewTweet(AppController.Current.getAccountForId(item.accountId), item);
                }
            }
             * */
        }

       /* public void ScrollToItem(object item)
        {
            if (item != null)
            {
                listView_Tweets.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                {
                    listView_Tweets.ScrollIntoView(item);
                }));
            }
        }
        * */

        
        public IItem GetCurrentlyTopmostShownItem()
        {
            if (listView_Tweets != null)
            {
                try
                {
                    HitTestResult hitTest = VisualTreeHelper.HitTest(listView_Tweets, new Point(10, 10));
                    System.Windows.Controls.ListBoxItem item = GetListViewItemFromEvent(null, hitTest.VisualHit) as System.Windows.Controls.ListBoxItem;
                    CurrentlyTopMostShownItem = item;
                    if (item != null)
                    {
                        return item.DataContext as IItem;
                    }
                }
                catch
                {
                    return null;
                }

            }
            return null;
        }
         

        public DateTime? GetCurrentlyTopmostShownDateTime()
        {
            IItem item = GetCurrentlyTopmostShownItem();
            if (item != null)
            {
                return item.CreatedAt;
            }
            else
            {
                return null;
            }
        }


        private System.Windows.Controls.ListBoxItem GetListViewItemFromEvent(object sender, object originalSource)
        {
            if (listView_Tweets != null && originalSource != null)
            {
                DependencyObject depObj = originalSource as DependencyObject;
                if (depObj != null)
                {
                    // go up the visual hierarchy until we find the list view item the click came from  
                    // the click might have been on the grid or column headers so we need to cater for this  
                    DependencyObject current = depObj;
                    while (current != null && current != listView_Tweets)
                    {
                        System.Windows.Controls.ListBoxItem ListViewItem = current as System.Windows.Controls.ListBoxItem;
                        if (ListViewItem != null)
                        {
                            return ListViewItem;
                        }
                        current = VisualTreeHelper.GetParent(current);
                    }
                }
            }
            return null;
        }

        private void listView_Tweets_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            return;
            if (AppController.Current != null)
            {
                if (AppController.Current.mainWindow != null)
                {
                    if (!AppController.Current.mainWindow.IsCollectionChange)
                    {
                        AppController.Current.mainWindow.TopMostShownItem = GetCurrentlyTopmostShownItem();
                    }
                    AppController.Current.mainWindow.IsCollectionChange = false;
                }
            }
        }

        private void listView_Tweets_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppController.Current != null)
            {
                if (AppController.Current.mainWindow != null)
                {
                    AppController.Current.mainWindow.TopMostShownItem = GetCurrentlyTopmostShownItem();
                }
            }
        }

        /// <summary>
        /// Oberstes Item soll sich nicht ändern - aber den Code gibt es in MainWindow.cs nochmal nahezu identisch.
        /// Daher hier mal aus der XAML rausgenommen, der folgende Code wird aktuell also nicht angesprungen!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_Tweets_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            return;
            if (AppController.Current.mainWindow.TopMostShownItem != null && listView_Tweets != null)
            {
                if (Properties.Settings.Default.DontScrollTopItemOutOfWindow)
                {
                    AppController.Current.mainWindow.ScrollToItemInListbox(AppController.Current.mainWindow.TopMostShownItem);
                }
            }
        }


   
    }
}
