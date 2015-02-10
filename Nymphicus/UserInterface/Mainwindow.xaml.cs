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
using TweetSharp;
using Nymphicus.Model;
using System.Timers;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace Nymphicus.UserInterface
{

    public partial class Mainwindow : Window
    {
        public static RoutedCommand OpenNewTweetWindow = new RoutedCommand();
        public static RoutedCommand OpenNewFacebookStatusWindows = new RoutedCommand();
        public static RoutedCommand SearchTwitter = new RoutedCommand();
        public static RoutedCommand OpenPreferences = new RoutedCommand();
        public static RoutedCommand Refresh = new RoutedCommand();
        public static RoutedCommand SelectView = new RoutedCommand();
        public static RoutedCommand SelectFilter = new RoutedCommand();
        
        public DispatcherTimer dispatcherTimer;

        private WindowState m_storedWindowState = WindowState.Normal;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        private System.Windows.Forms.ContextMenu m_notifyMenu;
        private bool isInRunningMode = false;

        public View lastOpenedView { get; set; }

        public IItem TopMostShownItem { get; set; }
        public bool IsCollectionChange { get; set; }
        public IItem ScrollToItem { get; set; }
        public Dictionary<decimal,DateTime> lastScrollToTimes { get; set; }

        public string filter_text { get; set; }
        public string filter_type { get; set; }

        public Mainwindow()
        {
            InitializeComponent();

            if (AppController.Current.isBetaVersion)
            {
                this.Title = "Nymphicus BETA";
            }

            lastScrollToTimes = new Dictionary<decimal, DateTime>();

            if (Properties.Settings.Default.WindowHeight != 0 && Properties.Settings.Default.WindowWidth != 0)
            {
                this.Height = Properties.Settings.Default.WindowHeight;
                this.Width = Properties.Settings.Default.WindowWidth;
                if (Properties.Settings.Default.WindowLocationX < System.Windows.Forms.SystemInformation.VirtualScreen.Width && Properties.Settings.Default.WindowLocationY < System.Windows.Forms.SystemInformation.VirtualScreen.Height)
                {
                    this.Left = Properties.Settings.Default.WindowLocationX;
                    this.Top = Properties.Settings.Default.WindowLocationY;
                }
                
            }

            #region tray icon stuff
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "Nymphicus";
            if (System.IO.File.Exists(System.IO.Path.Combine("Images", "NymphicusIcon.ico")))
            {
                m_notifyIcon.Icon = new System.Drawing.Icon("Images\\NymphicusIcon.ico");
                AppController.Current.Logger.writeToLogfile("No tray icon in " + System.IO.Path.Combine(AppController.Current.appProgramPath, "Images", "NymphicusIcon.ico").ToString(),true);
            }
            else if (System.IO.File.Exists(System.IO.Path.Combine("NymphicusIcon.ico")))
            {
                m_notifyIcon.Icon = new System.Drawing.Icon("NymphicusIcon.ico");
                AppController.Current.Logger.writeToLogfile("No tray icon in " + System.IO.Path.Combine(AppController.Current.appProgramPath, "NymphicusIcon.ico").ToString());
            }
            else
            {
                MessageBox.Show("Broken installation - minimize to tray won't work. It should be " + System.IO.Path.Combine(AppController.Current.appProgramPath, "NymphicusIcon.ico").ToString());
            }
            

            m_notifyIcon.DoubleClick += new EventHandler(m_notifyIcon_Click);
            System.Windows.Forms.MenuItem showMainMenu = new System.Windows.Forms.MenuItem("Show main window", new System.EventHandler(trayContextShow));

            m_notifyMenu = new System.Windows.Forms.ContextMenu();
            m_notifyMenu.MenuItems.Add("Nymphicus");
            m_notifyMenu.MenuItems.Add("-");
            m_notifyMenu.MenuItems.Add(showMainMenu);
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Refresh now", new System.EventHandler(trayContextRefresh)));
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Preferences", new System.EventHandler(trayContextPreferences)));
            m_notifyMenu.MenuItems.Add("-");
            if (AppController.Current.HasTwitterAccounts)
            {
                m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Compose new tweet", new System.EventHandler(trayContextComposeTweet)));
                m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Open Twitter search window", new System.EventHandler(trayContextTwitterSearch)));
            }
            if (AppController.Current.HasFacebookAccounts)
            {
                m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Write new Facebook status", new System.EventHandler(trayContextWriteNewFacebookStatus)));
            }
            m_notifyMenu.MenuItems.Add("-");
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Quit", new System.EventHandler(trayContextQuit)));

            m_notifyIcon.ContextMenu = m_notifyMenu;

            #endregion 

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);


            searchTextboxFilterItems.ExecuteSearch += new Controls.SearchTextbox.ExecuteSearchEventHandler(searchTextboxFilterItems_ExecuteSearch);
            searchTextboxFilterItems.SearchCleared += new Controls.SearchTextbox.SearchClearedEventHandler(searchTextboxFilterItems_SearchCleared);

            OpenNewTweetWindow.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            OpenNewFacebookStatusWindows.InputGestures.Add(new KeyGesture(Key.N,ModifierKeys.Control));
            SearchTwitter.InputGestures.Add(new KeyGesture(Key.S,ModifierKeys.Control));
            OpenPreferences.InputGestures.Add(new KeyGesture(Key.P,ModifierKeys.Control | ModifierKeys.Alt));
            Refresh.InputGestures.Add(new KeyGesture(Key.R,ModifierKeys.Control));
            SelectView.InputGestures.Add(new KeyGesture(Key.O,ModifierKeys.Control));
            SelectFilter.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));

            ScrollToItem = null;
            ListOfItems.listView_Tweets.Loaded += new RoutedEventHandler(listView_Tweets_Loaded);
            ListOfItems.listView_Tweets.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);

            checkWhichComposeButtonsToShow();
            AppController.Current.AllAccounts.CollectionChanged += AllAccounts_CollectionChanged;

            isInRunningMode = true;


        }

        void AllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            checkWhichComposeButtonsToShow();
        }

        private void checkWhichComposeButtonsToShow()
        {
            if (AppController.Current.HasApnAccounts)
            {
                buttonNewApnPost.Visibility = Visibility.Visible;
            }
            else
            {
                buttonNewApnPost.Visibility = Visibility.Collapsed;
            }

            if (AppController.Current.HasFacebookAccounts)
            {
                buttonNewFacebookStatus.Visibility = Visibility.Visible;
            }
            else
            {
                buttonNewFacebookStatus.Visibility = Visibility.Collapsed;
            }

            if (AppController.Current.HasTwitterAccounts)
            {
                buttonNewTweet.Visibility = Visibility.Visible;
                buttonSearch.Visibility = Visibility.Visible;
            }
            else
            {
                buttonNewTweet.Visibility = Visibility.Collapsed;
                buttonSearch.Visibility = Visibility.Collapsed;
            }
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ScrollToItem != null)
            {
                if (!ListOfItems.listView_Tweets.Items.Contains(ScrollToItem))
                {
                    ScrollToItem = ListOfItems.GetCurrentlyTopmostShownItem();
                    AppController.Current.Logger.addDebugMessage("Listbox did not contain the CurrentTopMostItem", "Selected new one (if item is empty none has been found", item: ScrollToItem, type: DebugMessage.DebugMessageTypes.ScrollIntoView, view: comboBoxViews.SelectedItem as View);
                }
                if (ScrollToItem != null)
                {
                    ScrollToItemInListbox(ScrollToItem);
                }
            }
            ScrollToItem = null;
        }

        void listView_Tweets_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppController.Current.CurrentView != null)
            {
                AppController.Current.CurrentView.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
            }
        }

        ~Mainwindow()
        {
            isInRunningMode = false;
        }

        void searchTextboxFilterItems_SearchCleared(object sender, EventArgs e)
        {
            filter_text = null;
            filter_update();
        }

        void searchTextboxFilterItems_ExecuteSearch(object sender, EventArgs e)
        {
            filter_text = searchTextboxFilterItems.textboxSearchString.Text;
            filter_update();
        }


        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            buttonRefresh_Click(null, null);
        }

        private void buttonNewTweet_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.openComposeNewTweet(null);
        }

        private void buttonPreferences_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.openPreferences();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.updateAllAccounts();
            //ListOfItems.listView_Tweets.Items.Refresh();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
            App.Current.Shutdown();
        }

        private void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(Properties.Settings.Default.ChosenView != "") {
                View view = AppController.Current.AllViews.Where(v => v.Name == Properties.Settings.Default.ChosenView).FirstOrDefault() as View;
                if (view != null)
                {
                    comboBoxViews.SelectedItem = view;
                }
            }

        }

        public void storeTweetMarkerOfCurrentSelectView() {

            if (lastOpenedView != null && AppController.Current.AllAccountsHaveInitialFetchDone && Properties.Settings.Default.UseTweetmarker)
            {
                AppController.Current.Logger.addDebugMessage("Storing TweetMarker of now unselected Views", lastOpenedView.Name, view: lastOpenedView, type: DebugMessage.DebugMessageTypes.TweetMarker);
                DateTime? newestShown = this.ListOfItems.GetCurrentlyTopmostShownDateTime();
                lastOpenedView.saveToTweetMarker(newestShown);
            }
            View view = comboBoxViews.SelectedItem as View;
            
            if (view != null)
            {
                if (lastOpenedView != view)
                {
                    if (lastOpenedView != null)
                    {
                        AppController.Current.Logger.addDebugMessage("removing old action on collection changed", lastOpenedView.Name, view: lastOpenedView, type: DebugMessage.DebugMessageTypes.TweetMarker);
                        lastOpenedView.Items.CollectionChanged -= Items_CollectionChanged;
                    }
                    view.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);

                    view.Items.CollectionChanged += Items_CollectionChanged;

                    updateScrollPositionDueToPositionMarker(view);

                    lastOpenedView = view;

                    AppController.Current.changeView(view);
                    ListOfItems.listView_Tweets.Items.Refresh();

                    Properties.Settings.Default.ChosenView = view.Name;
                }
            }
            
        }

        public void updateScrollPositionDueToPositionMarker(View view)
        {
            if (view == null)
            {
                view = this.comboBoxViews.SelectedItem as View;
                if (view == null)
                {
                    return;
                }
            }
            view.GetPositionMarkerItem();
        }

        /// <summary>
        /// Fired when the View combo box is used to select another View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsCollectionChange = true;
            if (TopMostShownItem != null && ListOfItems.listView_Tweets != null && Properties.Settings.Default.DontScrollTopItemOutOfWindow)
            {
                IsCollectionChange = true;
                ScrollToItemInListbox(TopMostShownItem);
                IsCollectionChange = true;
            }
        }

        private void comboBoxViews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         storeTweetMarkerOfCurrentSelectView();
           
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchResultsWindow searchWindow = new SearchResultsWindow(AppController.Current.AllTwitterAccounts[0]);
            searchWindow.Show();
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View selectedView = comboBoxViews.SelectedItem  as View;
            if (selectedView != null)
            {
                Properties.Settings.Default.LastUsedView = selectedView.Name;
                comboBoxViews_SelectionChanged(null, null);
            }
            Properties.Settings.Default.WindowWidth = this.Width;
            Properties.Settings.Default.WindowHeight = this.Height;
            Properties.Settings.Default.WindowLocationX = this.Left;
            Properties.Settings.Default.WindowLocationY = this.Top;
            Properties.Settings.Default.Save();

            if (lastOpenedView != null && Properties.Settings.Default.UseTweetmarker)
            {
                DateTime? topmostItemDateTime = this.ListOfItems.GetCurrentlyTopmostShownDateTime();
                if (topmostItemDateTime != null)
                {
                    lastOpenedView.saveToTweetMarker(topmostItemDateTime);
                }
            }

        }

        private void buttonNewFacebookStatus_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.Facebook.ComposeNewStatus composeWindow = new Facebook.ComposeNewStatus(null);
            composeWindow.Show();
        }

        


        #region Tray icon

        protected void trayContextShow(Object sender, System.EventArgs e)
        {
            m_notifyIcon_Click(null, null);
        }

        protected void trayContextRefresh(Object sender, System.EventArgs e)
        {
            buttonRefresh_Click(null, null);
        }


        protected void trayContextPreferences(Object sender, System.EventArgs e)
        {
            AppController.Current.openPreferences();
        }


        protected void trayContextComposeTweet(Object sender, System.EventArgs e)
        {
            buttonNewTweet_Click(null,null);
        }
        
        
        protected void trayContextTwitterSearch(Object sender, System.EventArgs e)
        {
            buttonSearch_Click(null,null);
        }
        
        
        protected void trayContextWriteNewFacebookStatus(Object sender, System.EventArgs e)
        {
            buttonNewFacebookStatus_Click(null,null);
        }


        protected void trayContextQuit(Object sender, System.EventArgs e)
        {
            Close();
        }

        void OnStateChanged(object sender, EventArgs args)
        {
            if (!isInRunningMode && !Properties.Settings.Default.StartMinimizedToTray)
            {
                return;
            }
            if (WindowState == WindowState.Minimized)
            {
                if (Properties.Settings.Default.MinimizeToTray)
                {
                    Hide();
                }
            }
            else if (WindowState != WindowState.Minimized)
            {
                ShowTrayIcon(false);
                m_storedWindowState = WindowState;
            }
        }
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        #endregion

        #region keyboard shortcuts

        private void OpenNewTweetWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonNewTweet_Click(null, null);
        }

        private void OpenNewFacebookStatusWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonNewFacebookStatus_Click(null, null);
        }

        
        private void SearchTwitterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonSearch_Click(null, null);
        }
        
        private void OpenPreferencesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonPreferences_Click(null,null);
        }
        
        private void RefreshExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonRefresh_Click(null, null);
        }

        private void SelectViewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            comboBoxViews.Focus();
            comboBoxViews.IsDropDownOpen = true;
        }

        private void SelectFilterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            searchTextboxFilterItems.textboxSearchString.Focus();
        }


        #endregion

        private void Window_Deactivated(object sender, EventArgs e)
        {
            AppController.Current.CurrentlyInBackground = true;
            ScrollToItem = ListOfItems.GetCurrentlyTopmostShownItem();
            TopMostShownItem = ScrollToItem;
            storeTweetMarkerOfCurrentSelectView();

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            AppController.Current.CurrentlyInBackground = false;
            this.Title = "Nymphicus";
            AppController.Current.NewItemsNotShownAlready = 0;
            clearOverlay();

        }

        public void setIconOverlayForNewItems(int number)
        {
            if (TaskbarManager.IsPlatformSupported)
            {

                System.Drawing.RectangleF rectF = new System.Drawing.RectangleF(0, 0, 40, 40);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(40, 40, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
                g.FillRectangle(System.Drawing.Brushes.DarkBlue, 0, 0, 40, 40);
                g.DrawString(number.ToString(), new System.Drawing.Font("Segoe UI", 20), System.Drawing.Brushes.White, new System.Drawing.PointF(0, 0));

                IntPtr hBitmap = bitmap.GetHbitmap();

                ImageSource wpfBitmap =
                    Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());


                System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
                TaskbarManager.Instance.SetOverlayIcon(icon , number.ToString());

            }
        }

        public void clearOverlay()
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.SetOverlayIcon(null, "");
            }
        }

        private void buttonDebugWindows_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.ShowDebugMessages debugWindow = new ShowDebugMessages();
            debugWindow.Show();
        }

        public void ScrollToItemInListbox(IItem item)
        {
            if (item == null)
            {
                return;
            }
            if (lastScrollToTimes.ContainsKey(item.Id))
            {
                DateTime? lastTimeScrolledTo = lastScrollToTimes[item.Id];
                if (lastTimeScrolledTo != null)
                {
                    if (lastTimeScrolledTo.Value.AddSeconds(2) > DateTime.Now)
                    {
                        return;
                    }
                }
            }
            lastScrollToTimes[item.Id] = DateTime.Now;

            //AppController.Current.sendNotification("General", "Scroll to item", item.Text, "", item);
            if (ListOfItems.listView_Tweets.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.GeneratingContainers)
            {
                AppController.Current.Logger.addDebugMessage("Scroll to item","ItemContainerGenerator is free",item:item,type:DebugMessage.DebugMessageTypes.ScrollIntoView);
                if(!ListOfItems.listView_Tweets.Items.Contains(item)) {
                    item = ListOfItems.GetCurrentlyTopmostShownItem();
                    AppController.Current.Logger.addDebugMessage("Scroll to item not displayed!","Initated retrieval of new top item",item:item,type:DebugMessage.DebugMessageTypes.ScrollIntoView);
                }
                if (ListOfItems.listView_Tweets.Items.Count < 2)
                {
                    return;
                }


                ListOfItems.listView_Tweets.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                {
                    try
                    {
                        //ttt
                        //ListOfItems.listView_Tweets.ScrollIntoView(ListOfItems.listView_Tweets.Items[ListOfItems.listView_Tweets.Items.Count - 1]);
                        //ListOfItems.listView_Tweets.ScrollIntoView(item);
                    }
                    catch { }
                }));
           
                
                ScrollToItem = item;
            }
            else
            {
                AppController.Current.Logger.addDebugMessage("Scroll to item","ItemContainerGenerator is in use",item:item,type:DebugMessage.DebugMessageTypes.ScrollIntoView);
                if(!ListOfItems.listView_Tweets.Items.Contains(item)) {
                    item = ListOfItems.GetCurrentlyTopmostShownItem();
                    AppController.Current.Logger.addDebugMessage("Scroll to item not displayed!","Initated retrieval of new top item",item:item,type:DebugMessage.DebugMessageTypes.ScrollIntoView);
                }
                ScrollToItem = item;
            }
        }

        private void buttonCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonMinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void BorderTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void buttonNewApnPost_Click_1(object sender, RoutedEventArgs e)
        {
            UserInterface.ComposeNewApnPost composeWindows = new ComposeNewApnPost();
            composeWindows.Show();
        }

        private void BorderTop_MouseEnter_1(object sender, MouseEventArgs e)
        {
            gridWindowButtons.Visibility = Visibility.Visible;
        }

        private void BorderTop_MouseLeave_1(object sender, MouseEventArgs e)
        {
            gridWindowButtons.Visibility = Visibility.Collapsed;
        }

        private void buttonMaximizeWindow_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        
        private void buttonQuickFilter_Click(object sender, RoutedEventArgs e)
        {
            buttonQuickFilter.ContextMenu.IsOpen = true;
        }

        private void quickFilter_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                filter_type = menuItem.CommandParameter as string;
                filter_update();
            }
        }

        public void update_filter_button(View view)
        {
            filter_type = null;
            if (view.isTwitterOnlyView)
            {
                filterTwitterRoot.Visibility = Visibility.Visible;
                filterApnRoot.Visibility = Visibility.Collapsed;
                filterFacebookRoot.Visibility = Visibility.Collapsed;
                filterQFM.Visibility = Visibility.Collapsed;
            }
            else
            {
                filterTwitterRoot.Visibility = Visibility.Collapsed;

                if (view.hasApnContents)
                {
                    filterApnRoot.Visibility = Visibility.Visible;
                }
                else
                {
                    filterApnRoot.Visibility = Visibility.Collapsed;
                }

                if (view.hasFacebookContents)
                {
                    filterFacebookRoot.Visibility = Visibility.Visible;
                }
                else
                {
                    filterFacebookRoot.Visibility = Visibility.Collapsed;
                }

                if (view.hasQfmContents)
                {
                    filterQFM.Visibility = Visibility.Visible;
                }
                else
                {
                    filterQFM.Visibility = Visibility.Collapsed;
                }
            }
            filter_update();
        }

        private void filter_update()
        {
            ListOfItems.listView_Tweets.Items.Filter = delegate(object obj)
             {
                 IItem item = obj as IItem;
                 if (item != null)
                 {
                     bool show_text = true;
                     bool show_type = true;

                     if (string.IsNullOrEmpty(filter_text))
                     {
                         show_text = true;
                     }
                     else
                     {
                         if (!string.IsNullOrEmpty(item.Text) && !string.IsNullOrEmpty(item.AuthorName))
                         {
                             if (item.Text.ToLower().Contains(filter_text.ToLower()) || item.AuthorName.ToLower().Contains(filter_text.ToLower()))
                             {
                                 AppController.Current.Logger.addDebugMessage("Item has been decided to be shown in text search", filter_text.ToLower(), type: DebugMessage.DebugMessageTypes.Display, item: item);
                                 show_text = true;
                             }
                             else
                             {
                                 AppController.Current.Logger.addDebugMessage("Item has been decided to be NOT shown in text search", filter_text.ToLower(), type: DebugMessage.DebugMessageTypes.Display, item: item);
                                 show_text = false;
                             }
                         }
                         else
                         {
                             AppController.Current.Logger.addDebugMessage("Item has been decided to be NOT shown in filter", "Item text or AuthorName is null", type: DebugMessage.DebugMessageTypes.Display, item: item);
                             show_text = false;
                         }
                     }

                     if (!string.IsNullOrEmpty(filter_type))
                     {
                             if(filter_type.StartsWith("twitter")) 
                             {
                                 TwitterItem twitterItem = item as TwitterItem;
                                 if (twitterItem == null)
                                 {
                                     show_type = false;
                                 }
                                 else
                                 {
                                     switch (filter_type)
                                     {
                                         case "twitterAll":
                                             show_type = true;
                                             break;

                                         case "twitterTimeline":
                                             show_type = (!twitterItem.isDirectMessage && !twitterItem.isMention && !twitterItem.isRetweeted && !twitterItem.isSearchResult && !twitterItem.isList);
                                             break;

                                         case "twitterMentions":
                                             show_type = twitterItem.isMention;
                                             break;

                                         case "twitterDM":
                                             show_type = twitterItem.isDirectMessage;
                                             break;

                                         case "twitterRetweets":
                                             show_type = twitterItem.isRetweeted;
                                             break;

                                         case "twitterFavs":
                                             show_type = twitterItem.isFavorited;
                                             break;

                                         case "twitterSearchResults":
                                             show_type = twitterItem.isSearchResult;
                                             break;

                                         case "twitterListEntries":
                                             show_type = twitterItem.isList;
                                             break;

                                         default:
                                             show_type = false;
                                             break;
                                     }
                                 }
                             } 
                             else if(filter_type.StartsWith("apn")) 
                             {
                                 ApnItem apnItem = item as ApnItem;
                                 if (apnItem == null)
                                 {
                                     show_type = false;
                                 }
                                 else
                                 {
                                     switch (filter_type)
                                     {
                                         case "apnAll":
                                             show_type = true;
                                             break;

                                         case "apnPersonalStream":

                                             break;

                                         case "apnMentions":
                                             show_type = apnItem.isMention;
                                             break;

                                         case "apnPM":
                                             show_type = apnItem.isPrivateMessage;
                                             break;

                                         case "apnReposts":
                                             show_type = apnItem.isReposted;
                                             break;

                                         case "apnFavs":
                                             throw new NotImplementedException();
                                             break;

                                         case "apnPhotos":
                                             show_type = apnItem.hasEmbeddedImages;
                                             break;

                                         default:
                                             show_type = false;
                                             break;
                                     }
                                 }
                             }
                             else if (filter_type.StartsWith("fb"))
                             {
                                 FacebookItem fbItem = item as FacebookItem;
                                 if (fbItem == null)
                                 {
                                     show_type = false;
                                 }
                                 else
                                 {
                                     switch (filter_type)
                                     {
                                         case "fbAll":
                                             show_type = true;
                                             break;

                                         case "fbStatusMessages":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.StatusMessage;
                                             break;
                                             
                                         case "fbLinks":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.Link;
                                             break;

                                         case "fbPhotos":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.Photo;
                                             break;

                                         case "fbVideos":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.Video;
                                             break;

                                         case "fbEvents":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.Event;
                                             break;

                                         case "fbNotes":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.Note;
                                             break;

                                         case "fbCheckIns":
                                             show_type = fbItem.MessageType == FacebookItem.MessageTypes.CheckIn;
                                             break;

                                         default:
                                             show_type = false;
                                             break;

                                     }
                                 }
                             }
                             else
                             {
                                 show_type = false;
                             }
                     }
                     else
                     {
                         show_type = true;
                     }

                     return (show_text && show_type);
                 }
                 else
                 {
                     AppController.Current.Logger.addDebugMessage("Item has been decided to be NOT shown in filter", "It is null", type: DebugMessage.DebugMessageTypes.Display, item: item);
                     return false;
                 }
             };
        }
    }
}
