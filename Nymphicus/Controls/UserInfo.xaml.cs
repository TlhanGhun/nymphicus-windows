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

        private void buttonFollowing_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Person person = button.CommandParameter as Person;
                if (person != null)
                {
                    if (person.IsFollowedBy == true)
                    {
                        
                    }
                }
            }      
        }

        public void createFriendshipOverview(string username) {
            int numberOfAccount = 0;
            if (AppController.Current != null)
            {
                GridFriendships.Children.Clear();
                
                foreach (IAccount iaccount in AppController.Current.AllAccounts)
                {
                    AccountTwitter account;
                    if (iaccount.GetType() == typeof(AccountTwitter))
                    {
                        account = iaccount as AccountTwitter;
                    }
                    else
                    {
                        continue;
                    }
                    if (username.ToLower() == account.Login.Username.ToLower())
                    {
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
                    if (account.checkFriendship(username).IsFollowed)
                    {
                        followedString = "followed";
                        followedBlock.Foreground = Brushes.Green;
                    }
                    followedBlock.Text = followedString;

                    string followingStrng = "not following";
                    TextBlock followingBlock = new TextBlock();
                    followingBlock.Foreground = Brushes.Red;
                    if (account.checkFriendship(username).IsFollowing)
                    {
                        followingStrng = "following";
                        followingBlock.Foreground = Brushes.Green;
                    }
                    followingBlock.Text = followingStrng;

                    TextBlock textblock = new TextBlock();

                    textblock.Inlines.Add(account.Login.Username);
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
                    parameter.account = account;
                    parameter.username = username;

                    if (account.checkFriendship(username).IsFollowing)
                    {
                        button.Content = "Unfollow";
                        button.Background = Brushes.DarkGray;
                        button.Foreground = Brushes.LightGray;
                        parameter.doFollow = false;
                    }
                    button.CommandParameter = parameter;
                    button.Click += new RoutedEventHandler(button_Click);
                    

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

        private class buttonParameter
        {
            public bool doFollow = true;
            public string username;
            public AccountTwitter account;
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            buttonParameter parameter = button.CommandParameter as buttonParameter;
            if (parameter.doFollow)
            {
                if (parameter.account.followUser(parameter.username))
                {
                    createFriendshipOverview(parameter.username);
                }
            }
            else
            {
                if (parameter.account.unfollowUser(parameter.username))
                {
                    createFriendshipOverview(parameter.username);
                }
            }
        }

        private void buttonAddToNewList_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Person person = button.CommandParameter as Person;
                if (person != null)
                {
                    UserInterface.NewList newList = new UserInterface.NewList();
                    newList.person = person;
                    newList.textBlockUserWillBeAdded.Visibility = System.Windows.Visibility.Visible;
                    newList.textBlockUserWillBeAdded.Text = "@" + person.Username + " will be added";
                    newList.Show();
                }
            }
        }

        private void buttonAddToList_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Person person = button.CommandParameter as Person;
                if (person != null)
                {
                    TweetList list = comboBoxLists.comboBoxLists.SelectedItem as TweetList;
                    if (list != null)
                    {
                        if (list.FullName != "Create new list...")
                        {
                            try
                            {
                                API.Functions.addPersonToList(person, list, AppController.Current.getAccountForId(list.AccountId));
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show(exp.Message, "Adding to list failed");
                                AppController.Current.Logger.writeToLogfile(exp);
                            }
                        }
                        else
                        {
                            try
                            {
                                addToNewlyCreatedList(person);
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show(exp.Message, "Adding to list failed");
                                AppController.Current.Logger.writeToLogfile(exp);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            addToNewlyCreatedList(person);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message, "Adding to list failed");
                            AppController.Current.Logger.writeToLogfile(exp);
                        }
                    }
                }
                
            }
        }

        private void addToNewlyCreatedList(Person person)
        {
            UserInterface.NewList newList = new UserInterface.NewList();
            newList.person = person;
            newList.textBlockUserWillBeAdded.Visibility = System.Windows.Visibility.Visible;
            newList.textBlockUserWillBeAdded.Text = "@" + person.Username + " will be added";
            newList.Show();
         
        }

        private void infoBoxUsername_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Person person = this.DataContext as Person;
            if (person != null)
            {
                try
                {
                    System.Diagnostics.Process.Start("http://www.twitter.com/" + person.Username);
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
        }

        private void buttonMention_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Person person = button.CommandParameter as Person;
                if (person != null)
                {
                    UserInterface.ComposeNewTweet composeWindow = new UserInterface.ComposeNewTweet();
                    composeWindow.textBoxTweet.textBoxContent.Text = "@" + person.Username + " ";
                    composeWindow.Show();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Person person = button.CommandParameter as Person;
                if (person != null)
                {
                    UserInterface.ComposeNewTweet composeWindow = new UserInterface.ComposeNewTweet();
                    composeWindow.isDirectMessage(person.Username);
                    composeWindow.Show();
                }
            }
        }
    }
}
