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
using System.ComponentModel;
using Nymphicus.Model;
using AppNetDotNet.ApiCalls;
using AppNetDotNet.Model;

namespace Nymphicus.UserInterface.Apn
{
    /// <summary>
    /// Interaction logic for UserInfo.xaml
    /// </summary>
    public partial class UserInfo : Window
    {
        public User user { get; set; }
        public ThreadSaveObservableCollection<ApnItem> recentPosts { get; set; }
        private BackgroundWorker backgroundWorkerRecentPosts { get; set; }
        private BackgroundWorker backgroundWorkerUpdateUser { get; set; }
        public AccountAppDotNet account { get; set; }

        Tuple<List<User>, ApiCallResponse> followings;
        Tuple<List<User>, ApiCallResponse> followers;

        private void initalizeAllData() {

            if (account == null || user == null)
            {
                MessageBox.Show("Loading userinfo failed");
            }

            recentPosts = new ThreadSaveObservableCollection<ApnItem>();
            
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
            
            this.Title = "User info of " + user.username;

            this.DataContext = user;
            this.listboxRecentPosts.listView_Tweets.ItemsSource = recentPosts;

            backgroundWorkerRecentPosts = new BackgroundWorker();
            backgroundWorkerRecentPosts.WorkerReportsProgress = true;
            backgroundWorkerRecentPosts.WorkerSupportsCancellation = true;
            backgroundWorkerRecentPosts.DoWork += backgroundWorkerRecentPosts_DoWork;
            backgroundWorkerRecentPosts.RunWorkerCompleted += backgroundWorkerRecentPosts_RunWorkerCompleted;
            backgroundWorkerRecentPosts.ProgressChanged += backgroundWorkerRecentPosts_ProgressChanged;

            backgroundWorkerUpdateUser = new BackgroundWorker();
            backgroundWorkerUpdateUser.WorkerReportsProgress = true;
            backgroundWorkerUpdateUser.WorkerSupportsCancellation = true;
            backgroundWorkerUpdateUser.DoWork += backgroundWorkerUpdateUser_DoWork;
            backgroundWorkerUpdateUser.RunWorkerCompleted += backgroundWorkerUpdateUser_RunWorkerCompleted;
            backgroundWorkerUpdateUser.ProgressChanged += backgroundWorkerUpdateUser_ProgressChanged;

            backgroundWorkerRecentPosts.RunWorkerAsync();
            backgroundWorkerUpdateUser.RunWorkerAsync();
        }

        public UserInfo(AccountAppDotNet receivingAccount, User toBeShownUser)
        {
            InitializeComponent();
            user = toBeShownUser;
            account = receivingAccount;

            initalizeAllData();
        }

        public UserInfo(AccountAppDotNet receivingAccount, string username)
        {
            InitializeComponent();
            Tuple<User,ApiCallResponse> userResponse = Users.getUserByUsernameOrId(receivingAccount.accessToken, username);
            if (userResponse.Item2.success)
            {
                user = userResponse.Item1;
            }
            account = receivingAccount;

            initalizeAllData();
        }

        #region Background worker

        #region recent posts

        void backgroundWorkerRecentPosts_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ApnItem item = e.UserState as ApnItem;
            if (item != null)
            {
                recentPosts.Add(item);
            }
        }

