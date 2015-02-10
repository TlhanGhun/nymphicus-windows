using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AppNetDotNet;
using AppNetDotNet.ApiCalls;
using AppNetDotNet.Model;
using System.Windows.Media;

namespace Nymphicus.Model
{
    public class AccountAppDotNet : IAccount
    {
        public Token token = null;
        public ThreadSaveObservableCollection<ApnItem> PersonalStream { get; set; }
        public ThreadSaveObservableCollection<ApnItem> Mentions { get; set; }
        public ThreadSaveObservableCollection<ApnItem> PrivateMessages { get; set; }
        public ThreadSaveObservableCollection<ApnItem> OtherMessages { get; set; }
        public ThreadSaveObservableCollection<ApnItem> Reposts { get; set; }
        public List<Channel> KnownChannels { get; set; }
        public List<string> KnownChannelIds { get; set; }

        private Streaming.UserStream userStream { get; set; }
        private bool streamingIsActive { get; set; }

        public bool allInitialFetchesHaveBeenCompleted { get; set; }
        public bool lastFetechCompleted { get; set; }
        public bool InitialUpdateForPersonalStream { get; set; }
        public bool InitialUpdateForMention { get; set; }
        public bool InitialUpdateForMessages { get; set; }
        public bool isFirstFetch { get; set; }
        public bool showNotifications { get; set; }

        private BackgroundWorker backgroundWorkerPersonalStream;
        private BackgroundWorker backgroundWorkerMentions;
        private BackgroundWorker backgroundWorkerMessages;
        public bool PersonalStreamIsBusy { get; set; }
        public bool MentionsIsBusy { get; set; }
        public bool MessagesIsBusy { get; set; }
        public ApnItem myStreamItemWithUpdatedStreamMarker { get; set; }
        public ApnItem mentionItemWithUpdatedStreamMarker { get; set; }

        #region Store Marker variables

        public bool streamMarkerUpdateCompleted { get; set; }
        public bool streamMarkerNewMarkerReceived { get; set; }

        public string storeMarkerVersionGeneral
        {
            get
            {
                return _storeMarkerVersionGeneral;
            }
            set
            {
                if (value != _storeMarkerVersionGeneral)
                {
                    _storeMarkerVersionGeneral = value;
                    if (value != null)
                    {
                        streamMarkerNewMarkerReceived = true;
                    }
                }
                
            }
        }
        private string _storeMarkerVersionGeneral { get; set; }
        public string storeMarkerIdGeneral { get; set; }
        public bool streamMarkerGeneralUpdateComplete { get; set; }
        public bool streamMarkerGeneralOwnUpdate { get; set; }

        public string storeMarkerVersionMyStream
        {
            get
            {
                return _storeMarkerVersionMyStream;
            }
            set
            {
                if (value != _storeMarkerVersionMyStream)
                {
                    _storeMarkerVersionMyStream = value;

                    if (streamMarkerMyStreamOwnUpdate)
                    {
                        streamMarkerMyStreamOwnUpdate = false;
                        //AppController.Current.sendNotification("General", "My stream own update", "", "", null);
                    }
                    else if (value != null)
                    {
                        streamMarkerNewMarkerReceived = true;
                        IEnumerable<ApnItem> oldMarker;
                        try
                        {
                            oldMarker = PersonalStream.Where(item => item.isStreamMarker);
                        }
                        catch { oldMarker = null; }
                        if (oldMarker != null)
                        {
                            foreach (ApnItem item in oldMarker)
                            {
                                item.isStreamMarker = false;
                            }
                        }
                        try
                        {
                            ApnItem newMarker = PersonalStream.Where(item => item.Id.ToString() == storeMarkerIdMyStream).First();
                            if (newMarker != null)
                            {
                                newMarker.isStreamMarker = true;
                                myStreamItemWithUpdatedStreamMarker = newMarker;
                            }
                        }
                        catch { }
                        checkIfStreamMarkersAllCompleted();
                    }
                }
            }
        }
        private string _storeMarkerVersionMyStream { get; set; }
        public string storeMarkerIdMyStream { get; set; }
        public bool streamMarkerMyStreamUpdateComplete { get; set; }
        public bool streamMarkerMyStreamOwnUpdate { get; set; }

