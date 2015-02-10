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

namespace Nymphicus.Controls.Facebook
{
    /// <summary>
    /// Interaction logic for UserInfo.xaml
    /// </summary>
    public partial class UserInfo : UserControl
    {
        public UserInfo()
        {
            InitializeComponent();

            //comboBoxLists.addAddNewListButton();
        }

        private void textBoxWebsite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock websiteBlock = sender as TextBlock;
            if (websiteBlock != null)
            {
                if (websiteBlock.Text != string.Empty)
                {
                    try
                    {
                        System.Uri uri = new Uri(websiteBlock.Text);

                    }
                    catch { }
                }
            }
        }




 


  

        private void infoBoxUsername_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FacebookUser user = this.DataContext as FacebookUser;
            if (user != null)
            {
                try
                {
                    System.Diagnostics.Process.Start("https://www.facebook.com/" + user.Username);
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
        }
    }
}
