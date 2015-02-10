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

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for ComboBoxLists.xaml
    /// </summary>
    public partial class ComboBoxLists : UserControl
    {
        public ComboBoxLists()
        {
            InitializeComponent();

            if (AppController.Current != null)
            {
                if (AppController.Current.AllAccounts != null)
                {
                    comboBoxLists.ItemsSource = AppController.Current.AllLists;
                    if (AppController.Current.AllLists.Count > 0)
                    {
                        comboBoxLists.SelectedIndex = 0;
                    }
                }
            }
        }

        public void selectAccount(AccountTwitter account)
        {
            if (account != null)
            {
                comboBoxLists.ItemsSource = account.Lists;
                if (account.Lists.Count > 0)
                {
                    comboBoxLists.SelectedIndex = 0;
                }
            }
            else
            {
                comboBoxLists.Items.Clear();
            }
        }

        public void addAddNewListButton()
        {
            /* when using itemssource is used no manual adding of extra buttons possible
            TweetList dummyList = new TweetList();
            dummyList.FullName = "Create new list...";
            comboBoxLists.Items.Add(dummyList);
             * */
        }
    }
}
