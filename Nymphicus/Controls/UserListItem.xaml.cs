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

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for UserListItem.xaml
    /// </summary>
    public partial class UserListItem : UserControl
    {
        public AccountTwitter InitiatingAccount { get; set; }

        public UserListItem()
        {
            InitializeComponent();
            ButtonUsername.Cursor = Cursors.Hand;
        }

        public void setAccount(AccountTwitter account)
        {
            InitiatingAccount = account;
        }

        private void ButtonUsername_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Person person = button.CommandParameter as Person;
                if (person != null)
                {
                    ShowUser userInfo = new ShowUser();
                    userInfo.setPerson(person, InitiatingAccount);
                    userInfo.Show();
                }
            }
        }
    }
}