        public string storeMarkerVersionMentions
        {
            get
            {
                return _storeMarkerVersionMentions;
            }
            set
            {
                if (value != _storeMarkerVersionMentions)
                {
                    _storeMarkerVersionMentions = value;
                    if (streamMarkerMentionsOwnUpdate)
                    {
                        streamMarkerMentionsOwnUpdate = false;
                        AppController.Current.sendNotification("General", "Mentions own update", "", "", null);
                    }
                    else if (value != null)
                    {
                        streamMarkerNewMarkerReceived = true;
                        IEnumerable<ApnItem> oldMarker;
                        try
                        {
                            oldMarker = Mentions.Where(item => item.isStreamMarker);
                        }
                        catch {
                            oldMarker = null;
                        }
                        if (oldMarker != null)
                        {
                            foreach (ApnItem item in oldMarker)
                            {
                                item.isStreamMarker = false;
                            }
                        }
                        try
                        {
                            ApnItem newMarker = Mentions.Where(item => item.Id.ToString() == storeMarkerIdMentions).First();
                            if (newMarker != null)
                            {
                                newMarker.isStreamMarker = true;
                                mentionItemWithUpdatedStreamMarker = newMarker;
                            }
                        }
                        catch { }
                        checkIfStreamMarkersAllCompleted();
                    }
                }
            }
        }
        private string _storeMarkerVersionMentions { get; set; }
        public string storeMarkerIdMentions { get; set; }
        public bool streamMarkerMentionsUpdateComplete { get; set; }
        public bool streamMarkerMentionsOwnUpdate { get; set; }

        public string storeMarkerVersionMessages
        {
            get
            {
                return _storeMarkerVersionMessages;
            }
            set
            {
                if (value != _storeMarkerVersionMessages)
                {
                    _storeMarkerVersionMessages = value;
                    if (streamMarkerMessagesOwnUpdate)
                    {
                        streamMarkerMessagesOwnUpdate = false;
                        AppController.Current.sendNotification("General", "Messages own update", "", "", null);
                    }
                    else if (value != null)
                    {
                        streamMarkerNewMarkerReceived = true;
                        IEnumerable<ApnItem> oldMarker = null;
                        try
                        {
                            oldMarker = PrivateMessages.Where(item => item.isStreamMarker);
                        }
                        catch { }
                        if (oldMarker != null)
                        {
                            foreach (ApnItem item in oldMarker)
                            {
                                item.isStreamMarker = false;
                            }
                        }
                        try
                        {
                            ApnItem newMarker = PrivateMessages.Where(item => item.Id.ToString() == storeMarkerIdMessages).First();
                            if (newMarker != null)
                            {
                                newMarker.isStreamMarker = true;
                                mentionItemWithUpdatedStreamMarker = newMarker;
                            }
                        }
                        catch { }
                        checkIfStreamMarkersAllCompleted();
                    }
                }
            }
        }
        private string _storeMarkerVersionMessages { get; set; }
        public string storeMarkerIdMessages { get; set; }
        public bool streamMarkerPrivateMessagesUpdateComplete { get; set; }
        public bool streamMarkerMessagesOwnUpdate { get; set; }
        #endregion

