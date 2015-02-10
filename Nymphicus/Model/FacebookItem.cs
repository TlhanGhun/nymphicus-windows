using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;

namespace Nymphicus.Model
{

    public class FacebookItem : INotifyPropertyChanged, IItem
    {
        public enum MessageTypes
        {
            StatusMessage,
            Link,
            Photo,
            Video,
            Event,
            CheckIn,
            Note
        }



        public bool isLiked
        {
            get
            {
                return _isLiked;
            }
            set
            {
                _isLiked = value;
                NotifyPropertyChanged("isLiked");
            }
                }
        private bool _isLiked { get; set; }

        public bool isCommented
        {
            get
            {
                return _isCommented;
            }
            set
            {
                _isCommented = value;
                NotifyPropertyChanged("isCommented");
            }
        }
        private bool _isCommented { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasUpdatedComments { get; set; }
        public bool HasUpdatedLikes { get; set; }

        public bool ShowComments
        {
            get
            {
                return _showComments;
            }
            set
            {
                _showComments = value;
                NotifyPropertyChanged("CommentsDisplayText");
                NotifyPropertyChanged("ShowComments");         
            }
        }

        public bool _showComments { get; set; }

        public bool IsUpdatedExistingItem { get; set; }

        public List<TextSubTypes.ISubType> ElementsInText { get; set; }
        public string Text
        {
            get
            {
                if (_text != null)
                {
                    return _text;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _text = value;
                findElementsInText(value);
            }
        }
        private string _text
        {
            get;
            set;
        }

        public MessageTypes MessageType { get; set; }

        public DateTime CreatedAt
        {
            get
            {
                return _createdAt;
            }
            set
            {
                _createdAt = value;
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(value, null));
                }
                NotifyPropertyChanged("SortDate");
            }
        }
        private DateTime _createdAt { get; set; }


        public DateTime UpdatedAt
        {
            get
            {
                return _updatedAt;
            }
            set
            {
                _updatedAt = value;
                NotifyPropertyChanged("SortDate");
            }
        }

        private DateTime _updatedAt { get; set; }
        public FacebookApplication Application { get; set; }
        public bool HasApplication
        {
            get
            {
                if (Application != null)
                {
                    return (!string.IsNullOrEmpty(Application.Name));
                }
                return false;
            }
        }
        public FacebookUser User { get; set; }
        public FacebookUser To { get; set; }

        public ObservableCollection<FacebookUser> Likes { get; set; }
        public ObservableCollection<FacebookComment> Comments { get; set; }
        public int CommentsCount { get
        {
            return _commentsCount;
        }
            private set
            {
                _commentsCount = value;
                NotifyPropertyChanged("CommentsDisplayText");
                NotifyPropertyChanged("CommentsCount");
            }
        }
        private int _commentsCount { get;set; }
        public string CommentsDisplayText
        {
            get
            {
                string returnValue = "No comments";

                if (CommentsCount == 1) 
                {
                    returnValue = "One comment";
                } 
                else if (CommentsCount > 1) 
                {
                    returnValue = CommentsCount.ToString() + " comments";
                }

                if (_showComments)
                {
                    returnValue += " - click to hide ⇑";
                }
                else
                {
                    returnValue += " - click to show ⇓";
                }

                return returnValue;
            }
        }

        public string ApplicationName { get { return Application.Name; } }
        public string ApplicationLink { get { return Application.Link; } }

        public string Caption { get; set; }
        public bool HasCaption
        {
            get
            {
                return (!string.IsNullOrEmpty(Caption));
            }
        }
        public string Description { get; set; }
        public bool HasDescription
        {
            get
            {
                return (!string.IsNullOrEmpty(Description));
            }
        }

        public bool HasImageCaptionOrDescription
        {
            get
            {
                return (!string.IsNullOrEmpty(Picture) || HasCaption || HasDescription);
            }
        }

        public string AuthorName
        {
            get
            {
                return User.FullName;
            }
            set { }
        }

