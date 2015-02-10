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
    /// Interaction logic for SearchUser.xaml
    /// </summary>
    public partial class SearchUser : Window
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

        public ObservableCollection<Person> persons {get;set;}
        public AccountTwitter SearchingAccount { get; set; }
        private bool ShowCheckboxesAndButtons { get; set; }

        public SearchUser(AccountTwitter account, bool showCheckboxesAndButtons)
        {
            InitializeComponent();

            SearchingAccount = account;
            ShowCheckboxesAndButtons = showCheckboxesAndButtons;
            if (!showCheckboxesAndButtons)
            {
                buttonOK.Visibility = Visibility.Collapsed;
                buttonCancle.Visibility = Visibility.Collapsed;
            }

            persons = new ObservableCollection<Person>();
            searchTextboxKeyword.ExecuteSearch += new SearchTextbox.ExecuteSearchEventHandler(searchTextboxKeyword_ExecuteSearch);
            searchTextboxKeyword.ExecuteSearchEnter += new SearchTextbox.ExecuteSearchEnterEventHandler(searchTextboxKeyword_ExecuteSearchEnter);
            searchTextboxKeyword.SearchCleared += new SearchTextbox.SearchClearedEventHandler(searchTextboxKeyword_SearchCleared);
            listBoxPersons.listBox_Persons.ItemsSource = persons;
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

        void searchTextboxKeyword_ExecuteSearchEnter(object sender, EventArgs e)
        {
            if (searchTextboxKeyword.textboxSearchString.Text != "")
            {
                executeSearch(searchTextboxKeyword.textboxSearchString.Text);
            }
        }

        void searchTextboxKeyword_ExecuteSearch(object sender, EventArgs e)
        {
          
        }

        void searchTextboxKeyword_SearchCleared(object sender, EventArgs e)
        {

        }

        public event SearchSuccessEventHandler SearchSuccess;
        public delegate void SearchSuccessEventHandler(object sender, SearchSuccessEventArgs e);
        public class SearchSuccessEventArgs : EventArgs
        {
            public List<Person> selectedPersons
            {
                get;
                set;
            }
        }


        public void executeSearch(string keyword)
        {
            textBlockSearchInProgress.Visibility = Visibility.Visible;
            this.UpdateLayout();
            searchTextboxKeyword.textboxSearchString.Text = keyword;
            persons.Clear();
            List<Person> foundPersons = API.Functions.executePersonSearch(keyword, SearchingAccount);
            foreach (Person foundPerson in foundPersons)
            {
                foundPerson.ShowCheckboxForThisUserInList = ShowCheckboxesAndButtons;
                persons.Add(foundPerson);
            }
            textBlockSearchInProgress.Visibility = Visibility.Collapsed;
        }


        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            List<Person> selectedPersons = new List<Person>();
            foreach (Person person in listBoxPersons.listBox_Persons.SelectedItems)
            {
                if (person != null)
                {
                    selectedPersons.Add(person);
                }
            }
            SearchSuccessEventArgs eventArgs = new SearchSuccessEventArgs();
            eventArgs.selectedPersons = selectedPersons;
            if (SearchSuccess != null)
            {
                SearchSuccess(this, eventArgs);
            }
            Close();
        }

        private void buttonCancle_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

 
    }
}