        public AccountAppDotNet()
        {
            isFirstFetch = true;

            PersonalStream = new ThreadSaveObservableCollection<ApnItem>();
            Mentions = new ThreadSaveObservableCollection<ApnItem>();
            PrivateMessages = new ThreadSaveObservableCollection<ApnItem>();
            OtherMessages = new ThreadSaveObservableCollection<ApnItem>();
            Reposts = new ThreadSaveObservableCollection<ApnItem>();
            KnownChannels = new List<Channel>();
            KnownChannelIds = new List<string>();

            backgroundWorkerPersonalStream = new BackgroundWorker();
            backgroundWorkerPersonalStream.WorkerReportsProgress = true;
            backgroundWorkerPersonalStream.WorkerSupportsCancellation = true;
            backgroundWorkerPersonalStream.DoWork += backgroundWorkerPersonalStream_DoWork;
            backgroundWorkerPersonalStream.RunWorkerCompleted += backgroundWorkerPersonalStream_RunWorkerCompleted;
            backgroundWorkerPersonalStream.ProgressChanged += backgroundWorkerPersonalStream_ProgressChanged;

            backgroundWorkerMentions = new BackgroundWorker();
            backgroundWorkerMentions.WorkerReportsProgress = true;
            backgroundWorkerMentions.WorkerSupportsCancellation = true;
            backgroundWorkerMentions.DoWork += backgroundWorkerMentions_DoWork;
            backgroundWorkerMentions.RunWorkerCompleted += backgroundWorkerMentions_RunWorkerCompleted;
            backgroundWorkerMentions.ProgressChanged += backgroundWorkerMentions_ProgressChanged;

            backgroundWorkerMessages = new BackgroundWorker();
            backgroundWorkerMessages.WorkerReportsProgress = true;
            backgroundWorkerMessages.WorkerSupportsCancellation = true;
            backgroundWorkerMessages.DoWork += backgroundWorkerMessages_DoWork;
            backgroundWorkerMessages.RunWorkerCompleted += backgroundWorkerMessages_RunWorkerCompleted;
            backgroundWorkerMessages.ProgressChanged += backgroundWorkerMessages_ProgressChanged;
        }

        #region Background worker

        #region personal stream

        void backgroundWorkerPersonalStream_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 50:
                        ApnItem item = e.UserState as ApnItem;
                        if (item != null)
                        {
                            PersonalStream.Add(item);
                            if (showNotifications)
                            {
                                AppController.Current.sendNotification("App.net " + username + " personal stream", item.AuthorName, item.Text, item.Avatar, item);
                            }
                        }
                        break;

                    case 99:
                        ApiCallResponse apiCallResponse = e.UserState as ApiCallResponse;
                        streamMarkerMyStreamUpdateComplete = true;
                        if (apiCallResponse != null)
                        {
                            if (apiCallResponse.meta != null)
                            {
                                if (apiCallResponse.meta.marker != null)
                                {
                                    storeMarkerIdMyStream = apiCallResponse.meta.marker.id;
                                    storeMarkerVersionMyStream = apiCallResponse.meta.marker.version;
                                }
                            }
                        }

                        if (PersonalStream.Count > 0)
                        {
                            InitialUpdateForPersonalStream = true;
                        }
                        streamMarkerMyStreamUpdateComplete = true;