        public int LikesCount
        {
            get { return _likesCount; }

            set
            {
                _likesCount = value;
                NotifyPropertyChanged("LikesDisplayText");
                NotifyPropertyChanged("LikesCount");
            }
        }
        private int _likesCount
        {
            get;
            set;
        }

        public string LikesDisplayText
        {
            get
            {
                string returnValue = "No likes";

                if (LikesCount == 1) 
                {
                    returnValue = "One user likes this status";
                } 
                else if (LikesCount > 1) 
                {
                    returnValue = LikesCount.ToString() + " users like this status";
                }

                if(_isLiked) {
                    returnValue += " (including yourself)";
                }

                return returnValue;
            }
        }

        public ObservableCollection<FacebookPicture> Images { get; set; }
        public string Picture { get; set; }
        public string PictureLink { get; set; }

        public string Avatar
        {
            get
            {
                if (User != null)
                {
                    if (User.Avatar != null)
                    {
                        return User.Avatar;
                    }
                }
                return "";
            }
            set {  }
        }

        public string OriginalAvatar
        {
            get
            {
                if (User != null)
                {
                    if (User.OriginalAvatar != null)
                    {
                        return User.OriginalAvatar;
                    }
                }
                return "";
            }
        }

        public decimal Username
        {
            get
            {
                return Convert.ToDecimal(User.Id);
            }
            set { }
        }

        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;

        public string HumanReadableAgo
        {
            get
            {
                if (_humanReadableAgo != null)
                {
                    return " " + _humanReadableAgo;
                }
                else
                {
                    return "";
                }
            }
        }
        private string _humanReadableAgo { get; set; }

        public FacebookItem()
        {
            IsUpdatedExistingItem = false;

            User = new FacebookUser();
            To = new FacebookUser();
            Application = new FacebookApplication();

            Likes = new ObservableCollection<FacebookUser>();
            Comments = new ObservableCollection<FacebookComment>();
            Comments.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Comments_CollectionChanged);

            Images = new ObservableCollection<FacebookPicture>();

