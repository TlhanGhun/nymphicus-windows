using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TweetSharp;
using System.ComponentModel;
using System.Windows.Threading;

namespace Nymphicus.Model
{
    public class AccountTwitter : IAccount, INotifyPropertyChanged
    {
        public TwitterService twitterService;
        private bool listsLoaded;
        private bool savedSearchesLoaded;
        public OAuthAccessToken Tokens
        {
            get
            {
                if (_tokens == null)
                {
                    createTokens();
                }
                return _tokens;
            }
        }

        public OAuthAccessToken _tokens
        {
            get;
            set;
        }

        public bool IsStreamingAccount { get; set; }
        private bool isStreaming { get; set; }
        private bool fetchSentDirectMessages { get; set; }
        public bool AllInitalFetchesCompleted { get; set; }

        private BackgroundWorker backgroundWorkerTimeline;
        private BackgroundWorker backgroundWorkerMentions;
        private BackgroundWorker backgroundWorkerDirectMessages;
        private BackgroundWorker backgroundWorkerRetweets;
        private BackgroundWorker backgroundWorkerFavorites;

        public ThreadSaveObservableCollection<IItem> AllItems { get; set; }
        public ThreadSaveObservableCollection<IItem> Timeline { get; set; }
        public decimal highestKnownTimeline { get; set; }
        private bool BusyTimeline = false;
        public bool InitialUpdateDoneForTimeline { get; private set; }
        public ThreadSaveObservableCollection<IItem> Mentions { get; set; }
        public decimal highestKnownMention = 0;
        private bool BusyMentions = false;
        public bool InitialUpdateDoneForMentions { get; private set; }
        public ThreadSaveObservableCollection<IItem> DirectMessages { get; set; }
        private bool BusyDirectMessages = false;
        public bool InitialUpdateDoneForDirectMessages { get; private set; }
        public decimal highestKnownDirectMessage = 0;
        public decimal highestKnownSentDirectMessage = 0;
        public ThreadSaveObservableCollection<IItem> Retweets { get; set; }
        private bool BusyRetweets = false;
        public bool InitialUpdateDoneForRetweets { get; private set; }
        private decimal highestKnownRetweet = 0;
        public ThreadSaveObservableCollection<IItem> Retweeted { get; set; }
        public ThreadSaveObservableCollection<IItem> Favorites { get; set; }
        private bool BusyFavorites = false;
        public bool InitialUpdateDoneForFavorites { get; private set; }
        private decimal highestKnownFavorite = 0;

        public List<TweetList> Lists { get; set; }
        public List<Search> Searches { get; set; }
        public Person Login { get; set; }
        public string Token;
        public string TokenSecret;
        public string VerifyKey { get; set; }
        Dictionary<string, Friendship> Friendships { get; set; }
        private bool StreamingInitialFriendshipsHandled { get; set; }

        public decimal tweetMarkerTimeline { get; set; }
        public decimal tweetMarkerMentions { get; set; }
        public decimal tweetMarkerDMs { get; set; }
        public decimal tweetMarkerFavorites { get; set; }

        public static Twitter_Help_Configuration twitter_configuration { get; set; }

        public ObservableCollection<SubscribableItemsCollection> subscribableItemCollections
        {
            get;
            set;
        }

        public bool LoginSuccessfull
        {
            get;
            set;
        }
        public string LoginErrorMessage { get; private set; }

        private string twitterStream;

        private bool firstFullUpdate;

