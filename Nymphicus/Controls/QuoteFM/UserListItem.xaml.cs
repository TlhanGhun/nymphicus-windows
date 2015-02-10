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

namespace Nymphicus.Controls.QuoteFM
{
    /// <summary>
    /// Interaction logic for UserListItem.xaml
    /// </summary>
    public partial class UserListItem : UserControl
    {
        public UserListItem()
        {
            InitializeComponent();

            ButtonUsername.Cursor = Cursors.Hand;
        }

        private void ButtonUsername_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                QuoteFmUser person = button.CommandParameter as QuoteFmUser;
                if (person != null)
                {
                    if (person.FollowerCount == 0 && person.FollowingCount == 0)
                    {
                        QuoteSharp.User user = QuoteSharp.API.getUser(person.Id);
                        if (user != null)
                        {
                            person = QuoteFmUser.createFromApi(user);
                        }
                    }
                    ShowUser userInfo = new ShowUser();
                    userInfo.setPerson(person);
                    userInfo.Show();
                }
            }
        }
    }
}
