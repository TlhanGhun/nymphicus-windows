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
using QuoteSharp;

namespace Nymphicus.Controls.QuoteFM
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
            QuoteFmUser person = this.DataContext as QuoteFmUser;
            if (person != null)
            {
                try
                {
                    System.Diagnostics.Process.Start("http://quote.fm/" + person.username);
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
        }

        private void InfoBoxFollowers_MouseDown(object sender, MouseButtonEventArgs e)
        {
            QuoteFmUser user = this.DataContext as QuoteFmUser;
            if (user != null)
            {
                ListOfUsers followers = QuoteSharp.API.getUsersListOfFollowers(user.username);
                if (followers != null)
                {
                    if (followers.entities != null)
                    {
                        ThreadSaveObservableCollection<QuoteFmUser> users = new ThreadSaveObservableCollection<QuoteFmUser>();
                        foreach (User follower in followers.entities)
                        {
                            users.Add(QuoteFmUser.createFromApi(follower));
                        }
                        int page = 0;
                        while (users.Count < user.FollowerCount)
                        {
                            page++;
                             ListOfUsers moreFollowers = QuoteSharp.API.getUsersListOfFollowers(user.username,page);
                             if (moreFollowers != null)
                             {
                                 if (moreFollowers.entities != null)
                                 {
                                     if (moreFollowers.entities.Count() == 0) { break; }
                                     foreach (User follower in followers.entities)
                                     {
                                         users.Add(QuoteFmUser.createFromApi(follower));
                                     }
                                 }
                                 else
                                 {
                                     break;
                                 }

                             }
                             else
                             {
                                 break;
                             }
                        }

                        UserInterface.QuoteFM.ListUsers listUsers = new UserInterface.QuoteFM.ListUsers(users);
                        listUsers.Show();
                    }
                }
            }
        }

        private void InfoBoxFollowing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            QuoteFmUser user = this.DataContext as QuoteFmUser;
            if (user != null)
            {
                ListOfUsers followings = QuoteSharp.API.getUsersListOfFollowings(user.username);
                if (followings != null)
                {
                    if (followings.entities != null)
                    {
                        ThreadSaveObservableCollection<QuoteFmUser> users = new ThreadSaveObservableCollection<QuoteFmUser>();
                        foreach (User following in followings.entities)
                        {
                            users.Add(QuoteFmUser.createFromApi(following));
                        }
                        int page = 0;
                        while (users.Count < user.FollowerCount)
                        {
                            page++;
                            ListOfUsers moreFollowings = QuoteSharp.API.getUsersListOfFollowings(user.username, page);
                            if (moreFollowings != null)
                            {
                                if (moreFollowings.entities != null)
                                {
                                    if (moreFollowings.entities.Count() == 0) { break; }
                                    foreach (User moreFollowing in moreFollowings.entities)
                                    {
                                        users.Add(QuoteFmUser.createFromApi(moreFollowing));
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }

                        UserInterface.QuoteFM.ListUsers listUsers = new UserInterface.QuoteFM.ListUsers(users);
                        listUsers.Show();
                    }
                }
            }
        }

    }
}