        public Color accountColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                NotifyPropertyChanged("backgroundColor");
                NotifyPropertyChanged("backgroundHtmlColor");
                NotifyPropertyChanged("AccountColorBrush");
            }
        }
        public Color _backgroundColor { get; set; }

        public SolidColorBrush AccountColorBrush
        {
            get
            {
                if (Properties.Settings.Default.ColourizedTimeline)
                {
                    return new SolidColorBrush(accountColor);
                }
                else
                {
                    return Brushes.Transparent;
                }
            }
        }

        public string backgroundHtmlColor
        {
            get
            {
                return accountColor.ToString();
            }
            set
            {
                return;
            }
        }



        public Color foregroundColor;
        public string foregroundHtmlColor
        {
            get
            {
                return foregroundColor.ToString();
            }
            set
            {
                return;
            }
        }


        public AccountTwitter()
        {
            twitterService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret);
            twitterService.Proxy = API.WebHelpers.getProxyString();

            twitterService.UserAgent = "Nymphicus for Windows";
            LoginSuccessfull = false;
            LoginErrorMessage = "";
            subscribableItemCollections = new ObservableCollection<SubscribableItemsCollection>();
            AllItems = new ThreadSaveObservableCollection<IItem>();
            Timeline = new ThreadSaveObservableCollection<IItem>();
            SubscribableItemsCollection timelineColl = new SubscribableItemsCollection();
            timelineColl.DisplayTitle = "Timeline";
            timelineColl.ItemCollection = Timeline;
            highestKnownTimeline = 0;
            fetchSentDirectMessages = true;
            Mentions = new ThreadSaveObservableCollection<IItem>();
            DirectMessages = new ThreadSaveObservableCollection<IItem>();
            Retweets = new ThreadSaveObservableCollection<IItem>();
            Retweeted = new ThreadSaveObservableCollection<IItem>();
            Favorites = new ThreadSaveObservableCollection<IItem>();
            Lists = new List<TweetList>();
            Searches = new List<Search>();
            Friendships = new Dictionary<string, Friendship>();
            accountColor = Colors.Black;
            foregroundColor.A = 255;

            highestKnownDirectMessage = 0;
            highestKnownMention = 0;
            highestKnownRetweet = 0;
            highestKnownSentDirectMessage = 0;
            highestKnownTimeline = 0;

            AvailableNotificationClasses = new List<string>();

            firstFullUpdate = true;

            backgroundWorkerTimeline = new BackgroundWorker();
            backgroundWorkerTimeline.WorkerReportsProgress = true;
            backgroundWorkerTimeline.WorkerSupportsCancellation = true;
            backgroundWorkerTimeline.DoWork += new DoWorkEventHandler(backgroundWorkerTimeline_DoWork);
            backgroundWorkerTimeline.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeline_RunWorkerCompleted);
            backgroundWorkerTimeline.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerTimeline_ProgressChanged);

            backgroundWorkerMentions = new BackgroundWorker();
            backgroundWorkerMentions.WorkerReportsProgress = true;
            backgroundWorkerMentions.WorkerSupportsCancellation = true;
            backgroundWorkerMentions.DoWork += new DoWorkEventHandler(backgroundWorkerMentions_DoWork);
            backgroundWorkerMentions.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerMentions_RunWorkerCompleted);
            backgroundWorkerMentions.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerMentions_ProgressChanged);

            backgroundWorkerDirectMessages = new BackgroundWorker();
            backgroundWorkerDirectMessages.WorkerReportsProgress = true;
            backgroundWorkerDirectMessages.WorkerSupportsCancellation = true;
            backgroundWorkerDirectMessages.DoWork += new DoWorkEventHandler(backgroundWorkerDirectMessages_DoWork);
            backgroundWorkerDirectMessages.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerDirectMessages_RunWorkerCompleted);
            backgroundWorkerDirectMessages.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerDirectMessages_ProgressChanged);

            backgroundWorkerRetweets = new BackgroundWorker();
            backgroundWorkerRetweets.WorkerReportsProgress = true;
            backgroundWorkerRetweets.WorkerSupportsCancellation = true;
            backgroundWorkerRetweets.DoWork += new DoWorkEventHandler(backgroundWorkerRetweets_DoWork);
            backgroundWorkerRetweets.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerRetweets_RunWorkerCompleted);
            backgroundWorkerRetweets.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerRetweets_ProgressChanged);

            backgroundWorkerFavorites = new BackgroundWorker();
            backgroundWorkerFavorites.WorkerReportsProgress = true;
            backgroundWorkerFavorites.WorkerSupportsCancellation = true;
            backgroundWorkerFavorites.DoWork += new DoWorkEventHandler(backgroundWorkerFavorites_DoWork);
            backgroundWorkerFavorites.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerFavorites_RunWorkerCompleted);
            backgroundWorkerFavorites.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerFavorites_ProgressChanged);

            Mentions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);
            DirectMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);
            Timeline.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);
            Retweets.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);
            Favorites.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);

            twitter_configuration = new Twitter_Help_Configuration();

        }

        public void createTokens()
        {
            _tokens = Nymphicus.API.Functions.getOAuthTokens(this);
        }

        public bool AddItemToCollection(ThreadSaveObservableCollection<IItem> Collection, IItem item, string CollectionName)
        {
            try
            {
                AppController.Current.Logger.addDebugMessage(CollectionName + " item received", "", type: DebugMessage.DebugMessageTypes.Twitter, item: item);
                Collection.Add(item);
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.addDebugMessage(CollectionName + " item error", exp.Message, type: DebugMessage.DebugMessageTypes.Twitter, item: item, account: this);
                AppController.Current.Logger.writeToLogfile("Exception on adding item to " + CollectionName);
                AppController.Current.Logger.writeToLogfile(exp);
                return false;
            }
            return true;
        }

        #region Background Workers
        #region DoWorks

        void backgroundWorkerTimeline_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
            AppController.Current.Logger.addDebugMessage("Starting timeline retrieval", "", account: this, type: DebugMessage.DebugMessageTypes.Retrival);
            decimal maxId = (decimal)e.Argument;
            List<TwitterItem> items;
            try
            {
                AppController.Current.Logger.addDebugMessage("Starting TwitterService by TweetSharp", "", account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                TwitterService timelineService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Token, this.TokenSecret);
                timelineService.Proxy = API.WebHelpers.getProxyString();
                timelineService.UserAgent = "Nymphicus for Windows";
                AppController.Current.Logger.addDebugMessage("Getting timeline from API", "", account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                if (maxId > 0)
                {
                    items = API.Functions.getTimeline(timelineService, this, e, maxId);
                }
                else
                {
                    items = API.Functions.getTimeline(timelineService, this, e);
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                items = new List<TwitterItem>();
            }
            foreach (TwitterItem item in items)
            {
                if (item == null) { continue; }
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                try
                {
                    AppController.Current.Logger.addDebugMessage("Adding item to timeline", "", account: this, item: item, type: DebugMessage.DebugMessageTypes.Retrival);
                    item.accountId = this.Login.Id;
                    item.ReceivingAccount = this;
                    if (item.Id == tweetMarkerTimeline)
                    {
                        AppController.Current.Logger.addDebugMessage("Is TweetMarker", "for timeline", account: this, item: item, type: DebugMessage.DebugMessageTypes.Retrival);
                        item.IsTweetMarker = true;
                    }
                    AppController.Current.Logger.addDebugMessage("Now reporting item to list", "in timeline", account: this, item: item, type: DebugMessage.DebugMessageTypes.Retrival);
                    backgroundWorkerTimeline.ReportProgress(100, item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                }
            }

            AppController.Current.Logger.addDebugMessage("Backgroundworker completed", "", account: this, type: DebugMessage.DebugMessageTypes.Retrival);
        }

        void backgroundWorkerRetweets_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
            decimal maxId = (decimal)e.Argument;
            List<TwitterItem> items;
            try
            {

                TwitterService retweetsService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Token, this.TokenSecret);
                retweetsService.Proxy = API.WebHelpers.getProxyString();
                retweetsService.UserAgent = "Nymphicus for Windows";

                if (maxId > 0)
                {
                    items = API.Functions.getRetweets(retweetsService, this, e, maxId);
                }
                else
                {
                    items = API.Functions.getRetweets(retweetsService, this, e);
                }
            }
            catch
            {
                items = new List<TwitterItem>();
            }
            foreach (TwitterItem item in items)
            {
                if (item == null) { continue; }
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                try
                {
                    item.accountId = this.Login.Id;
                    item.isRetweetedToMe = true;
                    item.ReceivingAccount = this;
                    backgroundWorkerRetweets.ReportProgress(100, item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                }
            }
        }

        void backgroundWorkerMentions_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }

            decimal maxId = (decimal)e.Argument;
            List<TwitterItem> items;
            try
            {
                TwitterService mentionsService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Token, this.TokenSecret);
                mentionsService.Proxy = API.WebHelpers.getProxyString();
                mentionsService.UserAgent = "Nymphicus for Windows";

                if (maxId > 0)
                {
                    items = API.Functions.getMentions(mentionsService, this, e, maxId);
                }
                else
                {
                    items = API.Functions.getMentions(mentionsService, this, e);
                }
            }
            catch
            {
                items = new List<TwitterItem>();
            }
            foreach (TwitterItem item in items)
            {
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                if (item == null) { continue; }
                try
                {
                    item.accountId = this.Login.Id;
                    item.isMention = true;
                    item.ReceivingAccount = this;

                    backgroundWorkerMentions.ReportProgress(100, item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                }
            }
        }

        void backgroundWorkerFavorites_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }

            decimal maxId = (decimal)e.Argument;
            List<TwitterItem> items;
            try
            {
                TwitterService favService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Token, this.TokenSecret);
                favService.Proxy = API.WebHelpers.getProxyString();
                favService.UserAgent = "Nymphicus for Windows";

                if (maxId > 0)
                {
                    items = API.Functions.getFavorites(favService, this, e, maxId);
                }
                else
                {
                    items = API.Functions.getFavorites(favService, this, e);
                }
            }
            catch
            {
                items = new List<TwitterItem>();
            }
            foreach (TwitterItem item in items)
            {
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                if (item == null) { continue; }
                try
                {
                    item.accountId = this.Login.Id;
                    item.isFavorited = true;
                    item.ReceivingAccount = this;
                    if (item.Id == tweetMarkerFavorites)
                    {
                        item.IsTweetMarker = true;
                    }
                    backgroundWorkerFavorites.ReportProgress(100, item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                }
            }
        }

        void backgroundWorkerDirectMessages_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }

            decimal maxId = (decimal)e.Argument;
            List<TwitterItem> items;
            try
            {
                TwitterService dmService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Token, this.TokenSecret);
                dmService.Proxy = API.WebHelpers.getProxyString();
                dmService.UserAgent = "Nymphicus for Windows";

                if (fetchSentDirectMessages)
                {
                    if (maxId > 0)
                    {
                        items = API.Functions.getSentDirectMessages(dmService, this, e, maxId);
                    }
                    else
                    {
                        items = API.Functions.getSentDirectMessages(dmService, this, e);
                    }
                }
                else
                {
                    if (maxId > 0)
                    {
                        items = API.Functions.getDirectMessages(dmService, this, e, maxId);
                    }
                    else
                    {
                        items = API.Functions.getDirectMessages(dmService, this, e);
                    }
                }
            }
            catch
            {
                items = new List<TwitterItem>();
            }
            foreach (TwitterItem item in items)
            {
                if (item == null) { continue; }
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                if (item == null) { continue; }

                try
                {
                    item.accountId = this.Login.Id;
                    item.isDirectMessage = true;
                    item.ReceivingAccount = this;
                    if (item.Id == tweetMarkerDMs)
                    {
                        item.IsTweetMarker = true;
                    }
                    backgroundWorkerDirectMessages.ReportProgress(100, item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, account: this, type: DebugMessage.DebugMessageTypes.Retrival);
                }
            }
        }

        #endregion

        #region Report progress / Progress changed


        void backgroundWorkerTimeline_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;
            if (AddItemToCollection(Timeline, item, "Timeline"))
            {
                if (InitialUpdateDoneForTimeline)
                {
                    AppController.Current.sendNotification(Login.Username + " Timeline", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                }
                if (item.Id > highestKnownTimeline || highestKnownTimeline == 0)
                {
                    highestKnownTimeline = item.Id;
                }
            }
        }

        void backgroundWorkerRetweets_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;
            if (item.RetweetedItem == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(item.RetweetedItem.Text))
            {
                return;
            }

            if (AddItemToCollection(Retweets, item, "Retweets"))
            {
                if (InitialUpdateDoneForRetweets)
                {
                    if (item.RetweetedItem != null)
                    {
                        AppController.Current.sendNotification(Login.Username + " Retweets", item.RetweetedItem.Author.NameAndLogin, item.RetweetedItem.Text, item.RetweetedItem.Author.Avatar, item.RetweetedItem);
                    }
                    else
                    {
                        AppController.Current.sendNotification(Login.Username + " Retweets", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                    }
                }

                if (item.Id > highestKnownRetweet || highestKnownRetweet == 0)
                {
                    highestKnownRetweet = item.Id;
                }

            }
        }

        void backgroundWorkerMentions_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;

            if (AddItemToCollection(Mentions, item, "Mentions"))
            {
                if (InitialUpdateDoneForMentions)
                {
                    AppController.Current.sendNotification(Login.Username + " Mentions", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                }
                if (item.Id > highestKnownMention || highestKnownMention == 0)
                {
                    highestKnownMention = item.Id;
                }

            }
        }

        void backgroundWorkerFavorites_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;
            if (AddItemToCollection(Favorites, item, "Favorites"))
            {
                if (InitialUpdateDoneForFavorites)
                {
                    AppController.Current.sendNotification(Login.Username + " Favorites", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                }
                if (item.Id > highestKnownFavorite || highestKnownFavorite == 0)
                {
                    highestKnownFavorite = item.Id;
                }

            }
        }

        void backgroundWorkerDirectMessages_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;


            if (AddItemToCollection(DirectMessages, item, "Direct messages"))
            {
                if (InitialUpdateDoneForDirectMessages)
                {
                    AppController.Current.sendNotification(Login.Username + " Direct Messages", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                }
                if (item.IsSentDirectMessage)
                {
                    if (item.Id > highestKnownSentDirectMessage || highestKnownSentDirectMessage == 0)
                    {
                        highestKnownSentDirectMessage = item.Id;
                    }
                }
                else
                {
                    if (item.Id > highestKnownDirectMessage || highestKnownDirectMessage == 0)
                    {
                        highestKnownDirectMessage = item.Id;
                    }
                }

            }
        }

        #endregion

        #region Worker completed

        void backgroundWorkerMentions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialUpdateDoneForMentions)
            {
                InitialUpdateDoneForMentions = true;
                CheckIfAllFetchesAreComplete();
            }

        }

        void backgroundWorkerRetweets_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialUpdateDoneForRetweets)
            {
                InitialUpdateDoneForRetweets = true;
                CheckIfAllFetchesAreComplete();
            }

        }

        void backgroundWorkerTimeline_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialUpdateDoneForTimeline)
            {
                InitialUpdateDoneForTimeline = true;
                CheckIfAllFetchesAreComplete();
            }
        }

        void backgroundWorkerFavorites_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialUpdateDoneForFavorites)
            {
                InitialUpdateDoneForFavorites = true;
                CheckIfAllFetchesAreComplete();
            }

        }

        void backgroundWorkerDirectMessages_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (fetchSentDirectMessages)
            {
                fetchSentDirectMessages = false;
                UpdateDirectMessages();
                return;
            }
            if (!InitialUpdateDoneForDirectMessages)
            {
                InitialUpdateDoneForDirectMessages = true;
                CheckIfAllFetchesAreComplete();
            }
        }

        #endregion

        #endregion



        void SubCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TwitterItem item in e.OldItems)
                {
                    if (AllItems.Contains(item))
                    {
                        try
                        {
                            AllItems.Remove(item);
                        }
                        catch { }
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (TwitterItem item in e.NewItems)
                {
                    if (!AllItems.Contains(item))
                    {
                        try
                        {
                            AllItems.Add(item);
                        }
                        catch { }
                    }
                }
            }
        }

        public void getLists()
        {
            if (listsLoaded)
            {
                return;
            }
            List<TweetList> lists = API.Functions.getLists(this, this.Login.Username);
            foreach (TweetList list in lists)
            {
                list.AccountId = Login.Id;
                if (list.person.Id == Login.Id)
                {
                    list.IsOwnList = true;
                }
                list.UpdateMembers(this);
                Lists.Add(list);
                AppController.Current.AllLists.Add(list);
                AppController.Current.registerNotificationClass("List " + list.FullName);
                AvailableNotificationClasses.Add("List " + list.FullName);
                AppController.Current.Logger.addDebugMessage("Adding list to account", list.name, account: this, type: DebugMessage.DebugMessageTypes.Twitter);

                list.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);
            }
            listsLoaded = true;
        }

        public void getSearches()
        {
            if (savedSearchesLoaded) { return; }
            List<Search> searches = API.Functions.getSearches(this);
            foreach (Search search in searches)
            {
                addSearch(search);
            }
            savedSearchesLoaded = true;
        }

        public void addSearch(Search search)
        {
            search.AccountId = Login.Id;
            Searches.Add(search);
            AppController.Current.AllSearches.Add(search);
            AppController.Current.registerNotificationClass("Search " + search.name);
            AvailableNotificationClasses.Add("Search " + search.name);
            search.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SubCollectionChanged);
            AppController.Current.Logger.addDebugMessage("Adding search to account", search.name, account: this, type: DebugMessage.DebugMessageTypes.Twitter);
        }

        public void UpdateItems()
        {
            if (Properties.Settings.Default.DisableTwitterRetrieval)
            {
                AppController.Current.Logger.writeToLogfile("Twitter retrieval deactivated", false);
                return;
            }

            if (!firstFullUpdate)
            {
                getTweetMarkers();
            }
            if (firstFullUpdate)
            {
                firstFullUpdate = false;
            }



            if (isStreaming && !IsStreamingAccount)
            {
                // streaming has been manually stopped
                isStreaming = false;
                if (twitterStream != null)
                {
                    // twitterStream.EndStream();
                }
            }

            // hhh
            isStreaming = false;

            if (!isStreaming || !InitialUpdateDoneForTimeline)
            {
                UpdateTimeline();
            }
            if (!isStreaming || !InitialUpdateDoneForDirectMessages)
            {
                UpdateDirectMessages();
            }
            if (!isStreaming || !InitialUpdateDoneForMentions)
            {
                UpdateMentions();
            }
            if (!isStreaming || !InitialUpdateDoneForRetweets)
            {
                UpdateRetweets();
            }
            if (!isStreaming || !InitialUpdateDoneForFavorites)
            {
                UpdateFavorites();
            }
            if (IsStreamingAccount)
            {
                // hhh
                //   isStreaming = true;
                //   startStreaming();
            }

            UpdateLists();
            UpdateSearches();


        }

        private void startStreaming()
        {
            AppController.Current.Logger.addDebugMessage("Starting streaming interface", "", account: this, type: DebugMessage.DebugMessageTypes.Retrival);
            if (twitterStream == null)
            {
                AppController.Current.sendNotification("General info", "Streaming interface activated", Login.NameAndLogin, Login.Avatar, null);


                IAsyncResult asyncResult = twitterService.StreamUser(OnStreamEvent);

                // twitterStream = new Twitterizer.Streaming.TwitterStream(Tokens, "Nymphicus for Windows" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (http://www.li-ghun.de/Nymphicus/)", options);

                // IAsyncResult asyncResult = twitterStream.StartUserStream(streamingNewFriendCallback, streamingStoppedCallback, streamingStatusCreatedCallback, streamingStatusDeletedCallback, streamingDirectMessageCreatedCallback, streamingDirectMessageDeletedCallback, streamingEventCallback,streamingRawJsonCallback);
            }
        }

        private void OnStreamEvent(TwitterStreamArtifact streamEvent, TwitterResponse response)
        {
            Console.WriteLine("Hello");
        }

        public void streamingNewFriendCallback(List<decimal> ids)
        {
            /*
            if (StreamingInitialFriendshipsHandled)
            {
                try
                {
                    foreach (Person person in API.Functions.getPersonsFromId(this, ids))
                    {
                        AppController.Current.sendNotification(this.Login.Username + " New follower", person.Username + " is following you now", "@" + person.Username + " just started following @" + this.Login.Username, person.Avatar, null);
                        this.Login.NumberOfFollowers++;
                    }
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
            else
            {
                StreamingInitialFriendshipsHandled = true;
            }
         */
        }

        /*
        public void streamingRawJsonCallback(string JSON)
        {
            AppController.Current.Logger.writeToLogfile("Raw json: " + JSON);
        }

        public  void streamingStatusCreatedCallback(TwitterStatus status)
        {
            try {
            TwitterItem item = API.TweetSharpConverter.getItemFromStatus(status, this);
            item.accountId = this.Login.Id;
            if (status.RetweetedStatus != null)
            {
                if (status.InReplyToUserId == this.Login.Id)
                {
                    item.isRetweetedToMe = true;
                }
                else if (status.User.Id == this.Login.Id)
                {
                    item.isRetweetedByMe = true;
                }
                if (AddItemToCollection(Retweets, item, "Retweets"))
                {
                    highestKnownDirectMessage = item.Id;
                    if (item.RetweetedItem != null)
                    {
                        AppController.Current.sendNotification(Login.Username + " Retweets", item.RetweetedItem.Author.NameAndLogin, item.RetweetedItem.Text, item.RetweetedItem.Author.Avatar, item.RetweetedItem);
                    }
                    else
                    {
                        AppController.Current.sendNotification(Login.Username + " Retweets", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                    }
                }
            }
            else if (status.InReplyToUserId == this.Login.Id)
            {
                item.isMention = true;
                highestKnownMention = item.Id;
                if (AddItemToCollection(Mentions, item, "Mentions"))
                {
                    AppController.Current.sendNotification(Login.Username + " Mentions", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                }
            }
            else
            {
                highestKnownTimeline = item.Id;
                if (AddItemToCollection(Timeline, item, "Timeline"))
                {
                    AppController.Current.sendNotification(Login.Username + " Timeline", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                }
            }
            if (status.IsFavorited != null)
            {
                if (status.IsFavorited)
                {
                    if (AddItemToCollection(Favorites, item, "Favorites"))
                    {
                        AppController.Current.sendNotification(Login.Username + " Favorites", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                    }    
                }
            }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
        }

        public static void streamingStatusDeletedCallback(TwitterStreamDeletedEvent status) {
            TwitterItem deletedItem = new TwitterItem();
            deletedItem.Id = status.Id;
            AppController.Current.DeleteTweetFromEverywhere(deletedItem);
        }

        public void streamingStoppedCallback(StopReasons reason) {
            try
            {
                if (reason != StopReasons.StoppedByRequest)
                {
                    switch (reason)
                    {
                        case StopReasons.Forbidden:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned access forbidden. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.NotAcceptable:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned not acceptable. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.NotFound:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned not found. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.RangeUnacceptable:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned inacceptable range. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.RateLimited:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned rate limit reached. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.TooLong:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned too long. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.TwitterOverloaded:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned service is overloaded. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.TwitterServerError:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned internal server error. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.Unauthorised:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned unauthorized. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.WebConnectionFailed:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Web connection failed. Automatic retry will be tried", "", null);
                            break;

                        case StopReasons.Unknown:
                            AppController.Current.sendNotification("General info", "Streaming stopped", "Twitter returned an unknown error. Automatic retry will be tried", "", null);
                            break;
                    }

                }
                else
                {
                    AppController.Current.sendNotification("General info", "Streaming has been stopped", "Streaming has been stopped by request.", "", null);
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
            this.isStreaming = false;
            twitterStream = null;
        }

        public void streamingDirectMessageCreatedCallback(TwitterDirectMessage status) {
            try
            {
                TwitterItem item = API.TweetSharpConverter.getItemFromDirectMessage(status, this);
                item.isDirectMessage = true;
                item.accountId = this.Login.Id;
                if (AddItemToCollection(DirectMessages, item, "Direct messages"))
                {
                    AppController.Current.sendNotification(Login.Username + " Direct Messages", item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                    if (item.IsSentDirectMessage)
                    {
                        if (item.Id > highestKnownSentDirectMessage)
                        {
                            highestKnownSentDirectMessage = item.Id;
                        }
                    }
                    else
                    {
                        if (item.Id > highestKnownDirectMessage)
                        {
                            highestKnownDirectMessage = item.Id;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
        }

        public static void streamingDirectMessageDeletedCallback(TwitterStreamDeletedEvent status) {
            try
            {
                TwitterItem deletedItem = new TwitterItem();
                deletedItem.Id = status.Id;
                AppController.Current.DeleteTweetFromEverywhere(deletedItem);
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
        }

        public void streamingEventCallback(TwitterStreamEvent eventDetails) {
            if(eventDetails != null) {
                try
                {
                    switch (eventDetails.EventType)
                    {
                        case TwitterSteamEvent.Follow:
                            if (eventDetails.Source.ScreenName == eventDetails.Target.ScreenName) { return; }
                            AppController.Current.sendNotification(this.Login.Username + " New follower", eventDetails.Source.ScreenName + " is following you now", "@" + eventDetails.Source.ScreenName + " just started following @" + eventDetails.Target.ScreenName, eventDetails.Source.ProfileImageLocation, null);
                            break;

                        case TwitterSteamEvent.Favorite:
                            AppController.Current.sendNotification("General info", eventDetails.Source.ScreenName + " favorited a tweet", "@" + eventDetails.Source.ScreenName + " favorited a tweet of @" + eventDetails.Target.ScreenName, eventDetails.Source.ProfileImageLocation, null);
                            break;

                        case TwitterSteamEvent.Unfollow:
                            AppController.Current.sendNotification("General info", eventDetails.Source.ScreenName + " unfollowed you", "@" + eventDetails.Source.ScreenName + " unfollowed @" + eventDetails.Target.ScreenName, eventDetails.Source.ProfileImageLocation, null);
                            break;

             

                        default:
                            AppController.Current.sendNotification("ERROR", "Unknown streming message", eventDetails.EventType.ToString(), Login.Avatar, null);
                            break;
                    }
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile(exp, true);
                }
                
            }
        }

        void twitterStream_OnStreamingStopped(string reason)
        {
            AppController.Current.sendNotification("ERROR", "Streaming stopped!", reason, Login.Avatar,0,null);
        }

*/

        public void UpdateTimeline()
        {
            if (!backgroundWorkerTimeline.IsBusy)
            {
                BusyTimeline = false;
                backgroundWorkerTimeline.RunWorkerAsync(highestKnownTimeline);
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Timeline background thread of @" + username + " is busy");
                if (BusyTimeline)
                {
                    AppController.Current.Logger.writeToLogfile("Timeline background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerTimeline.CancelAsync();
                }
                BusyTimeline = true;
            }


        }

        public void UpdateRetweets()
        {
            if (!backgroundWorkerRetweets.IsBusy)
            {
                BusyRetweets = false;
                backgroundWorkerRetweets.RunWorkerAsync(highestKnownRetweet);
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Retweets background thread of @" + username + " is busy");
                if (BusyRetweets)
                {
                    AppController.Current.Logger.writeToLogfile("Retweets background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerRetweets.CancelAsync();
                }
                BusyRetweets = true;
            }
        }

        public void UpdateMentions()
        {
            if (!backgroundWorkerMentions.IsBusy)
            {
                BusyMentions = false;
                backgroundWorkerMentions.RunWorkerAsync(highestKnownMention);
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Mentions background thread of @" + username + " is busy");
                if (BusyMentions)
                {
                    AppController.Current.Logger.writeToLogfile("Mentions background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerMentions.CancelAsync();
                }
                BusyMentions = true;
            }
        }

        public void UpdateDirectMessages()
        {
            if (!backgroundWorkerDirectMessages.IsBusy)
            {
                BusyDirectMessages = false;
                if (fetchSentDirectMessages)
                {
                    backgroundWorkerDirectMessages.RunWorkerAsync(highestKnownSentDirectMessage);
                }
                else
                {
                    backgroundWorkerDirectMessages.RunWorkerAsync(highestKnownDirectMessage);
                }
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Direct messages background thread of @" + username + " is busy");
                if (BusyDirectMessages)
                {
                    AppController.Current.Logger.writeToLogfile("Direct messages background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerDirectMessages.CancelAsync();
                }
                BusyDirectMessages = true;
            }
        }


        public void UpdateFavorites()
        {
            if (!backgroundWorkerFavorites.IsBusy)
            {
                BusyFavorites = false;
                backgroundWorkerFavorites.RunWorkerAsync(highestKnownFavorite);
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Favorites background thread of @" + username + " is busy");
                if (BusyFavorites)
                {
                    AppController.Current.Logger.writeToLogfile("Favorites background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerFavorites.CancelAsync();
                }
                BusyFavorites = true;
            }


        }

        public void UpdateLists()
        {
            foreach (TweetList list in Lists)
            {
                list.UpdateItems(this);
            }
        }

        public void UpdateSearches()
        {
            foreach (Search search in Searches)
            {
                search.Update();
            }

        }



        public void createFromTweetSharp(OAuthAccessToken token)
        {
            this._tokens = token;
            this.Token = token.Token;
            this.TokenSecret = token.TokenSecret;
            Person newAccountUser = new Person(this);
            GetUserProfileOptions options = new GetUserProfileOptions();

            TwitterUser user = twitterService.GetUserProfile(options);
            //newAccountUser = API.TweetSharpConverter.getPersonFromLogin(token.ScreenName, this);
            newAccountUser = API.TweetSharpConverter.getPersonFromUser(user, this);
            this.Login = newAccountUser;
            createNotificationClasses();
            AppController.Current.registerTwitterAccountForNotifications(this.username);
            getLists();
            getSearches();
        }




        public override string ToString()
        {
            return this.Login.Username;
        }


        public string getStorableSettings()
        {
            string delimiter = "|||";
            string storableString = Crypto.EncryptString(Crypto.ToSecureString(Token));
            storableString += delimiter + Crypto.EncryptString(Crypto.ToSecureString(TokenSecret));
            storableString += delimiter + getColorString(accountColor);
            storableString += delimiter + IsStreamingAccount.ToString();
            storableString += delimiter + Login.Username;
            return storableString;
        }

        private string getColorString(Color color)
        {
            return color.A.ToString() + "," + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString();
        }

        private Color getColorFromString(string colorString)
        {
            Color returnColor = Brushes.LightGray.Color;

            string[] delimiter = { "," };
            string[] argb = colorString.Split(delimiter, StringSplitOptions.None);
            if (argb.Length == 4)
            {
                try
                {
                    Color color = Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));
                    returnColor = color;
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace, "", null);
                }
            }
            return returnColor;
        }

        public void readStorableSettings(string storedSettingsString)
        {
            try
            {
                string[] delimiter = { "|||" };
                string[] storedData = storedSettingsString.Split(delimiter, StringSplitOptions.None);

                if (storedData.Length < 3)
                {
                    this.LoginSuccessfull = false;
                    this.LoginErrorMessage = "Invalid stored data set";
                    return;
                }

                this.Token = storedData[0];
                if (storedData[0].StartsWith("EncryptedString:"))
                {
                    this.Token = Crypto.ToInsecureString(Crypto.DecryptString(storedData[0]));
                }
                this.TokenSecret = storedData[1];
                if (storedData[1].StartsWith("EncryptedString:"))
                {
                    this.TokenSecret = Crypto.ToInsecureString(Crypto.DecryptString(storedData[1]));
                }
                this.accountColor = getColorFromString(storedData[2]);

                if (storedData.Length > 3)
                {
                    if (storedData[3].ToLower() == "true")
                    {
                        IsStreamingAccount = true;
                    }
                }

                if (storedData.Length > 4)
                {
                    Login = new Person(this);
                    Login.Username = storedData[4];
                }

                if (!verifyCredentials())
                {
                    this.LoginSuccessfull = false;
                    this.LoginErrorMessage = "Verification failed";
                }

            }
            catch (Exception exp)
            {
                this.LoginSuccessfull = false;
                this.LoginErrorMessage = exp.Message;
            }
        }

        public void createNotificationClasses()
        {
            AvailableNotificationClasses = new List<string>();
            AvailableNotificationClasses.Add(username + " Timeline");
            AvailableNotificationClasses.Add(username + " Mentions");
            AvailableNotificationClasses.Add(username + " Retweets");
            AvailableNotificationClasses.Add(username + " Direct Messages");
        }

        public bool verifyCredentials()
        {
            try
            {
                twitterService.AuthenticateWith(this.Token, this.TokenSecret);
                //  VerifyCredentialsOptions options = new VerifyCredentialsOptions();
                //  options.IncludeEntities = true;

                GetUserProfileOptions options = new GetUserProfileOptions();



                //  TwitterUser user = twitterService.VerifyCredentials(options);
                TwitterUser user = twitterService.GetUserProfile(options);
                if (true)
                //if (user != null)
                {
                    this.Login = API.TweetSharpConverter.getPersonFromUser(user, this);
                    AppController.Current.Logger.addDebugMessage("Twitter account loaded", this.Login.Username, account: this, type: DebugMessage.DebugMessageTypes.Twitter);
                    createNotificationClasses();
                    getLists();
                    getSearches();

                    // start getting TweetMarker
                    getTweetMarkers();

                    LoginSuccessfull = true;
                    return true;
                }
                LoginSuccessfull = false;
                return false;
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp);
                LoginSuccessfull = false;
                return false;
            }
        }

        public void registerAccount()
        {
            AppController.Current.Logger.writeToLogfile("Registering account with Snarl");
            AppController.Current.registerTwitterAccountForNotifications(this.Login.Username);
            AppController.Current.Logger.writeToLogfile("Reading color of account");
            AppController.Current.accountColors.Add(this.Login.Id, new SolidColorBrush(accountColor));
        }

        public string FullName
        {
            get
            {
                return Login.NameAndLogin;
            }
        }

        public Friendship checkFriendship(string screenname)
        {
            if (Friendships.ContainsKey(screenname.ToLower()))
            {
                return Friendships[screenname.ToLower()];
            }
            else
            {
                Friendship friendship = retrieveFriendship(screenname);
                Friendships.Add(screenname.ToLower(), friendship);
                return friendship;
            }
        }

        public bool followUser(string screenname)
        {
            bool success = API.Functions.followUser(this, screenname);
            if (success)
            {
                if (Friendships.ContainsKey(screenname.ToLower()))
                {
                    Friendships[screenname.ToLower()].IsFollowed = true;
                }
                else
                {
                    Friendship friendship = new Friendship();
                    friendship.IsFollowed = true;
                    Friendships.Add(screenname.ToLower(), friendship);
                }
            }
            return success;
        }

        public bool unfollowUser(string screenname)
        {
            bool success = API.Functions.unfollowUser(this, screenname);
            if (success)
            {
                if (Friendships.ContainsKey(screenname.ToLower()))
                {
                    Friendships[screenname.ToLower()].IsFollowed = false;
                }
                else
                {
                    Friendship friendship = new Friendship();
                    friendship.IsFollowed = false;
                    Friendships.Add(screenname.ToLower(), friendship);
                }
            }
            return success;
        }

        private Friendship retrieveFriendship(string screenname)
        {
            Friendship friendship = new Friendship();
            GetFriendshipInfoOptions options = new GetFriendshipInfoOptions();
            options.SourceScreenName = this.username;
            options.TargetScreenName = screenname;

            TwitterFriendship twitterFriendShip = twitterService.GetFriendshipInfo(options);

            if (twitterFriendShip != null)
            {
                TwitterRelationship relationship = twitterFriendShip.Relationship;
                if (relationship.Source.Following)
                {
                    friendship.IsFollowing = true;
                }
                if (relationship.Source.FollowedBy)
                {
                    friendship.IsFollowed = true;
                }
                return friendship;
            }

            return friendship;
        }

        public class Friendship
        {
            public bool IsFollowing { get; set; }
            public bool IsFollowed { get; set; }

            public Friendship()
            {
                IsFollowed = false;
                IsFollowing = false;
            }
        }



        public decimal Id
        {
            get
            {
                if (Login != null)
                {
                    return Login.Id;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string username
        {
            get
            {
                if (Login != null)
                {
                    return Login.Username;
                }
                else
                {
                    return "";
                }
            }
        }


        public string Avatar
        {
            get
            {
                if (this.Login != null)
                {
                    return this.Login.Avatar;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (this.Login != null)
                {
                    this.Login.Avatar = value;
                }
            }
        }


        public List<string> AvailableNotificationClasses
        {
            get;
            private set;
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
            {
                PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;


        public string AccountType
        {
            get { return "Twitter"; }
        }

        public void DeleteTweetFromEverywhere(TwitterItem item, List<TwitterItem> KnownInstances)
        {
            try
            {
                if (item != null)
                {
                    List<TwitterItem> toBeDeletedItems = new List<TwitterItem>();

                    List<ThreadSaveObservableCollection<IItem>> collections = new List<ThreadSaveObservableCollection<IItem>>();
                    collections.Add(Timeline);
                    collections.Add(Mentions);
                    collections.Add(DirectMessages);
                    collections.Add(Retweets);
                    collections.Add(Retweeted);


                    foreach (ThreadSaveObservableCollection<IItem> collection in collections)
                    {
                        foreach (TwitterItem toBeDeletedItem in collection.Where(i => i.Id == item.Id))
                        {
                            toBeDeletedItems.Add(toBeDeletedItem);
                        }

                        foreach (TwitterItem toBeDeletedItem in toBeDeletedItems)
                        {
                            try
                            {
                                collection.Remove(toBeDeletedItem);

                                if (!KnownInstances.Contains(toBeDeletedItem))
                                {
                                    KnownInstances.Add(toBeDeletedItem);
                                }
                            }
                            catch { }
                        }
                    }


                    foreach (Search search in Searches)
                    {
                        search.DeleteTweetFromEverywhere(item, KnownInstances);
                    }

                    foreach (TweetList list in Lists)
                    {
                        list.DeleteTweetFromEverywhere(item, KnownInstances);
                    }

                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
        }

        private bool _tweetMarkerLoadingComplete { get; set; }
        public bool TweetMarkerLoadingCompleted
        {
            get
            {
                return _tweetMarkerLoadingComplete;
            }
            set
            {
                _tweetMarkerLoadingComplete = value;
                NotifyPropertyChanged("TweetMarkerLoadingCompleted");
            }
        }

        public void getTweetMarkers()
        {
            return;
            if (!Properties.Settings.Default.UseTweetmarker)
            {
                return;
            }
            bool tweetMarkerHasBeenChanged = false;
            decimal newId = API.Tweetmarker.getTweetMark(this, "timeline");
            if (newId != 0 && newId != tweetMarkerTimeline)
            {
                tweetMarkerTimeline = newId;
                tweetMarkerHasBeenChanged = true;
            }

            newId = API.Tweetmarker.getTweetMark(this, "mentions");
            if (newId != 0 && newId != tweetMarkerMentions)
            {
                tweetMarkerMentions = newId;
                tweetMarkerHasBeenChanged = true;
            }

            newId = API.Tweetmarker.getTweetMark(this, "messages");
            if (newId != 0 && newId != tweetMarkerDMs)
            {
                tweetMarkerDMs = newId;
                tweetMarkerHasBeenChanged = true;
            }

            newId = API.Tweetmarker.getTweetMark(this, "favorites");
            if (newId != 0 && newId != tweetMarkerFavorites)
            {
                tweetMarkerFavorites = newId;
                tweetMarkerHasBeenChanged = true;
            }

            decimal oldId = 0;
            foreach (TweetList list in Lists)
            {
                oldId = list.tweetMarkerLastKnown;
                newId = list.getTweetMarker();
                if (newId != 0 && newId != oldId)
                {
                    tweetMarkerHasBeenChanged = true;
                }
            }

            foreach (Search search in Searches)
            {
                oldId = search.tweetMarkerLastKnown;
                newId = search.getTweetMarker();
                if (newId != 0 && newId != oldId)
                {
                    tweetMarkerHasBeenChanged = true;
                }
            }
            TweetMarkerLoadingCompleted = true;
            if (tweetMarkerHasBeenChanged)
            {
                AppController.Current.scrollToPositionMarkerNow();
            }
        }

        public void CheckIfAllFetchesAreComplete()
        {
            if (AllInitalFetchesCompleted) { return; }
            bool complete = false;
            complete = InitialUpdateDoneForTimeline && InitialUpdateDoneForRetweets && InitialUpdateDoneForMentions && InitialUpdateDoneForDirectMessages && InitialUpdateDoneForFavorites;
            complete = complete && Searches.TrueForAll(s => s.InitialFetchDone);
            complete = complete && Lists.TrueForAll(l => l.InitialFetchDone);

            if (complete)
            {
                AllInitalFetchesCompleted = true;
                AppController.Current.CheckIfAllAccountsHaveInitialFetchDone();
            }
        }

        ~AccountTwitter()
        {
            backgroundWorkerDirectMessages.CancelAsync();
            backgroundWorkerMentions.CancelAsync();
            backgroundWorkerRetweets.CancelAsync();
            backgroundWorkerTimeline.CancelAsync();
            backgroundWorkerFavorites.CancelAsync();
        }

        public string DebugText
        {
            get
            {
                try
                {
                    return AccountType + " " + username;
                }
                catch (Exception exp)
                {
                    return "Account error: " + exp.Message;
                }
            }
        }

        public SolidColorBrush accountBrush
        {
            get
            {
                if (accountColor != null)
                {
                    return new System.Windows.Media.SolidColorBrush(accountColor);
                }
                else
                {
                    return System.Windows.Media.Brushes.LightBlue;
                }
            }
        }
    }

    public class Twitter_Help_Configuration
    {
        public static int characters_reserved_per_media = 23;
        public static int max_media_per_upload = 1;
        public static List<string> non_username_paths = new List<string>();
        public static decimal photo_size_limit = 3145728;
        public static int short_url_length = 22;
        public static int short_url_length_https = 23;
    }

}
