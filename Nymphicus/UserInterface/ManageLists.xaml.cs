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


namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for ManageLists.xaml
    /// </summary>
    public partial class ManageLists : Window
    {
        #region Aero Glass
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


        public ManageLists()
        {
            InitializeComponent();
            comboBoxAccounts.comboBoxAccounts.SelectionChanged += new SelectionChangedEventHandler(comboBoxAccounts_SelectionChanged);
            if (comboBoxAccounts.comboBoxAccounts.Items.Count > 0)
            {
                comboBoxAccounts.comboBoxAccounts.SelectedIndex = 0;
            }
            listBoxLists.SelectionChanged += new SelectionChangedEventHandler(listBoxLists_SelectionChanged);
            if (listBoxLists.Items.Count > 0)
            {
                listBoxLists.SelectedIndex = 0;
            }
            comboBoxAccounts_SelectionChanged(null, null);
        }

        void listBoxLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TweetList list = listBoxLists.SelectedItem as TweetList;
            if (list != null)
            {
                listBoxListMembers.ItemsSource = list.Members;
                if (list.IsOwnList)
                {
                    buttonAddListMember.IsEnabled = true;
                    buttonRemoveListMember.IsEnabled = true;
                    buttonRemoveList.IsEnabled = true;
                }
                else
                {
                    buttonAddListMember.IsEnabled = false;
                    buttonRemoveListMember.IsEnabled = false;
                    buttonRemoveList.IsEnabled = false;
                }
            }
        }

        void comboBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountTwitter account = comboBoxAccounts.comboBoxAccounts.SelectedItem as AccountTwitter;
            if (account != null)
            {
                listBoxLists.ItemsSource = account.Lists;
            }
            if (listBoxLists.Items.Count > 0)
            {
                listBoxLists.SelectedIndex = 0;
            }
            else
            {
                listBoxListMembers.ItemsSource = null;
            }
        }

        public void selectAccount(AccountTwitter account)
        {
            comboBoxAccounts.comboBoxAccounts.SelectedItem = account;
        }


        void newList_CreateSuccess(object sender, NewList.CreateSuccessEventArgs e)
        {
            listBoxLists.Items.Refresh();
        }

        private void buttonAddList_Click(object sender, RoutedEventArgs e)
        {
            AccountTwitter account = comboBoxAccounts.comboBoxAccounts.SelectedItem as AccountTwitter;
            if (account != null)
            {
                NewList newList = new NewList(account);
                newList.CreateSuccess += new NewList.CreateSuccessEventHandler(newList_CreateSuccess);
                newList.Show();
            }
        }

        private void buttonRemoveList_Click(object sender, RoutedEventArgs e)
        {
            AccountTwitter account = comboBoxAccounts.comboBoxAccounts.SelectedItem as AccountTwitter;
            if (account != null)
            {
                TweetList list = listBoxLists.SelectedItem as TweetList;
                if (list != null)
                {
                    if (API.Functions.deleteList(account, list))
                    {
                        listBoxLists.Items.Refresh();
                    }
                }
            }
        }

        private void buttonAddListMember_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxAccounts.comboBoxAccounts.SelectedItem != null)
            {
                AccountTwitter selectedAccount = comboBoxAccounts.comboBoxAccounts.SelectedItem as AccountTwitter;
                if (selectedAccount != null)
                {
                    SearchUser userSearchWindows = new SearchUser(selectedAccount, true);
                    userSearchWindows.SearchSuccess += new SearchUser.SearchSuccessEventHandler(userSearchWindows_SearchSuccess);
                    userSearchWindows.Show();
                }
            }
        }

        void userSearchWindows_SearchSuccess(object sender, SearchUser.SearchSuccessEventArgs e)
        {
            TweetList list = listBoxLists.SelectedItem as TweetList;
            if (list != null)
            {
                foreach (Person person in e.selectedPersons)
                {
                    API.Functions.addPersonToList(person, list, AppController.Current.getAccountForId(list.AccountId));
                }
            }
        }

        private void buttonRemoveListMember_Click(object sender, RoutedEventArgs e)
        {
            Person person = listBoxListMembers.SelectedItem as Person;
            TweetList list = listBoxLists.SelectedItem as TweetList;
            if (person != null && list != null)
            {
                API.Functions.removePersonFromList(person, list, AppController.Current.getAccountForId(list.AccountId));
            }
        }
    }
}
