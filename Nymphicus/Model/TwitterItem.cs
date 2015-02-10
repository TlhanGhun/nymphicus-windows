using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;
using TweetSharp;
using System.Collections.ObjectModel;

namespace Nymphicus.Model
{
    public class TwitterItem : INotifyPropertyChanged, IItem
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool parsingOfTextItemsInProgress { get; set; }

        public string Text { 
            get {return _text;}
            set { 
                findThumbnailInText(value);
                findElementsInText(value);
            }
        }
        private string _text { get; set; }

        public bool hasEmbeddedImages
        {
            get
            {
                if (imagesInPost == null)
                {
                    return false;
                }
                if (imagesInPost.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public ObservableCollection<embedded_image> imagesInPost
        {
            get
            {
                return _imagesInPost;
            }
            set
            {
                _imagesInPost = value;
            }
        }
        private ObservableCollection<embedded_image> _imagesInPost { get; set; }

        public bool IsTweetMarker
        {
            get
            { return _isTweetMarker; }
            set
            {
                _isTweetMarker = value;
                NotifyPropertyChanged("IsTweetMarker");
            }
        }
        private bool _isTweetMarker { get; set; }
        public string itemPermaLink
        {
            get
            {
                if (this.Author != null && this.Id != 0)
                {
                    return string.Format("https://twitter.com/{0}/status/{1}",this.Author.Username,this.Id);
                }
                return "https://www.twitter.com";
            }
        }
        public bool IsRead
        {
            get
            {
                return _isRead;
            }
            set
            {
                _isRead = value;
                NotifyPropertyChanged("IsRead");
            }
        }
        private bool _isRead { get; set; }
        public List<TextSubTypes.ISubType> ElementsInText
        {
            get
            {
                if (RetweetedItem != null)
                {
                    return RetweetedItem.ElementsInText;
                }
                else
                {
                    return _elementsInText;
                }
            }
            set
            {
                _elementsInText = value;
            }
        }
        private List<TextSubTypes.ISubType> _elementsInText {get; set;}
        public AccountTwitter OwnAccountHavingWrittenThisTweet { get; set; }
        public bool IsOwnTweet
        {
            get
            {
                if (OwnAccountHavingWrittenThisTweet == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public Model.Person Author { get; set; }
        public string AuthorName
        {
            get
            {
                return Author.Name;
            }
            set { }
        }
        public bool isRetweeted
        {
            get
            {
                if (RetweetedItem != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool isFavorited
        {
            get
            {
                return _isFavorited;
            }
            set
            {
                _isFavorited = value;
                NotifyPropertyChanged("isFavorited");
            }
        }
            private bool _isFavorited
        {
            get;
            set;
        }

        public TweetSharp.TwitterEntities Entities { get; set; }

        public decimal accountId
        {
            get
            {
                if (_retrievingAccount != null)
                {
                    return _retrievingAccount.Id;
                }
                else
                {
                    return _accountId;
                }
            }
            set
            {
                _accountId = value;
            }
        }
        private decimal _accountId { get; set; }
        public AccountTwitter RetrievingAccount
        {
            get
            {

                if (_retrievingAccount == null)
                {
                    return _retrievingAccount = AppController.Current.getAccountForId(accountId);
                }
                return _retrievingAccount;
            }
            set
            {
                _retrievingAccount = value;
            }
        }
        private AccountTwitter _retrievingAccount
        {
            get;
            set;
        }
        public bool isMention { get; set; }
        public bool isDirectMessage { get; set; }
        public bool isRetweetedToMe { get; set; }
        public bool isRetweetedByMe { get; set; }
        public bool isSearchResult { get; set; }
        public bool isList { get; set; }
        public Person DMReceipient { get; set; }
        public string searchName { get; set; }
        public string listName { get; set; }

        public Dictionary<string,string> Images { get; set; }

        public Controls.TextblockItem cachedTextblock { get; set; }

        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;

        public Geo Place {get;set;}
        public bool hasGeoLocation
        {
            get
            {
                if (Place != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public TwitterItem RetweetedItem { get; set; }
        public SolidColorBrush accountColor
        {
            get
            {
                if (this.RetrievingAccount != null)
                {
                    return RetrievingAccount.AccountColorBrush;
                  
                }
                else
                {
                    return Brushes.Blue;
                }
            }
        }



        public decimal Id { 
            get
        {
            return _id;
        }
            set {
                _id = value;
                if(Id > 0) {
                    IsRead = GeneralFunctions.ReadStates.GetCurrentReadStateByTweetId(Id);
                }
            }
        }
        private decimal _id { get; set; }
        public DateTime CreatedAt
        {
            get
            { 
                return _createdAt; 
            }
            set
            {
                if (timerHumanReadableAgo != null)
                {
                    timerHumanReadableAgo.Dispose();
                }
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(value, null));
                }
                _createdAt = value;
            }
        }
        private DateTime _createdAt { get; set; }

        public string HumanReadableAgo
        {
            get
            {
                if (_humanReadableAgo != null)
                {
                    return _humanReadableAgo;
                }
                else
                {
                    return "";
                }
            }
        }
        private string _humanReadableAgo { get; set; }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }
        public Client Source { get; set; }
        public string SourceString
        {
            
            set
            {
                
                Regex regex = new Regex(".* href=\"(?<url>[^\"]*)[^>]*>(?<name>[^<]*).*$");
                MatchCollection matches = regex.Matches(value);
                if (matches.Count > 0)
                {
                    Source.Url = matches[0].Groups["url"].Value;
                    Source.Name = matches[0].Groups["name"].Value;
                }
                else
                {
                    Source.Url = "";
                    Source.Name = value;
                }
            }
        }

        public decimal SourceListId { get; set; }
        public decimal SourceSearchId { get; set; }

        public string  InReplyToScreenname { get; set; }
        public decimal InReplyToStatusId { get; set; }
        public decimal InReplyToUserId { get; set; }
        public bool IsPartOfConversation
        {
            get
            {
                if (isDirectMessage)
                {
                    return true;
                }
                if (InReplyToStatusId == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool? IsTruncated { get; set; }
        public bool IsSentDirectMessage { get; set; }
        public int? RetweetCount { get; set; }
        public bool RetweetCountPlus { get; set; }
        public string RetweetCountString { get; set; }
        public bool IsTwitLongerItem { get; set; }

        TwitterStatus status = new TwitterStatus();

        public TwitterItem()
        {
            ElementsInText = new List<TextSubTypes.ISubType>();
            imagesInPost = new ObservableCollection<embedded_image>();
            Images = new Dictionary<string, string>();

            BackgroundColor = "#aabb55";
            IsSentDirectMessage = false;
            Text = "";
            Source = new Client();
            isFavorited = false;
            isMention = false;
            isDirectMessage = false;
            isRetweetedToMe = false;
            isSearchResult = false;
            isList = false;
            accountId = 0;
            IsTwitLongerItem = false;
            

           // cachedTextblock = new Controls.TextblockItem();
           // cachedTextblock.DataContext = this;

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted); 

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

        #region HumanReadableAgo

        public void timerHumanReadableAgoUpdate(Object state)
        {
            try {
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(_createdAt, _humanReadableAgo));
                }
                else
                {
                    timerHumanReadableAgo.Dispose();
                    timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, 60000, 0);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        private class backgroundWorkerTimeAgoArgument
        {
            public DateTime CreatedAt { get; set; }
            public string OldHumanReadableString { get; set; }

            public backgroundWorkerTimeAgoArgument(DateTime createdAt, string oldHumanReadableString)
            {
                CreatedAt = createdAt;
                OldHumanReadableString = oldHumanReadableString;
            }
        }

        private class backgroundWorkerTimeAgoResult
        {
            public string HumanReadableTimeAgo { get; set; }
            public int SecondsTillNextUpdate { get; set; }
            public bool StopUpdating { get; set; }

            public backgroundWorkerTimeAgoResult(string humanReadableTimeAgo, int secondsTillNextUpdate, bool stopUpdating = false) {
                HumanReadableTimeAgo = humanReadableTimeAgo;
                SecondsTillNextUpdate = secondsTillNextUpdate;
                StopUpdating = stopUpdating;
            }
        }

        void backgroundWorkerTimeAgo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                int nextUpdateInMilliSeconds = 60000;
                if (e.Result != null)
                {
                    try
                    {
                        backgroundWorkerTimeAgoResult result = e.Result as backgroundWorkerTimeAgoResult;
                        _humanReadableAgo = result.HumanReadableTimeAgo;
                        NotifyPropertyChanged("HumanReadableAgo");
                        if (result.StopUpdating)
                        {
                            return;
                        }
                        if (result.SecondsTillNextUpdate < 1)
                        {
                            result.SecondsTillNextUpdate = 120;
                        }
                        nextUpdateInMilliSeconds = result.SecondsTillNextUpdate * 1000;
                        if (result.SecondsTillNextUpdate < 5)
                        {
                            AppController.Current.Logger.writeToLogfile("Immediate refresh of HumanReadableAgo");
                        }
                        if (timerHumanReadableAgo != null)
                        {
                            timerHumanReadableAgo.Dispose();
                        }
                        timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, Math.Max(1000, nextUpdateInMilliSeconds), 0);
                    }
                    catch
                    {
                        _humanReadableAgo = null;
                    }

                }
                else
                {
                    if (timerHumanReadableAgo != null)
                    {
                        timerHumanReadableAgo.Dispose();
                    }
                    timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, Math.Max(1000, nextUpdateInMilliSeconds), 0);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        void backgroundWorkerTimeAgo_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                backgroundWorkerTimeAgoArgument arguments;
                try
                {
                    arguments = e.Argument as backgroundWorkerTimeAgoArgument;
                }
                catch
                {
                    arguments = new backgroundWorkerTimeAgoArgument(DateTime.Now, null);
                }

                if (e.Cancel)
                {
                    return;
                }

                string output = String.Empty;
                bool stopUpdating = false;
                TimeSpan time = DateTime.Now - arguments.CreatedAt;

                int NextUpdateInXSeconds = 60;

                if (time.TotalSeconds <= 0)
                {
                    output += " right now";
                    // if date in the future wait until it in the current time
                    e.Result = new backgroundWorkerTimeAgoResult(output, Math.Min(2,-1  * Convert.ToInt32(time.TotalSeconds)));
                    return;
                }

                if (time.Days > 0)
                {
                    output = arguments.CreatedAt.ToLongDateString();
                    NextUpdateInXSeconds = -1;
                    stopUpdating = true;

                }
                else if (time.Hours > 0)
                {
                   
                    if (time.Hours == 1)
                    {
                        output += "one hour ago";
                    }
                    else
                    {
                        output += time.Hours + " hours ago";
                    }
                    NextUpdateInXSeconds = 10;
                    NextUpdateInXSeconds += (60 - time.Minutes) * 60;
                    NextUpdateInXSeconds += (60 - time.Seconds);
                }

                else if (time.Minutes > 0)
                {
                   
                    if (time.Minutes == 1)
                    {
                        output += "one minute ago ";
                    }
                    else
                    {
                        output += time.Minutes + " minutes ago ";
                    }
                    NextUpdateInXSeconds += (60 - time.Seconds);
                }
                else
                {
                    NextUpdateInXSeconds = 1;
                    if (time.Seconds == 1)
                    {
                        output += time.Seconds + " second ago";
                    }
                    else
                    {
                        output += time.Seconds + " seconds ago";
                    }
                }
                if (string.IsNullOrEmpty(arguments.OldHumanReadableString))
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, stopUpdating);
                }
                else if (output != arguments.OldHumanReadableString)
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, stopUpdating);
                }
                else
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, stopUpdating);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        #endregion

        public override string ToString()
        {
            return Text;
        }

        public string BackgroundColor
        {
            get
            {
                if (!Properties.Settings.Default.ColourizedTimeline)
                {
                    return _bgColor;
                }
                else
                {
                    return "#ffffff";
                }
            }
            set
            {
                _bgColor = value;
                if (BackgroundColor == null)
                {
                    BackgroundColorGradientLight = "#ffffff";
                }
                else
                {
                    Color normalColor = new Color();

                    System.Drawing.Color test = System.Drawing.ColorTranslator.FromHtml(BackgroundColor);
                    normalColor.R = test.R;
                    normalColor.G = test.G;
                    normalColor.B = test.B;

                    HSBColor origColor = HSBColor.FromColor(normalColor);
                    HSBColor gradientStart = origColor;
                    gradientStart.B = gradientStart.B * 1.1;
                    string returnCode = "#FF" + gradientStart.ToColor().R.ToString("X") + gradientStart.ToColor().G.ToString("X") + gradientStart.ToColor().B.ToString("X");
                    BackgroundColorGradientLight = returnCode;
                }

                if (BackgroundColor == null)
                {
                    BackgroundColorGradientDark = "Red";
                }
                else
                {
                    Color normalColor = new Color();
                    System.Drawing.Color test = System.Drawing.ColorTranslator.FromHtml(BackgroundColor);
                    normalColor.R = test.R;
                    normalColor.G = test.G;
                    normalColor.B = test.B;

                    HSBColor origColor = HSBColor.FromColor(normalColor);
                    HSBColor gradientStop = origColor;
                    gradientStop.B = gradientStop.B * 0.9;
                    string returnCode = "#FF" + gradientStop.ToColor().R.ToString("X") + gradientStop.ToColor().G.ToString("X") + gradientStop.ToColor().B.ToString("X");
                    BackgroundColorGradientDark = returnCode;
                }
                NotifyPropertyChanged("BackgroundColor");
            }

        }
        private string _bgColor;
        public string BackgroundColorGradientLight
        {
            get;
            set;
        }
        public string BackgroundColorGradientDark
        {
            get;
            set;
        }

        public class Client
        {
            public string Name { get; set; }
            public string Url { get; set; }

            public Client() {
                Name = "";
                Url = "";
            }
        }

        private void findElementsInText(string fullText)
        {
            if (parsingOfTextItemsInProgress) { return; }
            if (string.IsNullOrEmpty(fullText))
            {
                return;
            }
            parsingOfTextItemsInProgress = true;
            try
            {
                string twitlongerLink = "";
                string twitlongerShortLink = "";
                List<TextSubTypes.ISubType> foundElements = new List<TextSubTypes.ISubType>();
                string[] words = Regex.Split(fullText, @"([\r\n \(\)\{\}\[\];])");
                foreach (string word in words)
                {
                    twitlongerLink = "";
                    if (word.ToLower().StartsWith("http://") || word.ToLower().StartsWith("https://"))
                    {

                        string url = word;
                        string expandedLink = word;

                        if (AppController.Current.AllShortenedLinksInItems.ContainsKey(word))
                        {
                            //AppController.Current.Logger.writeToLogfile("Cached shortened link found for " + word);
                            expandedLink = AppController.Current.AllShortenedLinksInItems[word];
                        }
                        else
                        {
                            //AppController.Current.Logger.writeToLogfile("No cached shortened link found for " + word);
                            // tl.gd wird sonst zu www.twitlonger.com/show expandiert :(
                            if (!word.StartsWith("http://tl.gd/") && 1 == 2)
                            {
                                foreach (API.UrlShortener.ILinkShortener shortener in AppController.Current.AlllinkShortenerServices)
                                {
                                    if (shortener.IsLinkOfThisShortener(word))
                                    {
                                        string unshortedLink = shortener.ExpandLink(word);
                                        if (!string.IsNullOrEmpty(unshortedLink) && unshortedLink != word)
                                        {
                                            expandedLink = unshortedLink;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (expandedLink.StartsWith("http://tl.gd/") || expandedLink.StartsWith("http://www.twitlonger.com/show/"))
                        {
                            twitlongerLink = expandedLink;
                            twitlongerShortLink = word;
                        }

                        if (AppController.Current.AllImagesInItems.ContainsKey(expandedLink))
                        {
                            TextSubTypes.ImageLink imageLink = new TextSubTypes.ImageLink(expandedLink, word, AppController.Current.AllImagesInItems[expandedLink]);
                            foundElements.Add(imageLink);
                        }
                        else
                        {
                            TextSubTypes.Link currentLink = new TextSubTypes.Link(expandedLink, word);
                            foundElements.Add(currentLink);
                        }


                    }
                    else if (word.StartsWith("#") && word.Length > 1)
                    {
                        TextSubTypes.HashTag hashTag = new TextSubTypes.HashTag(word);
                        foundElements.Add(hashTag);
                    }
                    else if (word.StartsWith("@") && word.Length > 1)
                    {
                        TextSubTypes.User user = new TextSubTypes.User(word);
                        foundElements.Add(user);
                    }
                    else
                    {
                        TextSubTypes.Text currentText = new TextSubTypes.Text(word);
                        foundElements.Add(currentText);
                    }
                }
                if (twitlongerLink != "")
                {
                    fullText = fullText.Replace(twitlongerShortLink, twitlongerLink);
                    ExternalServices.Twitlonger.TwitLongerResponse response = ExternalServices.Twitlonger.GetLongText(fullText);
                    if (response != null)
                    {
                        if (!string.IsNullOrEmpty(response.MessageText))
                        {
                            IsTwitLongerItem = true;
                            fullText = response.MessageText;
                            findElementsInText(fullText);
                            return;
                        }
                    }
                }
                ElementsInText.Clear();
                foreach (TextSubTypes.ISubType element in foundElements)
                {
                    ElementsInText.Add(element);
                }
                _text = fullText;
                parsingOfTextItemsInProgress = false;
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
                parsingOfTextItemsInProgress = false;
            }

        }

        private void findThumbnailInText(string fullText) {
            string[] words = Regex.Split(fullText, @"([ \(\)\{\}\[\]])");            
            //xxx das geht doch besser...
            foreach (string word in words)
            {

                if (word.ToLower().StartsWith("http://") || word.ToLower().StartsWith("https://"))
                {
                    foreach (API.ImageServices.IImageService imageService in AppController.Current.AllImageServices)
                    {

                        if (imageService.IsUrlFromThisService(word))
                        {
                            string miniImage = imageService.GetMini(word);
                            if (!string.IsNullOrEmpty(miniImage))
                            {
                                Images.Add(word, miniImage);
                                if (!AppController.Current.AllImagesInItems.ContainsKey(word))
                                {
                                    AppController.Current.AllImagesInItems.Add(word, miniImage);
                                }
                            }
                         
                        }
                        
                    }
                }
            }
        }

        public string TextContent
        {
            get
            {
                return Text;
            }
            set
            {
                Text = value;
            }
        }


        public string ClientName
        {
            get
            {
                if (Source != null)
                {
                    return Source.Name;
                }
                else
                {
                    return "";
                }
            }
        }


        public DateTime UpdatedAt
        {
            get
            {
                return CreatedAt;
            }
            set
            {
                
            }
        }

        public DateTime SortDate
        {
            get { return CreatedAt; }
        }

        public void DeleteThisTweet()
        {
            if (OwnAccountHavingWrittenThisTweet == null)
            {
                return;
            }
            DeleteTweetOptions options = new DeleteTweetOptions();
            options.Id = Convert.ToInt64(this.Id);

            TwitterStatus status = RetrievingAccount.twitterService.DeleteTweet(options);

            if (status != null)
            {
                AppController.Current.sendNotification("General info", "Status has been deleted", "Status has been deleted: " + this.Text, this.Author.Avatar, null);
                AppController.Current.DeleteTweetFromEverywhere(this);
            }
            else
            {
                AppController.Current.sendNotification("General info", "Status delete failed", "Status " + this.Text + " --- delete failed with message: ", this.Author.Avatar, null);
            }
                 
        }




        public bool HumanReadableUpdateInProgress
        {
            get {
                return backgroundWorkerTimeAgo.IsBusy;
            }
        }

 

        public void markRead()
        {
            this.IsRead = true;
            GeneralFunctions.ReadStates.AddTweetAsBeingRead(this.Id);
        }

        public void markUnread()
        {
            this.IsRead = false;
            GeneralFunctions.ReadStates.RemoveTweetAsBeingRead(this.Id);
        }

        ~TwitterItem()
        {
            Author = null;
            if (backgroundWorkerTimeAgo != null)
            {
                if (backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.CancelAsync();
                }
            }
            if (timerHumanReadableAgo != null)
            {
                timerHumanReadableAgo.Dispose();
            }
        }

        public string DebugText
        {
            get
            {
                try
                {
                    return "Twitter " + AuthorName + ": " + Text;
                }
                catch (Exception exp)
                {
                    return "Twitter error item: " + exp.Message;
                }
            }
        }




        public IAccount ReceivingAccount
        {
            get;
            set;
        }

        public class embedded_image
        {
            public string thumbnail_url { get; set; }
            public string url { get; set; }
        }
    }
}
