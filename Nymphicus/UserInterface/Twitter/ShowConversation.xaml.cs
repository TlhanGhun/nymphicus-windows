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
using TweetSharp;

namespace Nymphicus.UserInterface.Twitter
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class ShowConversationWindow : Window
    {
        #region AeroGlass stuff
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

        private BackgroundWorker backgroundWorkerBuildConversation;

        public ObservableCollection<TwitterItem> conversationItems { get; set; }
        private TwitterItem startItem {get;set;}

        public ShowConversationWindow(TwitterItem item)
        {
            InitializeComponent();
            if (item == null)
            {
                return;
            }
            conversationItems = new ObservableCollection<TwitterItem>();
            conversationItems.Add(item);
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
            TwitterItem item = (TwitterItem)e.UserState;
            item.accountId = startItem.accountId;
            conversationItems.Add(item);
        }

        void backgroundWorkerBuildConversation_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
 
        }

        void backgroundWorkerBuildConversation_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            AccountTwitter account = AppController.Current.getAccountForId(startItem.accountId);
            if (!startItem.isDirectMessage)
            {
                #region Mention conversation
                decimal currentStatusId = startItem.InReplyToStatusId;
                while (currentStatusId != 0)
                {
                    if (account.Mentions.Where(i => i.Id == currentStatusId).Count() > 0)
                    {
                        TwitterItem knownMention = account.Mentions.Where(i => i.Id == currentStatusId).First() as TwitterItem;
                        if (knownMention != null)
                        {
                            currentStatusId = 0;
                            currentStatusId = knownMention.InReplyToStatusId;
                            backgroundWorkerBuildConversation.ReportProgress(100, knownMention);
                        }
                    }
                    else if (account.Timeline.Where(i => i.Id == currentStatusId).Count() > 0)
                    {
                        TwitterItem knownTimeline = account.Timeline.Where(i => i.Id == currentStatusId).First() as TwitterItem;
                        if (knownTimeline != null)
                        {
                            currentStatusId = 0;
                            currentStatusId = knownTimeline.InReplyToStatusId;
                            backgroundWorkerBuildConversation.ReportProgress(100, knownTimeline);
                        }
                    }
                    else
                    {
                        GetTweetOptions options = new GetTweetOptions();
                        options.Id = Convert.ToInt64(currentStatusId);
                        TwitterStatus status = startItem.RetrievingAccount.twitterService.GetTweet(options);

                        currentStatusId = 0;
                        if (status != null)
                        {
                            TwitterItem newItem = API.TweetSharpConverter.getItemFromStatus(status, startItem.RetrievingAccount);
                            if (newItem != null)
                            {
                                backgroundWorkerBuildConversation.ReportProgress(100, newItem);
                                currentStatusId = newItem.InReplyToStatusId;
                            }
                        }
                    }
                }

                // future entries
                currentStatusId = startItem.Id;

                List<TwitterItem> allItems = new List<TwitterItem>();
                foreach (IItem item in account.Mentions)
                {
                    allItems.Add(item as TwitterItem);
                }
                foreach (IItem item in account.Timeline)
                {
                    allItems.Add(item as TwitterItem);
                }

                List<decimal> multipleQueue = new List<decimal>();

                while (currentStatusId != 0)
                {
                    if (allItems.Where(i => i.InReplyToStatusId == currentStatusId).Count() > 0)
                    {
                        IEnumerable<TwitterItem> knownItems = allItems.Where(i => i.InReplyToStatusId == currentStatusId);
                        currentStatusId = 0;
                        foreach (TwitterItem item in knownItems)
                        {
                            if (item != null)
                            {
                                backgroundWorkerBuildConversation.ReportProgress(100, item);
                                if (currentStatusId == 0)
                                {
                                    currentStatusId = item.Id;
                                }
                                else
                                {
                                    multipleQueue.Add(item.InReplyToStatusId);
                                }

                            }
                        }
                    }
                    else
                    {
                        currentStatusId = 0;
                    }


                    if (currentStatusId == 0 && multipleQueue.Count() > 0)
                    {
                        currentStatusId = multipleQueue.First();
                        multipleQueue.RemoveAt(0);
                    }
                }
                #endregion
            }
            else
            {
                #region DM conversation

                List<TwitterItem> allDms = new List<TwitterItem>();
                foreach(IItem iitem in account.DirectMessages) {
                    allDms.Add(iitem as TwitterItem);
                }

                Person chatPartner;
                if(startItem.Author.Username == account.Login.Username) {
                    chatPartner = startItem.DMReceipient;
                }
                else
                {
                    chatPartner = startItem.Author;
                }

                IEnumerable<IItem> allOwnDMsInConversation = allDms.Where(i => i.DMReceipient.Username == chatPartner.Username);
                foreach (TwitterItem item in allOwnDMsInConversation)
                {
                    if (item != startItem)
                    {
                        backgroundWorkerBuildConversation.ReportProgress(100, item);
                    }
                }

                IEnumerable<IItem> allRecDMsInConversation = allDms.Where(i => i.Author.Username == chatPartner.Username);
                foreach (TwitterItem item in allRecDMsInConversation)
                {
                    if (item != startItem)
                    {
                        backgroundWorkerBuildConversation.ReportProgress(100, item);
                    }
                }

                #endregion
            }
        }

    }
}
