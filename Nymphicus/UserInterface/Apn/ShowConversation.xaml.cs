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
using AppNetDotNet;
using AppNetDotNet.ApiCalls;
using AppNetDotNet.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nymphicus.UserInterface.Apn
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class ShowConversationWindow : Window
    {
        

        private BackgroundWorker backgroundWorkerBuildConversation;

        public ObservableCollection<ApnItem> conversationItems { get; set; }
        private ApnItem startItem {get;set;}

        public ShowConversationWindow(ApnItem item)
        {
            InitializeComponent();
            if (item == null)
            {
                return;
            }
            conversationItems = new ObservableCollection<ApnItem>();
            startItem = item;
            listBoxItems.listView_Tweets.ItemsSource = conversationItems;
            listBoxItems.listView_Tweets.Items.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Descending));

            backgroundWorkerBuildConversation = new System.ComponentModel.BackgroundWorker();
            backgroundWorkerBuildConversation.WorkerSupportsCancellation = true;
            backgroundWorkerBuildConversation.WorkerReportsProgress = true;
            backgroundWorkerBuildConversation.ProgressChanged +=new ProgressChangedEventHandler(backgroundWorkerBuildConversation_ProgressChanged);
            backgroundWorkerBuildConversation.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorkerBuildConversation_DoWork);
            backgroundWorkerBuildConversation.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorkerBuildConversation_RunWorkerCompleted);
            backgroundWorkerBuildConversation.RunWorkerAsync();
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

            Show();
        }

        void backgroundWorkerBuildConversation_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ApnItem item = (ApnItem)e.UserState;
            item.accountId = startItem.accountId;
            conversationItems.Add(item);
        }

        void backgroundWorkerBuildConversation_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
 
        }

        void backgroundWorkerBuildConversation_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            AccountAppDotNet account = startItem.receivingAccount;


            if (!startItem.isPrivateMessage)
            {
                Tuple<List<Post>, ApiCallResponse> postsInFuture = Posts.getRepliesById(account.accessToken, startItem.apnItem.reply_to);
                if (postsInFuture.Item2.success)
                {
                    foreach (Post post in postsInFuture.Item1)
                    {
                        ApnItem item = new ApnItem(post, startItem.receivingAccount);
                        if (item != null)
                        {
                            item.receivingAccount = account;
                            backgroundWorkerBuildConversation.ReportProgress(100, item);
                        }
                    }
                }

            }

            else
            {
                Tuple<List<Message>, ApiCallResponse> messages = Messages.getMessagesInChannel(account.accessToken, startItem.channelId);
                if (messages.Item2.success)
                {
                    foreach (Message message in messages.Item1)
                    {
                        ApnItem item = new ApnItem(message, startItem.receivingAccount);
                        if (item != null)
                        {
                            item.receivingAccount = account;
                            backgroundWorkerBuildConversation.ReportProgress(100, item);
                        }
                    }
                }

            }
            return;
            // old approach not needed in App.net API! 
                #region Mention conversation
                string currentStatusId = startItem.apnItem.reply_to;
                while (!string.IsNullOrEmpty(currentStatusId))
                {
                    if (account.PersonalStream.Where(i => i.Id.ToString() == currentStatusId).Count() > 0)
                    {
                        ApnItem knownMention = account.PersonalStream.Where(i => i.Id.ToString() == currentStatusId).First() as ApnItem;
                        if (knownMention != null)
                        {
                            currentStatusId = "";
                            currentStatusId = knownMention.apnItem.reply_to;
                            backgroundWorkerBuildConversation.ReportProgress(100, knownMention);
                        }
                    }
                    else if (account.PersonalStream.Where(i => i.Id.ToString() == currentStatusId).Count() > 0)
                    {
                        ApnItem knownTimeline = account.PersonalStream.Where(i => i.Id.ToString() == currentStatusId).First() as ApnItem;
                        if (knownTimeline != null)
                        {
                            currentStatusId = "";
                            currentStatusId = knownTimeline.apnItem.reply_to;
                            backgroundWorkerBuildConversation.ReportProgress(100, knownTimeline);
                        }
                    }
                    else
                    {
                        Tuple<Post,ApiCallResponse> newPost = Posts.getById(account.accessToken, currentStatusId);
                        currentStatusId = "";
                        if (newPost.Item2.success)
                        {
                            ApnItem newItem = new ApnItem(newPost.Item1, startItem.receivingAccount);
                            if (newItem != null)
                            {
                                backgroundWorkerBuildConversation.ReportProgress(100, newItem);
                                currentStatusId = newItem.apnItem.reply_to;
                            }
                        }
                    }
                }

                // future entries
                currentStatusId = startItem.apnItem.id;

                List<ApnItem> allItems = new List<ApnItem>();
                foreach (ApnItem item in account.PersonalStream)
                {
                    allItems.Add(item);
                }

                List<string> multipleQueue = new List<string>();

                while (!string.IsNullOrEmpty(currentStatusId))
                {
                    if (allItems.Where(i => i.apnItem.reply_to == currentStatusId).Count() > 0)
                    {
                        IEnumerable<ApnItem> knownItems = allItems.Where(i => i.apnItem.reply_to == currentStatusId);
                        currentStatusId = "";
                        foreach (ApnItem item in knownItems)
                        {
                            if (item != null)
                            {
                                backgroundWorkerBuildConversation.ReportProgress(100, item);
                                if (string.IsNullOrEmpty(currentStatusId))
                                {
                                    currentStatusId = item.apnItem.id;
                                }
                                else
                                {
                                    multipleQueue.Add(item.apnItem.reply_to);
                                }

                            }
                        }
                    }
                    else
                    {
                        currentStatusId = "";
                    }

                    


                    if (string.IsNullOrEmpty(currentStatusId) && multipleQueue.Count() > 0)
                    {
                        currentStatusId = multipleQueue.First();
                        multipleQueue.RemoveAt(0);
                    }
                }
                #endregion

        }

    }
}
