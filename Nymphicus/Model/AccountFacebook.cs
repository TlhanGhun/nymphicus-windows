using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook;
using System.Dynamic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Nymphicus.API.FacebookAPI;
using System.Windows.Media;

namespace Nymphicus.Model
{
    public class AccountFacebook : IAccount, INotifyPropertyChanged
    {
        private string appId = API.ConnectionData.facebookAppId;
        private string appSecret = API.ConnectionData.facebookAppSecret;

        DateTime newestKnownItem = new DateTime(0);
        public DateTime TokenExpiresAt { get; set; }
        public string AccessToken { get; set; }
        public FacebookClient facebookClient { get; set; }
        public ThreadSaveObservableCollection<FacebookItem> StatusMessages { get; set; }
        public bool InitialUpdateDoneForStatusMessages { get; private set; }
        public ThreadSaveObservableCollection<FacebookItem> Links { get; set; }
        public bool InitialUpdateDoneForLinks { get; private set; }
        public ThreadSaveObservableCollection<FacebookItem> Videos { get; set; }
        public bool InitialUpdateDoneForVideos { get; private set; }
        public ThreadSaveObservableCollection<FacebookItem> Photos { get; set; }
        public bool InitialUpdateDoneForPhotos { get; private set; }
        public ThreadSaveObservableCollection<FacebookItem> Events { get; set; }
        public bool InitialUpdateDoneForEvents { get; private set; }
        public ThreadSaveObservableCollection<FacebookItem> CheckIns { get; set; }
        public bool InitialUpdateDoneForCheckIns { get; private set; }
        public ThreadSaveObservableCollection<FacebookItem> Notes { get; set; }
        public bool InitialUpdateDoneForNotes { get; private set; }
        public FacebookUser User { get; set; }


        private string _username { get; set; }
        private string _id { get; set; }
        public string FullName { get; set; }

        private DateTime LastUpdate { get; set; }

        private bool IsInitialFetch { get; set; }

        // Background worker
        private BackgroundWorker backgroundWorkerNewsFeed;


