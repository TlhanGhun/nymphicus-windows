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
using AppNetDotNet.Model;

namespace Nymphicus.Controls.AppDotNet
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
                User person = button.CommandParameter as User;
                if (person != null)
                {
                    //uuu
                    //UserInterface.Apn.UserInfo userInfo = new UserInterface.Apn.UserInfo(person);
                    //userInfo.Show();
                }
            }
        }
    }
}