                        break;
                }
            }
        }

        void backgroundWorkerPersonalStream_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        void backgroundWorkerPersonalStream_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = Properties.Settings.Default.TwitterItemsFetchInPast;
            if (PersonalStream.Count > 0)
            {
                parameter.since_id = PersonalStream.Max(i => i.Id).ToString();
            }
            parameter.include_annotations = true;
            items = SimpleStreams.getUserStream(this.accessToken, parameter);
                
            if (items.Item2.success)
            {
                foreach (Post post in items.Item1)
                {
                    if (!post.machine_only && !string.IsNullOrEmpty(post.text) && !post.is_deleted)
                    {
                        ApnItem item = new ApnItem(post, this);
                        if (item != null)
                        {
                            item.receivingAccount = this;
                            backgroundWorkerPersonalStream.ReportProgress(50, item);
                        }
                    }
                }
                if (items.Item1 != null)
                {
                    backgroundWorkerPersonalStream.ReportProgress(99, items.Item2);
                }
            }
        }

        #endregion

        #region mentions

        void backgroundWorkerMentions_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
             if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 50:
                        ApnItem item = e.UserState as ApnItem;
                        Mentions.Add(item);
                        if (showNotifications)
                        {
                            AppController.Current.sendNotification("App.net " + username + " mentions", item.AuthorName, item.Text, item.Avatar, item);
                        }
                        break;

                    case 99:
                        ApiCallResponse apiCallResponse = e.UserState as ApiCallResponse;
                        streamMarkerMentionsUpdateComplete = true;
                        if (apiCallResponse != null)
                        {
                            if (apiCallResponse.meta != null)
                            {
                                if (apiCallResponse.meta.marker != null)
                                {
                                    storeMarkerIdMentions = apiCallResponse.meta.marker.id;
                                    storeMarkerVersionMentions = apiCallResponse.meta.marker.version;
                                }
                            }
                        }
                        if (Mentions.Count > 0)
                        {
                            InitialUpdateForMention = true;
                        }
                        streamMarkerMentionsUpdateComplete = true;
                        break;
                }
            }
        }

        void backgroundWorkerMentions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        void backgroundWorkerMentions_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = Properties.Settings.Default.TwitterItemsFetchInPast;
            if (Mentions.Count > 0)
            {
                parameter.since_id = Mentions.Max(i => i.Id).ToString();    
            }
            parameter.include_annotations = true;
            items = Posts.getMentionsOfUsernameOrId(this.accessToken, this.username, parameter);
            
            if (items.Item2.success)
            {
                foreach (Post post in items.Item1)
                {
                    if (!post.machine_only && !string.IsNullOrEmpty(post.text) && !post.is_deleted)
                    {
                        ApnItem item = new ApnItem(post, this);
                        if (item != null)
                        {
                            item.isMention = true;
                            item.receivingAccount = this;
                            backgroundWorkerMentions.ReportProgress(50, item);
                        }
                    }
                }
                backgroundWorkerMentions.ReportProgress(99, items.Item2);

            }
        }

        #endregion

        #region messages

        void backgroundWorkerMessages_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 20:
                        // Channel list
                        KnownChannels = e.UserState as List<Channel>;
                        if (KnownChannels != null)
                        {
                            foreach (Channel channel in KnownChannels)
                            {
                                if (KnownChannelIds.Contains(channel.id))
                                {
                                    KnownChannelIds.Add(channel.id);
                                }
                            }
                        }
                        break;

                    case 50:
                        ApnItem item = e.UserState as ApnItem;
                        PrivateMessages.Add(item);
                        // || true muss raus
                        if (showNotifications)
                        {
                            AppController.Current.sendNotification("App.net " + username + " private messages", item.AuthorName, item.Text, item.Avatar, item);
                        }
                        
                        break;

                    case 99:
                        streamMarkerPrivateMessagesUpdateComplete = true;
                        InitialUpdateForMessages = true;
                        break;
                }
            }
        }

        void backgroundWorkerMessages_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        void backgroundWorkerMessages_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Message> messages = new List<Message>();

            Tuple<List<Message>, ApiCallResponse> items;
            Tuple<List<Channel>, ApiCallResponse> channels = Channels.Subscriptions.getOfCurrentUser(this.accessToken);
            if (channels.Item2.success)
            {
                backgroundWorkerMessages.ReportProgress(20, channels.Item1);
            }
            else
            {
                return;
            }


            DateTime newestKnownDateTime = DateTime.MinValue;
            if (PrivateMessages.Count > 0)
            {
                newestKnownDateTime = PrivateMessages.Max(m => m.CreatedAt);
            }

            foreach (Channel channel in channels.Item1)
            {
                if (channel.type == "net.app.core.pm")
                {
                    Messages.messageParameters parameter = new Messages.messageParameters();
                    parameter.include_annotations = true;
                    items = Messages.getMessagesInChannel(this.accessToken, channel.id, parameters:parameter);
                    if (items.Item2.success)
                    {
                        foreach (Message message in items.Item1)
                        {
                            if (message.created_at > newestKnownDateTime)
                            {
                                messages.Add(message);
                            }
                        }
                    }
                }
            }

            foreach (Message message in messages)
            {
                if (!string.IsNullOrEmpty(message.text) && !message.is_deleted && !message.machine_only)
                {
                    ApnItem item = new ApnItem(message, this);
                    if (item != null)
                    {
                        item.isPrivateMessage = true;
                        item.receivingAccount = this;
                        backgroundWorkerMessages.ReportProgress(50, item);
                    }
                }
            }

            backgroundWorkerMessages.ReportProgress(99, null);
        }

        #endregion

        #endregion

        public User user { get; set; }

        public string Avatar
        {
            get
            {
                if (user != null)
                {
                    return user.avatar_image.url;
                }
                else
                {
                    return "";
                }
            }
            set { }
        }

        public string FullName
        {
            get
            {
                if (user != null)
                {
                    return user.name + " (@" + user.username + ")";
                }
                else
                {
                    return "";
                }
            }
        }

        public  string username {
            get
            {
                if (user != null)
                {
                    return user.username;
                }
                else
                {
                    return "Unauthorized";
                }
            }
        }

        public string accessToken
        {
            get
            {
                return _accessToken;
            }
            set
            {
                _accessToken = value;
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    Tuple<Token, ApiCallResponse> response = Tokens.get(_accessToken);
                    if (response.Item2.success)
                    {
                        token = response.Item1;
                        user = token.user;
                    }
                }
            }
        }
        private string _accessToken { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<SubscribableItemsCollection> subscribableItemCollections
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        public decimal Id
        {
            get {
                if (token == null)
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt32(token.user.id);
                }
            }
        }

        public System.Windows.Media.Color accountColor
        {
            get;
            set;
        }

        public bool LoginSuccessfull
        {
            get
            {
                return (user != null);
            }
            set
            {
                
            }
        }


        public List<string> AvailableNotificationClasses
        {
            get {
                List<string> classes = new List<string>();
                classes.Add("App.net " + username + " personal stream"); 
                classes.Add("App.net " + username + " mentions");
                classes.Add("App.net " + username + " private messages");
                classes.Add("App.net " + username + " messages");
                return classes;
            }
        }

        public string AccountType
        {
            get { return "App.net"; }
        }

        public string DebugText
        {
            get { return ""; }
        }

        public void registerAccount()
        {
            AppController.Current.Logger.writeToLogfile("Reading stored color of App.net account");
            try
            {
                foreach (string className in AvailableNotificationClasses)
                {
                    AppController.Current.registerNotificationClass(className);
                }
                AppController.Current.accountColors.Add(this.Id, new SolidColorBrush(accountColor));
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Loading of color failed - using App.net default dark grey");
                AppController.Current.Logger.writeToLogfile(exp);
                if (AppController.Current.accountColors.ContainsKey(this.Id))
                {
                    AppController.Current.accountColors[this.Id] = new SolidColorBrush(Colors.DarkGray);
                }
                else
                {
                    AppController.Current.accountColors.Add(this.Id, new SolidColorBrush(Colors.DarkGray));
                }
            }
        }

        public void UpdateItems()
        {
            if (!isFirstFetch)
            {
                showNotifications = true;
            }
            streamMarkerNewMarkerReceived = false;
            streamMarkerGeneralUpdateComplete = false;
            streamMarkerMentionsUpdateComplete = false;
            streamMarkerMyStreamUpdateComplete = false;
            streamMarkerPrivateMessagesUpdateComplete = false;

            lastFetechCompleted = false;

            if (userStream == null)
            {
                UpdatePersonalStream();
                UpdateMentions();
                UpdateMessages();
            }

            // not implemented so always completed
            streamMarkerGeneralUpdateComplete = true;
            isFirstFetch = false;

            startStreaming();
        }

        private void startStreaming()
        {
            if(!streamingIsActive) {
                StreamingOptions streamingOptions = new StreamingOptions();
                streamingOptions.include_annotations = true;
                streamingOptions.include_html = false;
                streamingOptions.include_marker = true;
                streamingOptions.include_channel_annotations = false;
                streamingOptions.include_message_annotations = true;
                streamingOptions.include_post_annotations = true;
                streamingOptions.include_user_annotations = true;

                userStream = new Streaming.UserStream(this.accessToken, streamingOptions);
            
                IAsyncResult asyncResult = userStream.StartUserStream(
                    streamCallback: streamCallback,
                    unifiedCallback: unifiedCallback,
                    mentionsCallback: mentionsCallback,
                    channelsCallback: channelsCallback,
                    streamStoppedCallback: streamStoppedCallback);

                SubscriptionOptions subscriptionOptions = new SubscriptionOptions();
                subscriptionOptions.include_deleted = true;
                subscriptionOptions.include_incomplete = false;
                subscriptionOptions.include_muted = false;
                subscriptionOptions.include_private = true;
                subscriptionOptions.include_read = true;

                //userStream.subscribe_to_endpoint(userStream.available_endpoints["Unified"]);
                userStream.subscribe_to_endpoint(userStream.available_endpoints["Stream"]);
                userStream.subscribe_to_endpoint(userStream.available_endpoints["Mentions"]);
                userStream.subscribe_to_endpoint(userStream.available_endpoints["Channels"]);

                streamingIsActive = true;
            }
        
        }


        #region Streaming callbacks

        public void streamCallback(List<Post> posts, bool is_deleted = false)
        {
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    if (post == null)
                    {
                        continue;
                    }
                    if (post.machine_only || string.IsNullOrEmpty(post.text))
                    {
                        continue;
                    }
                    if (!post.is_deleted)
                    {
                        ApnItem item = new ApnItem(post, this);
                        item.receivingAccount = this;
                        PersonalStream.Add(item);
                        AppController.Current.sendNotification("App.net " + username + " personal stream", item.AuthorName, item.Text, item.Avatar, item);
                    }
                    else
                    {
                        IEnumerable<ApnItem> existing_items = PersonalStream.Where(item => item.Id.ToString() == post.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<ApnItem> cache = new List<ApnItem>();
                                foreach (ApnItem item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (ApnItem item in cache)
                                {
                                    PersonalStream.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
        }

        public void unifiedCallback(List<Post> posts, bool is_deleted = false)
        {
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    if (post == null)
                    {
                        continue;
                    }
                    if (post.machine_only || string.IsNullOrEmpty(post.text))
                    {
                        continue;
                    }
                    if (!post.is_deleted)
                    {
                        ApnItem item = new ApnItem(post, this);
                        item.receivingAccount = this;
                        PersonalStream.Add(item);
                    }
                    else
                    {
                        IEnumerable<ApnItem> existing_items = PersonalStream.Where(item => item.Id.ToString() == post.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<ApnItem> cache = new List<ApnItem>();
                                foreach (ApnItem item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (ApnItem item in cache)
                                {
                                    PersonalStream.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
        }

        public void mentionsCallback(List<Post> posts, bool is_deleted = false)
        {
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    if (post == null)
                    {
                        continue;
                    }
                    if (post.machine_only || string.IsNullOrEmpty(post.text))
                    {
                        continue;
                    }
                    if (!post.is_deleted)
                    {
                        ApnItem item = new ApnItem(post, this);
                        item.receivingAccount = this;
                        Mentions.Add(item);
                        AppController.Current.sendNotification("App.net " + username + " mentions", item.AuthorName, item.Text, item.Avatar, item);
                    }
                    else
                    {
                        IEnumerable<ApnItem> existing_items = Mentions.Where(item => item.Id.ToString() == post.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<ApnItem> cache = new List<ApnItem>();
                                foreach (ApnItem item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (ApnItem item in cache)
                                {
                                    Mentions.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
            
        }

        public void channelsCallback(List<Message> messages, bool is_deleted = false)
        {
            if (messages != null)
            {
                foreach (Message message in messages)
                {
                    if (message == null)
                    {
                        continue;
                    }
                    if (message.machine_only || string.IsNullOrEmpty(message.text))
                    {
                        continue;
                    }
                    if (message.channel_id != "net.app.core.pm")
                    {
                        // PM for now
                        continue;
                    }
                    if (!message.is_deleted)
                    {
                        ApnItem item = new ApnItem(message, this);
                        item.receivingAccount = this;
                        PrivateMessages.Add(item);
                    }
                    else
                    {
                        IEnumerable<ApnItem> existing_items = PrivateMessages.Where(item => item.Id.ToString() == message.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<ApnItem> cache = new List<ApnItem>();
                                foreach (ApnItem item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (ApnItem item in cache)
                                {
                                    PrivateMessages.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
        }

        public void streamStoppedCallback(Streaming.StopReasons reason)
        {
            Console.WriteLine(reason);
            Console.WriteLine(userStream.last_error);
            streamingIsActive = false;
        }


        #endregion

        public void UpdatePersonalStream()
        {
            if (!backgroundWorkerPersonalStream.IsBusy)
            {
                PersonalStreamIsBusy = false;
                myStreamItemWithUpdatedStreamMarker = null;
                backgroundWorkerPersonalStream.RunWorkerAsync();
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Personal stream background thread of @" + username + " is busy");
                if (PersonalStreamIsBusy)
                {
                    AppController.Current.Logger.writeToLogfile("Personal stream background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerPersonalStream.CancelAsync();
                }
                PersonalStreamIsBusy = true;
            }
        }

        public void UpdateMentions()
        {
            if (!backgroundWorkerMentions.IsBusy)
            {
                mentionItemWithUpdatedStreamMarker = null;
                MentionsIsBusy = false;
                backgroundWorkerMentions.RunWorkerAsync();
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Mention background thread of @" + username + " is busy");
                if (MentionsIsBusy)
                {
                    AppController.Current.Logger.writeToLogfile("Mention background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerMentions.CancelAsync();
                }
                MentionsIsBusy = true;
            }
        }

        public void UpdateMessages()
        {
            if (!backgroundWorkerMessages.IsBusy)
            {
                MessagesIsBusy = false;
                backgroundWorkerMessages.RunWorkerAsync();
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Messages background thread of @" + username + " is busy");
                if (MessagesIsBusy)
                {
                    AppController.Current.Logger.writeToLogfile("Messages background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerMessages.CancelAsync();
                }
                MessagesIsBusy = true;
            }
        }

        public string getStorableSettings()
        {
            string delimiter = "|||";
            string storableString = "App.Net";
            storableString += delimiter + Crypto.EncryptString(Crypto.ToSecureString(accessToken));
            storableString += delimiter + getColorString(accountColor);
            storableString += delimiter + username;
            return storableString;
        }

        public void checkIfStreamMarkersAllCompleted()
        {
            if (streamMarkerPrivateMessagesUpdateComplete && streamMarkerGeneralUpdateComplete && streamMarkerMentionsUpdateComplete && streamMarkerMyStreamUpdateComplete)
            {
                streamMarkerUpdateCompleted = true;
                if (!allInitialFetchesHaveBeenCompleted)
                {
                    allInitialFetchesHaveBeenCompleted = true;
                    AppController.Current.CheckIfAllAccountsHaveInitialFetchDone();
                }
                else
                {
                    lastFetechCompleted = true;
                    if (streamMarkerNewMarkerReceived)
                    {
                        AppController.Current.liveUpdateScrollPosition();
                    }
                }
            }
        }

        #region Colors

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

        #endregion

        public void readStorableSettings(string storedSettingsString)
        {
            try
            {
                string[] delimiter = { "|||" };
                string[] storedData = storedSettingsString.Split(delimiter, StringSplitOptions.None);

                if (storedData.Length < 4)
                {
                    this.LoginSuccessfull = false;
                    return;
                }

                this.accessToken = Crypto.ToInsecureString(Crypto.DecryptString(storedData[1]));
                
                this.accountColor = getColorFromString(storedData[2]);
    
                if (!verifyCredentials())
                {
                    this.LoginSuccessfull = false;
                }

            }
            catch 
            {
                this.LoginSuccessfull = false;
            }
        }

        public bool verifyCredentials()
        {
            return (user != null);
        }

        public static void authorizeNewAccount()
        {
            AppNetDotNet.Model.Authorization.clientSideFlow apnClientAuthProcess = new AppNetDotNet.Model.Authorization.clientSideFlow(API.ConnectionData.appDotNetSecret, API.ConnectionData.appDotNetRedirectUrl, "basic stream write_post follow messages files update_profile");
            apnClientAuthProcess.AuthSuccess += apnAuthProcess_AuthSuccess;
            apnClientAuthProcess.showAuthWindow();
        }

        static void apnAuthProcess_AuthSuccess(object sender, AppNetDotNet.AuthorizationWindow.AuthEventArgs e)
        {
            if (e != null)
            {
                if (e.success)
                {
                    AccountAppDotNet account = new AccountAppDotNet();
                    account.accessToken = e.accessToken;
                    if (account.verifyCredentials())
                    {
                        AppController.Current.AllAccounts.Add(account);
                        account.UpdateItems();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(":(", "Authorization failed");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(e.error, "Authorization failed");
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
                    return System.Windows.Media.Brushes.Green;
                }
            }
        }
        
    }
}
