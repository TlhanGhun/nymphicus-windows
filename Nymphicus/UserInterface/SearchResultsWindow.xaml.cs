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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Nymphicus.Model;
using Nymphicus.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TweetSharp;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class SearchResultsWindow : Window
    {
        #region AeroGlass stuff
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [StructLayout(LayoutKind.Sequential)]
        public class MARGINS
        {
            public int cxLeftWidth, cxRightWidth,
                cyTopHeight, cyBottomHeight;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
            {
                // Get the current window handle
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent;

                this.Background = Brushes.Transparent;

                // Set the proper margins for the extended glass part
                MARGINS margins = new MARGINS();
                margins.cxLeftWidth = -1;
                margins.cxRightWidth = -1;
                margins.cyTopHeight = -1;
                margins.cyBottomHeight = -1;

                int result = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);

                if (result < 0)
                {
                    MessageBox.Show("An error occured while extending the glass unit.");
                }

            }
        }
        #endregion

        private BackgroundWorker backgroundWorkerSearch;
        AccountTwitter SearchingAccount;

        public ObservableCollection<TwitterItem> searchResults { get; set; }

        public SearchResultsWindow(AccountTwitter account)
        {
            InitializeComponent();
            SearchingAccount = account;
            searchResults = new ObservableCollection<TwitterItem>();
            searchTextboxKeyword.ExecuteSearchEnter += new SearchTextbox.ExecuteSearchEnterEventHandler(searchTextboxKeyword_ExecuteSearch);
            searchTextboxKeyword.SearchCleared += new SearchTextbox.SearchClearedEventHandler(searchTextboxKeyword_SearchCleared);
            listBoxItems.listView_Tweets.ItemsSource = searchResults;
            listBoxItems.listView_Tweets.Items.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Descending));

            backgroundWorkerSearch = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerSearch.WorkerSupportsCancellation = true;
            backgroundWorkerSearch.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorkerSearch_DoWork);
            backgroundWorkerSearch.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorkerSearch_RunWorkerCompleted);

            searchTextboxKeyword.textboxSearchString.Focus();

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

        void backgroundWorkerSearch_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            return;
        }

        void backgroundWorkerSearch_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            List<TwitterItem> foundItems = e.Result as List<TwitterItem>;
            if (foundItems != null)
            {
                foreach (TwitterItem foundItem in foundItems)
                {
                    searchResults.Add(foundItem);
                }
            }
            textBlockSearchInProgress.Visibility = Visibility.Collapsed;
        }

        void backgroundWorkerSearch_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string keyword = e.Argument as string;
            if (!string.IsNullOrEmpty(keyword))
            {
                TwitterService searchService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, SearchingAccount.Token, SearchingAccount.TokenSecret);
                searchService.Proxy = API.WebHelpers.getProxyString();
                searchService.UserAgent = "Nymphicus for Windows";

                List<TwitterItem> foundItems = API.Functions.executeSearch(searchService, keyword, null, SearchingAccount);
                e.Result = foundItems;
            }
            else
            {
                e.Result = null;
            }
            return;
        }

        void searchTextboxKeyword_SearchCleared(object sender, EventArgs e)
        {
            
        }

        void searchTextboxKeyword_ExecuteSearch(object sender, EventArgs e)
        {
            if (searchTextboxKeyword.textboxSearchString.Text != "")
            {
                executeSearch(searchTextboxKeyword.textboxSearchString.Text, SearchingAccount);
            }
        }

        public void executeSearch(string keyword, AccountTwitter account)
        {
            SearchingAccount = account;
            if (account == null)
            {
                if (AppController.Current.AllTwitterAccounts.Count > 0)
                {
                    account = AppController.Current.AllTwitterAccounts[0];
                }
                else
                {
                    return;
                }
            }
            textBlockSearchInProgress.Visibility = Visibility.Visible;
            searchTextboxKeyword.textboxSearchString.Text = keyword;
            searchResults.Clear();


            backgroundWorkerSearch = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerSearch.WorkerSupportsCancellation = true;
            backgroundWorkerSearch.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorkerSearch_DoWork);
            backgroundWorkerSearch.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorkerSearch_RunWorkerCompleted);

            if (backgroundWorkerSearch.IsBusy)
            {
                backgroundWorkerSearch.CancelAsync();
            }
            if (!backgroundWorkerSearch.IsBusy)
            {
                backgroundWorkerSearch.RunWorkerAsync(keyword);
            }
        }



        private void textBoxKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (searchTextboxKeyword.textboxSearchString.Text != "")
                {
                    executeSearch(searchTextboxKeyword.textboxSearchString.Text, SearchingAccount);
                }
            }
        }

        private void buttonSaveSeach_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(searchTextboxKeyword.textboxSearchString.Text))
            {
                AccountTwitter account = comboBoxAccount.comboBoxAccounts.SelectedItem as AccountTwitter;
                if (account != null)
                {
                    API.Functions.addSearchToAccount(account, searchTextboxKeyword.textboxSearchString.Text);
                }
            }
        }

        private void textBoxKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox != null)
            {
                if (string.IsNullOrEmpty(textbox.Text))
                {
                    buttonSaveSeach.IsEnabled = false;
                }
                else
                {
                    buttonSaveSeach.IsEnabled = true;
                }
            }
        }

        private void buttonOpenUserSearch_Click(object sender, RoutedEventArgs e)
        {
            if (AppController.Current.AllTwitterAccounts.Count > 0)
            {
                // xxx was, wenn erster nicht Twitter bzw. generell was ist mit Suche in Facebook...?
                AccountTwitter account = AppController.Current.AllTwitterAccounts[0] as AccountTwitter;
                UserInterface.SearchUser searchUser = new SearchUser(account, false);
                searchUser.Width = this.Width;
                searchUser.Height = this.Height;
                searchUser.Top = this.Top;
                searchUser.Left = this.Left;
                searchUser.Show();
                this.Close();
            }
        }
    }
}