        void backgroundWorkerRecentPosts_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            borderUpdatingRecentPosts.Visibility = Visibility.Collapsed;
            backgroundWorkerRecentPosts.Dispose();
        }

        void backgroundWorkerRecentPosts_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = 20;
            items = Posts.getByUsernameOrId(account.accessToken, user.id, parameter);
            if (items.Item2.success)
            {
                foreach (Post post in items.Item1)
                {
                    ApnItem item = new ApnItem(post, account);
                    if (item != null)
                    {
                        item.receivingAccount = account;
                        backgroundWorkerRecentPosts.ReportProgress(100, item);
                    }
                }
            }
        }

        #endregion

        #region user infos

        void backgroundWorkerUpdateUser_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ApnItem item = e.UserState as ApnItem;
            if (e.ProgressPercentage == 25)
            {
                // followings
                Tuple<List<User>, ApiCallResponse> followingsOfUser = e.UserState as Tuple<List<User>, ApiCallResponse>;
                if (followingsOfUser != null)
                {
                    followings = followingsOfUser;
                }
                else
                {
                    followings.Item1.AddRange(followingsOfUser.Item1);
                }
                
            }
            else if (e.ProgressPercentage == 50)
            {
                // followers
                Tuple<List<User>, ApiCallResponse> followersOfUser = e.UserState as Tuple<List<User>, ApiCallResponse>;
                if (followersOfUser != null)
                {
                    followers = followersOfUser;
                }
                else
                {
                    followers.Item1.AddRange(followersOfUser.Item1);
                }
                createFriendshipOverview(user.username);
                
            } else if (e.ProgressPercentage == 75)
            {
                // stars
            }
            else if (e.ProgressPercentage == 100)
            {
                User updatedUser = e.UserState as User;
                if (updatedUser != null)
                {
                    this.user = updatedUser;
                }
            }
           
        }

        void backgroundWorkerUpdateUser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorkerUpdateUser.Dispose();
        }

        void backgroundWorkerUpdateUser_DoWork(object sender, DoWorkEventArgs e)
        {
            bool still_not_complete = true;
            Parameters parameter = new Parameters();
            parameter.count = 200;
            while (still_not_complete)
            {
                Tuple<List<User>, ApiCallResponse> followingsOfUser = Users.getFollowingsOfUser(account.accessToken, user.username, parameter);
                backgroundWorkerUpdateUser.ReportProgress(25, followingsOfUser);
                if (followingsOfUser.Item2.meta.more)
                {
                    parameter.before_id = followingsOfUser.Item2.meta.min_id;
                }
                else
                {
                    still_not_complete = false;
                }
            }

            still_not_complete = true;
            parameter.before_id = null;
            while (still_not_complete)
            {
                Tuple<List<User>, ApiCallResponse> followersOfUser = Users.getFollowersOfUser(account.accessToken, user.username, parameter);
                backgroundWorkerUpdateUser.ReportProgress(50, followersOfUser);
                if (followersOfUser.Item2.meta.more)
                {
                    parameter.before_id = followersOfUser.Item2.meta.min_id;
                }
                else
                {
                    still_not_complete = false;
                }
            }

            // stars not available!?
            // Tuple<List<User>, ApiCallResponse> starsOfUser = Users.get (account.accessToken, user.username);

            Tuple<User, ApiCallResponse> updatedUser = Users.getUserByUsernameOrId(account.accessToken, user.username, parameter);
            backgroundWorkerUpdateUser.ReportProgress(100, updatedUser);
        }

        #endregion

        #endregion

        #region Friendships

        private class buttonParameter
        {
            public bool doFollow = true;
            public string username;
            public AccountAppDotNet paccount;
        }

        public void createFriendshipOverview(string username)
        {
            if (followers == null || followings == null)
            {
                textblockUpdatingUser.Text = "Unable to update current user...";
                return;
            }
            GridFriendships.Children.Clear();
            
            int numberOfAccount = 0;
            if (AppController.Current != null)
            {
                foreach (AccountAppDotNet faccount in AppController.Current.AllApnAccounts)
                {
                    if (username.ToLower() == faccount.username.ToLower())
                    {
                        // we don't need to follow ourselves...
                        continue;
                    }
                    RowDefinition thisRow = new RowDefinition();
                    GridFriendships.RowDefinitions.Add(thisRow);
                    ColumnDefinition thisTextColumn = new ColumnDefinition();
                    thisTextColumn.Width = GridLength.Auto;
                    ColumnDefinition thisButtonColumn = new ColumnDefinition();
                    thisButtonColumn.MinWidth = 64;
                    thisButtonColumn.MaxWidth = 64;

                    string followedString = "not followed";
                    TextBlock followedBlock = new TextBlock();
                    followedBlock.Foreground = Brushes.Red;
                    // check if already followed by that account
                    if(followings.Item1.Where(fol => fol.id == faccount.Id.ToString()).Count() > 0)
                    {
                        followedString = "followed";
                        followedBlock.Foreground = Brushes.Green;
                    }
                    followedBlock.Text = followedString;

                    string followingStrng = "not following";
                    TextBlock followingBlock = new TextBlock();
                    followingBlock.Foreground = Brushes.Red;
                    if (followers.Item1.Where(fol => fol.id == faccount.Id.ToString()).Count() > 0)
                    {
                        followingStrng = "following";
                        followingBlock.Foreground = Brushes.Green;
                    }
                    followingBlock.Text = followingStrng;

                    TextBlock textblock = new TextBlock();

                    textblock.Inlines.Add(faccount.username);
                    textblock.Inlines.Add(" is ");
                    textblock.Inlines.Add(followedBlock);
                    textblock.Inlines.Add(" / is ");
                    textblock.Inlines.Add(followingBlock);
                    textblock.Inlines.Add("  ");

                    Grid.SetColumn(textblock, 0);
                    Grid.SetRow(textblock, numberOfAccount);

                    Button button = new Button();
                    button.Content = "Follow";

                    buttonParameter parameter = new buttonParameter();
                    parameter.paccount = account;
                    parameter.username = username;

                    if (followers.Item1.Where(fol => fol.id == faccount.Id.ToString()).Count() > 0)
                    {
                        button.Content = "Unfollow";
                        button.Background = Brushes.DarkGray;
                        button.Foreground = Brushes.LightGray;
                        parameter.doFollow = false;
                    }
                    button.CommandParameter = parameter;
                    button.Click += buttonFollowUser_Click;


                    Grid.SetColumn(button, 1);
                    Grid.SetRow(button, numberOfAccount);


                    GridFriendships.ColumnDefinitions.Add(thisTextColumn);
                    GridFriendships.ColumnDefinitions.Add(thisButtonColumn);
                    GridFriendships.Children.Add(textblock);
                    GridFriendships.Children.Add(button);
                    numberOfAccount++;
                }
            }
        }

        void buttonFollowUser_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            buttonParameter parameter = button.CommandParameter as buttonParameter;
            if (parameter.doFollow)
            {
                Tuple<User,ApiCallResponse> response = Users.followByUsernameOrId(parameter.paccount.accessToken, parameter.username);
                if (response.Item2.success)
                {
                    followers.Item1.Add(parameter.paccount.user);
                }
            }
            else
            {
                Tuple<User,ApiCallResponse> response = Users.unfollowByUsernameOrId(parameter.paccount.accessToken, parameter.username);
                if (response.Item2.success)
                {
                    try
                    {
                        User user = followers.Item1.Where(acc => acc.id == parameter.paccount.Id.ToString()).First();
                        if (user != null)
                        {
                            followers.Item1.Remove(user);
                        }
                    }
                    catch { }
                }
            }
            createFriendshipOverview(parameter.username);
        }

        #endregion
    }
}