        public Color accountColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                NotifyPropertyChanged("accountColor");
                
            }
            }
        public Color _backgroundColor { get; set; }

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


        public AccountFacebook()
        {
            StatusMessages = new ThreadSaveObservableCollection<FacebookItem>();
            Links = new ThreadSaveObservableCollection<FacebookItem>();
            Videos = new ThreadSaveObservableCollection<FacebookItem>();
            Photos = new ThreadSaveObservableCollection<FacebookItem>();
            Events = new ThreadSaveObservableCollection<FacebookItem>();
            CheckIns = new ThreadSaveObservableCollection<FacebookItem>();
            Notes = new ThreadSaveObservableCollection<FacebookItem>();
            User = new FacebookUser();

            LoginSuccessfull = false;
            IsInitialFetch = true;
            AvailableNotificationClasses = new List<string>();

            backgroundWorkerNewsFeed = new BackgroundWorker();
            backgroundWorkerNewsFeed.WorkerReportsProgress = true;
            backgroundWorkerNewsFeed.WorkerSupportsCancellation = true;
            backgroundWorkerNewsFeed.DoWork += new DoWorkEventHandler(backgroundWorkerNewsFeed_DoWork);
            backgroundWorkerNewsFeed.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerNewsFeed_RunWorkerCompleted);
            backgroundWorkerNewsFeed.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerNewsFeed_ProgressChanged);

            accountColor = Colors.DarkBlue;
        }

        void backgroundWorkerNewsFeed_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FacebookItem item = (FacebookItem)e.UserState;
            if (item == null)
            {
                return;
            }

            string notificationClassName = "Facebook " + FullName + " ";
            if(item.HasUpdatedLikes || item.HasUpdatedComments) {
                FacebookItem ExistingItem = null;
                try
                {
                
                    switch (item.MessageType) {
                        case FacebookItem.MessageTypes.Link:
                            ExistingItem = Links.Where(oldItem => oldItem.fullId == item.fullId).First();
                      
                            break;

                        case FacebookItem.MessageTypes.Video:
                            ExistingItem = Videos.Where(oldItem => oldItem.fullId == item.fullId).First();
                     
                            break;

                        case FacebookItem.MessageTypes.Photo:
                            ExistingItem = Photos.Where(oldItem => oldItem.fullId == item.fullId).First();
                       
                            break;

                        case FacebookItem.MessageTypes.Note:
                            ExistingItem = Notes.Where(oldItem => oldItem.fullId == item.fullId).First();
                      
                            break;

                        case FacebookItem.MessageTypes.Event:
                            ExistingItem = Events.Where(oldItem => oldItem.fullId == item.fullId).First();
                      
                            break;

                        case FacebookItem.MessageTypes.CheckIn:
                            ExistingItem = CheckIns.Where(oldItem => oldItem.fullId == item.fullId).First();
                      
                            break;

                        default:
                            ExistingItem = StatusMessages.Where(oldItem => oldItem.fullId == item.fullId).First();
                      
                            break;
                    }

                    
                    if(item.HasUpdatedLikes) {
                        string notificationText = "";
                        if (!string.IsNullOrWhiteSpace(item.Text))
                        {
                            notificationText += item.Text.Substring(0, Math.Min(20, item.Text.Length - 1)) + "... ";
                        }
                        else
                        {
                            notificationText += "Facebook item... ";
                        }
                        notificationText += "has new likes";
                        if (item.isLiked)
                        {
                            AppController.Current.sendNotification(notificationClassName + "Like", notificationText, "The item has " + (item.LikesCount - ExistingItem.LikesCount).ToString() + " new likes", item.User.Avatar, null);
                        }
                        else
                        {
                            AppController.Current.sendNotification(notificationClassName + "Like (on message you didn't comment or like yourself)", notificationText, "The item has " + (item.LikesCount - ExistingItem.LikesCount).ToString() + " new likes", item.User.Avatar, null);
                        }
                        ExistingItem.UpdatedAt = item.UpdatedAt;
                        ExistingItem.LikesCount = item.LikesCount;
                        
                    }
                    if(item.HasUpdatedComments) {
                        foreach(FacebookComment newComment in item.Comments) {
                            if (item.isCommented)
                            {
                                AppController.Current.sendNotification(notificationClassName + "Comment", "New comment on entry of " + ExistingItem.User.FullName, newComment.Text + "\r\nby " + newComment.User.FullName, newComment.User.Avatar, null);
                            }
                            else
                            {
                                AppController.Current.sendNotification(notificationClassName + "Comment (on message you didn't comment or like yourself)", "New comment on entry of " + ExistingItem.User.FullName, newComment.Text + "\r\nby " + newComment.User.FullName, newComment.User.Avatar, null);
                            }
                            ExistingItem.Comments.Add(newComment);
                            ExistingItem.UpdatedAt = item.UpdatedAt;
                        }
                    }
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
            else
            {
                switch (item.MessageType)
                {
                    case FacebookItem.MessageTypes.Link:

                        Links.Add(item);
                        if (InitialUpdateDoneForLinks)
                        {
                            notificationClassName += "Link";
                        }

                        break;

                    case FacebookItem.MessageTypes.Video:

                        Videos.Add(item);
                        if (InitialUpdateDoneForVideos)
                        {
                            notificationClassName += "Video";
                        }

                        break;

                    case FacebookItem.MessageTypes.Photo:

                        Photos.Add(item);
                        if (InitialUpdateDoneForPhotos)
                        {
                            notificationClassName += "Photo";
                        }

                        break;

                    case FacebookItem.MessageTypes.Note:

                        Notes.Add(item);
                        if (InitialUpdateDoneForNotes)
                        {
                            notificationClassName += "Note";
                        }

                        break;

                    case FacebookItem.MessageTypes.Event:

                        Events.Add(item);
                        if (InitialUpdateDoneForEvents)
                        {
                            notificationClassName += "Event";
                        }

                        break;

                    case FacebookItem.MessageTypes.CheckIn:

                        CheckIns.Add(item);
                        if (InitialUpdateDoneForCheckIns)
                        {
                            notificationClassName += "CheckIn";
                        }

                        break;

                    default:

                        StatusMessages.Add(item);
                        if (InitialUpdateDoneForStatusMessages)
                        {
                            notificationClassName += "Status message";
                        }

                        break;
                }
            
                if(notificationClassName != "Facebook " + FullName + " ") {
                    AppController.Current.sendNotification(notificationClassName, item.User.FullName, item.Text, item.Avatar, item);
                }
            }

        }

        void backgroundWorkerNewsFeed_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsInitialFetch)
            {
                InitialUpdateDoneForStatusMessages = true;
                InitialUpdateDoneForLinks = true;
                InitialUpdateDoneForVideos = true;
                InitialUpdateDoneForPhotos = true;
                InitialUpdateDoneForEvents = true;
                InitialUpdateDoneForCheckIns = true;
                InitialUpdateDoneForNotes = true;
            }
            IsInitialFetch = false;
            
            LastUpdate = DateTime.Now;
            return;

        }

        void backgroundWorkerNewsFeed_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                dynamic parameters = new ExpandoObject();

                if (!IsInitialFetch && LastUpdate != null)
                {
                    // danach kamen keine Updates mehr durch :(
                  //  parameters.since = GetUnixTimestamp(LastUpdate).ToString();
                }

                string[] feeds = { "home", "feed" };

                foreach (string feedName in feeds)
                {
                    dynamic result = facebookClient.Get("/me/" + feedName, parameters); // Home

                    if (result != null)
                    {
                        foreach (dynamic post in result.data)
                        {
                            if (e != null)
                            {
                                if (e.Cancel)
                                {
                                    return;
                                }
                            }
                            FacebookItem item = FacebookItem.ConvertResponseToItem(post, this);
                            if (item == null)
                            {
                                AppController.Current.Logger.writeToLogfile("Null item in facebook retrieval");
                                continue;
                            }

                            FacebookItem ExistingItem = null;
                            try
                            {

                                switch (item.MessageType)
                                {
                                    case FacebookItem.MessageTypes.Link:
                                        if (Links.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = Links.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;

                                    case FacebookItem.MessageTypes.Video:
                                        if (Videos.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = Videos.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;

                                    case FacebookItem.MessageTypes.Photo:
                                        if (Photos.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = Photos.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;

                                    case FacebookItem.MessageTypes.Note:
                                        if (Notes.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = Notes.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;

                                    case FacebookItem.MessageTypes.Event:
                                        if (Events.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = Events.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;

                                    case FacebookItem.MessageTypes.CheckIn:
                                        if (CheckIns.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = CheckIns.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;

                                    default:
                                        if (StatusMessages.Where(oldItem => oldItem.fullId == item.fullId).Count() > 0)
                                        {
                                            ExistingItem = StatusMessages.Where(oldItem => oldItem.fullId == item.fullId).First();
                                        }

                                        break;
                                }
                            }
                            catch
                            {
                                ExistingItem = null;
                            }
                            if (ExistingItem == null)
                            {

                                try
                                {
                                    dynamic comments = facebookClient.Get("/" + item.fullId + "/comments", parameters);
                                    item.Comments.Clear();
                                    if (comments != null)
                                    {
                                        if (comments.data != null)
                                        {
                                            AppController.Current.Logger.writeToLogfile("Facebook item has comments/data");
                                            foreach (dynamic fbcomment in comments.data)
                                            {
                                                AppController.Current.Logger.writeToLogfile("Reading comment");
                                                FacebookComment comment = new FacebookComment(fbcomment);
                                                comment.Account = item.Account;
                                                if (comment.User.Id == this.Id.ToString())
                                                {
                                                    item.isCommented = true;
                                                }
                                                item.Comments.Add(comment);
                                            }
                                        }
                                    }
                                }
                                catch { }
                                item.ReceivingAccount = this;
                                backgroundWorkerNewsFeed.ReportProgress(100, item);
                                continue;
                            }
                            else
                            {
                   
                                if (item.LikesCount != ExistingItem.LikesCount)
                                {
                                    item.HasUpdatedLikes = true;
                                }

                                List<FacebookComment> updatedComments = new List<FacebookComment>();
                                foreach (FacebookComment comment in item.Comments)
                                {
                                    if (e != null)
                                    {
                                        if (e.Cancel)
                                        {
                                            return;
                                        }
                                    }
                                    if (ExistingItem.Comments.Where(c => c.Text == comment.Text && c.User.Id == comment.User.Id).Count() == 0)
                                    {
                                        updatedComments.Add(comment);
                                    }
                                }

                                if (updatedComments.Count > 0)
                                {
                                    item.HasUpdatedComments = true;
                                    item.Comments.Clear();
                                    foreach (FacebookComment comment in updatedComments)
                                    {
                                        item.Comments.Add(comment);
                                    }
                                }
                                if (item.HasUpdatedComments || item.HasUpdatedLikes)
                                {
                                    item.ReceivingAccount = this;
                                    backgroundWorkerNewsFeed.ReportProgress(100, item);
                                }
                                else
                                {
                                    item = null;
                                }
                            }

                        }
                    }
                }

            }
            catch (FacebookOAuthException oauthExp)
            {
                AppController.Current.Logger.writeToLogfile(oauthExp);
                
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp); ;
            }
        }



        public System.Collections.ObjectModel.ObservableCollection<SubscribableItemsCollection> subscribableItemCollections
        {
            get
            {
                return null;
            }
            set
            {
                return;
            }
        }

        public void createTokens()
        {
            return;
        }

        public void UpdateItems()
        {
            if (LoginSuccessfull && !Properties.Settings.Default.DisableFacebookRetrieval)
            {
                UpdateNewsFeed();

            }
        }

        public void UpdateNewsFeed() {
            if (!backgroundWorkerNewsFeed.IsBusy)
            {
                try
                {
                    backgroundWorkerNewsFeed.RunWorkerAsync();
                }
                catch
                {
                    AppController.Current.Logger.writeToLogfile("Facebook background worker busy");
                }
            }
        }


        
        void WebBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            FacebookClient _fb = new FacebookClient();
            if (_fb.TryParseOAuthCallbackUrl(e.Url, out oauthResult))
            {
                if (oauthResult.IsSuccess)
                {
                    this.AccessToken = oauthResult.AccessToken;
                    UpdateItems();
                }
            }
        }

        public string getStorableSettings()
        {
            string delimiter = "|||";
            string storableString = "Facebook";
            storableString += delimiter + Crypto.EncryptString(Crypto.ToSecureString(AccessToken));
            storableString += delimiter + _username;
            storableString += delimiter + getColorString(accountColor);
            return storableString;
        }

        public void readStorableSettings(string storedSettingsString)
        {
            AppController.Current.Logger.writeToLogfile("Parsing stored Facebook settings");
            try
            {
                string[] delimiter = { "|||" };
                string[] storedData = storedSettingsString.Split(delimiter, StringSplitOptions.None);

                if (storedData.Length < 4)
                {
                    return;
                }

                this.AccessToken = Crypto.ToInsecureString(Crypto.DecryptString(storedData[1]));

                accountColor = getColorFromString(storedData[3]);

                _username = storedData[2];


                if (!verifyCredentials())
                {

                }

            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        public bool verifyCredentials()
        {
            
            try
            {
    


                 IDictionary<string, object> loginParameters = new Dictionary<string, object>
                                                              {
                                                                  { "client_id", appId },
                                                                  { "client_secret", appSecret},
                                                                  { "grant_type", "fb_exchange_token" },
                                                                  { "fb_exchange_token", AccessToken },
                                                                  
                                                              };

                var updateAccessTokenClient = new FacebookClient();

               /* IDictionary<string, object> loginParameters = new Dictionary<string, object>
                                                              { 
                                                                  { "client_id", appId },
                                                                  { "client_secret", appSecret },
                                                                  { "grant_type", "fb_exchange_token" },
                                                                  {"fb_exchange_token", AccessToken}
                                                              }; */


                dynamic updateTokenResult = updateAccessTokenClient.Get("oauth/access_token", 
                                            loginParameters);

                AccessToken = updateTokenResult.access_token;
                facebookClient = new FacebookClient(AccessToken);
                
                dynamic result = (IDictionary<string, object>)facebookClient.Get("me");

                if (result != null)
                {
                    LoginSuccessfull = true;
                    //_id = (decimal)result["id"];
                    _id = result.id;
                    _username = result.username;
                    FullName = result.name;
                    User = FacebookUser.CreateFromDynamic(result, this);

                    AvailableNotificationClasses = new List<string>();
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Status message");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Link");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Photo");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Video");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " CheckIn");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Event");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Note");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Comment");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Comment (on message you didn't comment or like yourself)");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Like");
                    AvailableNotificationClasses.Add("Facebook " + FullName + " Like (on message you didn't comment or like yourself)");

                    foreach (string className in AvailableNotificationClasses)
                    {
                        AppController.Current.registerNotificationClass(className);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FacebookOAuthException oauthExp)
            {
                
                AppController.Current.Logger.writeToLogfile(oauthExp);
                
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp); ;
                return false;
            }
            return false;
        }


        public decimal Id
        {
            get
            {
                try
                {
                    return Convert.ToDecimal(_id);
                }
                catch
                {
                    return 42;
                }
            }
        }

        public string username
        {
            get { return _username; }
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



        public bool LoginSuccessfull
        {
            get;
            set;
        }



        public void registerAccount()
        {
           // AppController.Current.registerAccountForNotifications(this.Login.Username);
            AppController.Current.Logger.writeToLogfile("Reading stored color of Facebook account");
            try
            {
                AppController.Current.accountColors.Add(this.Id, new SolidColorBrush(accountColor));
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Loading of color failed - using Facebook default blue");
                AppController.Current.Logger.writeToLogfile(exp);
                if (AppController.Current.accountColors.ContainsKey(this.Id))
                {
                    AppController.Current.accountColors[this.Id] = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    AppController.Current.accountColors.Add(this.Id, new SolidColorBrush(Colors.Blue));
                }
            }
        }


        public string Avatar
        {
            get
            {
                return "https://graph.facebook.com/" + username + "/picture";
            }
            set { }
        }

        public void LikeItem(FacebookItem item)
        {
            dynamic parameters = new ExpandoObject();
            facebookClient.Post("/" + item.fullId + "/likes",parameters);
        }
        public void UnlikeItem(FacebookItem item)
        {
            dynamic parameters = new ExpandoObject();
            facebookClient.Delete("/" + item.fullId + "/likes", parameters);
        }

        public void LikeComment(FacebookComment comment)
        {
            dynamic parameters = new ExpandoObject();
            facebookClient.Post("/" + comment.Id + "/likes", parameters);
            UpdateNewsFeed();
        }
        public void UnlikeComment(FacebookComment comment)
        {
            dynamic parameters = new ExpandoObject();
            facebookClient.Delete("/" + comment.Id + "/likes", parameters);
            UpdateNewsFeed();
        }

        public void CommentItem(FacebookItem item, string text)
        {
            dynamic parameters = new ExpandoObject();
            parameters.message = text;

            dynamic result = facebookClient.Post("/" + item.fullId + "/comments", parameters);
            item.isCommented = true;
            UpdateNewsFeed();
        }

        public void PostTextStatus(string text)
        {
            dynamic parameters = new ExpandoObject();
            parameters.message = text;
 
            dynamic result = facebookClient.Post("me/feed", parameters);
            UpdateNewsFeed();
        }

        public void PostTextStatusToUserWall(string text, FacebookUser user)
        {
            dynamic parameters = new ExpandoObject();
            parameters.message = text;

            dynamic result = facebookClient.Post("/" + user.Id + "/feed", parameters);
            UpdateNewsFeed();
        }

        public void PostLink(string text, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                PostTextStatus(text);
                return;
            }

            dynamic parameters = new ExpandoObject();
            parameters.message = text;
            parameters.link = System.Web.HttpUtility.UrlEncode(url);

            dynamic result = facebookClient.Post("me/links", parameters);
            UpdateNewsFeed();
        }

        public List<string> AvailableNotificationClasses
        {
            get;
            private set;
        }

        private int GetUnixTimestamp(DateTime date2)
        {
            DateTime date1 = new DateTime(1970, 1, 1);  //Refernzdatum (festgelegt)
            TimeSpan ts = new TimeSpan(date2.Ticks - date1.Ticks);  // das Delta ermitteln
            // Das Delta als gesammtzahl der sekunden ist der Timestamp
            return (Convert.ToInt32(ts.TotalSeconds));
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


        public string AccountType
        {
            get { return "Facebook"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        ~AccountFacebook()
        {
            backgroundWorkerNewsFeed.CancelAsync();
        }

        public string DebugText
        {
            get
            {
                try
                {
                    return "Facebook " + username;
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
                    return System.Windows.Media.Brushes.Blue;
                }
            }
        }
    }
}