            MessageType = MessageTypes.StatusMessage;
            ElementsInText = new List<TextSubTypes.ISubType>();

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted);

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            return;
            /*
            if (e != null)
            {
                if (e.PropertyName != null)
                {
                    if (e.PropertyName == "SortFacebookByUpdated")
                    {
                        NotifyPropertyChanged("SortDate");
                    }
                }
            } */
        }

        void Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommentsCount = Comments.Count();
        }

        public override string ToString()
        {
            return Text;
        }

        public string TextContent
        {
            get
            {
                return Text;
            }
            set
            {
                _text = value;
            }
        }

        private void findElementsInText(string fullText)
        {
            if(string.IsNullOrEmpty(fullText)) {
                return;
            }

            List<TextSubTypes.ISubType> foundElements = new List<TextSubTypes.ISubType>();
            string[] words = Regex.Split(fullText, @"([\r\n \(\)\{\}\[\];])");
            foreach (string word in words)
            {
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
                else
                {
                    TextSubTypes.Text currentText = new TextSubTypes.Text(word);
                    foundElements.Add(currentText);
                }
                if (ElementsInText == null)
                {
                    ElementsInText = new List<TextSubTypes.ISubType>();
                }
                else
                {
                    ElementsInText.Clear();
                }
                foreach(TextSubTypes.ISubType element in foundElements) {
                    ElementsInText.Add(element);
                }
            }
        }

        public decimal accountId
        {
            get
            {
                decimal _id = 0;
                try {
                    _id = Convert.ToDecimal(User.Id);
                } catch {}
                return _id;
            }
            set
            {
                User.Id = value.ToString();
            }
        }



        public decimal Id
        {
            get;
            set;
        }
        public string fullId { get; set; }

        public AccountFacebook Account { get; set; }
        public System.Windows.Media.SolidColorBrush accountColor
        {
            get
            {
                if (Account != null)
                {
                    return new System.Windows.Media.SolidColorBrush(Account.accountColor);
                }
                else
                {
                    return System.Windows.Media.Brushes.Blue;
                }
            }
        }

        public void LikeThisItem()
        {
            if (!isLiked)
            {
                Account.LikeItem(this);
                isLiked = true;
                LikesCount++;
            }
            else
            {
                Account.UnlikeItem(this);
                isLiked = false;
                LikesCount = Math.Max(0, --LikesCount);
            }
        }

        public static FacebookItem ConvertResponseToItem(dynamic post, AccountFacebook ReceivingAccount)
        {
            try
            {
                FacebookItem item = new FacebookItem();
                item.Account = ReceivingAccount;
                item.fullId = post.id as string;
                string[] ids = item.fullId.Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (ids.Length < 2)
                {
                    return null;
                }
                try
                {
                    item.Id = Convert.ToDecimal(ids[1]);
                }
                catch
                {
                    return null;
                }

                #region Messagetypes

                if (post.type != null)
                {
                    string postType = post.type;
                    switch (postType)
                    {
                        case "photo":
                            item.MessageType = MessageTypes.Photo;
                            break;
                        case "link":
                            item.MessageType = MessageTypes.Link;
                            break;
                        case "event":
                            item.MessageType = MessageTypes.Event;
                            break;
                        case "checkin":
                            item.MessageType = MessageTypes.CheckIn;
                            break;
                        case "status":
                            item.MessageType = MessageTypes.StatusMessage;
                            break;
                        case "video":
                            item.MessageType = MessageTypes.Video;
                            break;
                        case "note":
                            item.MessageType = MessageTypes.Note;
                            break;
                    }
                }

                #endregion 

                item.To.FullName = null;

                if (post.to != null)
                {
                    if (post.to.data != null)
                    {
                        foreach (dynamic receiver in post.to.data)
                        {
                            item.To = FacebookUser.CreateFromDynamic(receiver, ReceivingAccount);
                            item.To.ReceivingAccount = ReceivingAccount;
                            // only one receiver for now - should be a collection later on of course
                            break;
                        }
                        
                    }
                }

                
                DateTime createdDate;
                DateTime.TryParse(post.created_time as string, out createdDate);
                item.CreatedAt = createdDate;

                DateTime updatedDate;
                DateTime.TryParse(post.updated_time as string, out updatedDate);
                item.UpdatedAt = updatedDate;

                
                
                //item.User.Id = post.from.id;
                //item.User.FullName = post.from.name;
                item.User = FacebookUser.CreateFromDynamic(post.from, item.Account);
                if (item.User == null)
                {
                    AppController.Current.Logger.writeToLogfile("Null user on item retrieval");
                    return null;
                }

                if (post.comments != null)
                {
                    AppController.Current.Logger.writeToLogfile("Facebook item has comments");
                    if (post.comments.data != null)
                    {
                        AppController.Current.Logger.writeToLogfile("Facebook item has comments/data");
                        foreach (dynamic fbcomment in post.comments.data)
                        {
                            AppController.Current.Logger.writeToLogfile("Reading comment");
                            FacebookComment comment = new FacebookComment(fbcomment);
                            comment.Account = item.Account;
                            if (comment.Id == ReceivingAccount.Id.ToString())
                            {
                                item.isCommented = true;
                            }
                            item.Comments.Add(comment);
                        }
                    }
                }
                
                if (post.application != null)
                {
                    item.Application.Name = post.application.name;
                    item.Application.Link = post.application.link;
                    item.Application.Id = post.application.id;
                }
                

                if (post.likes != null)
                {
                    if (post.likes.count != null)
                    {
                        item.LikesCount = Convert.ToInt32(post.likes.count);
                    }
                    if (post.likes.data != null)
                    {
                        AppController.Current.Logger.writeToLogfile("Facebook item has likes/data");
                        foreach (dynamic fbLike in post.likes.data)
                        {
                            AppController.Current.Logger.writeToLogfile("Reading like");
                            FacebookUser user = FacebookUser.CreateFromDynamic(fbLike,item.Account);
                            user.ReceivingAccount = item.Account;
                            
                            if (user != null)
                            {
                                item.Likes.Add(user);
                                if (user.FullName == item.Account.FullName)
                                {
                                    item.isLiked = true;
                                }
                            }
                        }
                    }
                }

                if (post.picture != null)
                {
                    FacebookPicture picture = new FacebookPicture();
                    picture.ThumbnailPath = post.picture;
                    picture.Link = post.link;
                    picture.Caption = post.caption;
                    item.Picture = picture.ThumbnailPath;
                    item.PictureLink = picture.Link;

                    item.Images.Add(picture);
                }

                item.Description = post.description;

                item.Message = post.message;
                item.Name = post.name;
                item.Caption = post.caption;
                item.Link = post.link;

                // ganz zum Schluss, das in der Textbox auf diese Aenderung getriggert wird
                item.Text = item.Message;

                return item;
            }
            catch (Exception exp)
            {

                AppController.Current.Logger.writeToLogfile(exp);
                return null;
            }
        }

        public string Message { get; set; }
        public bool HasMessage
        {
            get
            {
                return (!string.IsNullOrEmpty(Message));
            }
        }

        public string Name { get; set; }
        public bool HasName
        {
            get
            {
                return (!string.IsNullOrEmpty(Name));
            }
        }

        public string Link { get; set; }
        public bool hasLink
        {
            get
            {
                return (!string.IsNullOrEmpty(Link));
            }
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



        public string ClientName
        {
            get {
                if (Application != null)
                {
                    if (Application.Name != null)
                    {
                        return Application.Name;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        #region TIme background process

        public void timerHumanReadableAgoUpdate(Object state)
        {
            try
            {
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(this.CreatedAt, _humanReadableAgo));
                }
                else
                {
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

            public backgroundWorkerTimeAgoResult(string humanReadableTimeAgo, int secondsTillNextUpdate)
            {
                HumanReadableTimeAgo = humanReadableTimeAgo;
                SecondsTillNextUpdate = secondsTillNextUpdate;
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
                        nextUpdateInMilliSeconds = result.SecondsTillNextUpdate * 1000;
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
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
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

                string output = String.Empty;

                TimeSpan time = DateTime.Now - arguments.CreatedAt;

                int NextUpdateInXSeconds = 60;

                if (time.TotalSeconds <= 0)
                {
                    output += " right now";
                    // if date in the future wait until it in the current time
                    e.Result = new backgroundWorkerTimeAgoResult(output, Math.Min(2, -1 * Convert.ToInt32(time.TotalSeconds)));
                    return;
                }

                if (time.Days > 0)
                {

                    if (time.Days == 1)
                    {
                        output += " one day ago";
                    }
                    else
                    {
                        output += time.Days + " days ago";
                    }
                    NextUpdateInXSeconds = 10;
                    NextUpdateInXSeconds += (24 - time.Hours) * 60 * 60;
                    NextUpdateInXSeconds += (60 - time.Minutes) * 60;
                    NextUpdateInXSeconds += (60 - time.Seconds);

                }
                else if (time.Hours > 0)
                {

                    if (time.Hours == 1)
                    {
                        output += " one hour ago";
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
                        output += " one minute ago ";
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
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds);
                }
                else if (output != arguments.OldHumanReadableString)
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds);
                }
                else
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        #endregion





        public DateTime SortDate
        {
            get {
                if (Properties.Settings.Default.SortFacebookByUpdated)
                {
                    if (UpdatedAt != null)
                    {
                        return UpdatedAt;
                    }
                }
                return CreatedAt;
            
            }
        }

        public bool HumanReadableUpdateInProgress
        {
            get
            {
                return backgroundWorkerTimeAgo.IsBusy;
            }
        }

        ~FacebookItem()
        {
            if(backgroundWorkerTimeAgo != null) {
                backgroundWorkerTimeAgo.CancelAsync();
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
                    return "Facebook " + MessageType.ToString() + " " + User.FullName + ": " + Text;
                }
                catch (Exception exp)
                {
                    return "Facebook error item: " + exp.Message;
                }
            }
        }



        public IAccount ReceivingAccount
        {
            get;
            set;
        }
    }

}
