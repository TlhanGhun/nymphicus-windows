using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Threading;
using System.Text.RegularExpressions;
using AppNetDotNet;
using AppNetDotNet.ApiCalls;
using AppNetDotNet.Model;
using System.Collections.ObjectModel;

namespace Nymphicus.Model
{
    public class ApnItem : INotifyPropertyChanged, IItem
    {

        public Post apnItem;
        public Message apnMessage;

        public Entities entities { get; set; }
        public bool hasEmbeddedImages { get; set; }
        public ObservableCollection<AppNetDotNet.Model.Annotations.EmbeddedMedia> imagesInPost
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
        private ObservableCollection<AppNetDotNet.Model.Annotations.EmbeddedMedia> _imagesInPost { get; set; }

        public bool isRepost { get; set; }
        public string channelId
        {
            get
            {
                if (apnMessage != null)
                {
                    return apnMessage.channel_id;
                }
                else
                {
                    return null;
                }
            }
        }
        public bool isStreamMarker
        {
            get
            { return _isStreamMarker; }
            set
            {
                _isStreamMarker = value;
                NotifyPropertyChanged("isStreamMarker");
            }
        }
        private bool _isStreamMarker { get; set; }
        public bool isMessage { get; set; }
        public bool isPrivateMessage { get; set; }

        public ApnItem()
        {
            apnItem = new Post();
            if (apnItem.repost_of != null)
            {
                isRepost = true;

            }
            ApnItemInit();
        }
        public ApnItem(Post post, AccountAppDotNet account)
        {
            this.receivingAccount = account;
            if (post == null)
            {
                return;
            }
            apnItem = post;
            this.entities = post.entities;
            if (apnItem.repost_of != null)
            {
                isRepost = true;
                this.entities = apnItem.repost_of.entities;
                this.entities = post.repost_of.entities;
                scanForImages(post.repost_of.annotations);
            }
            else
            {
                scanForImages(post.annotations);
            }
            
            ApnItemInit();
        }

        public ApnItem(Message message, AccountAppDotNet account)
        {
            this.receivingAccount = account;
            apnMessage = message;
            this.Text = message.text;
            this.entities = message.entities;
            isMessage = true;
            isPrivateMessage = true;

            scanForImages(message.annotations);
            ApnItemInit();
        }

        private void ApnItemInit()
        {
            ElementsInText = new List<TextSubTypes.ISubType>();

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted);

            if (timerHumanReadableAgo != null)
            {
                timerHumanReadableAgo.Dispose();
            }
            if (!backgroundWorkerTimeAgo.IsBusy)
            {
                backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(CreatedAt, null));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

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

        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;

        public string displayText
        {
            get
            {
                if (!isReposted || this.isPrivateMessage)
                {
                    return Text;
                }
                else
                {
                    return apnItem.repost_of.text;
                }
            }
        }

        public bool isReposted
        {
            get
            {
                if (apnItem != null)
                {
                    return (apnItem.repost_of != null);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool isMention
        {
            get;set;
        }

        public bool isPartOfConversation
        {
            get
            {
                if (apnItem != null)
                {
                    return (apnItem.reply_to != null);
                }
                else if (apnMessage != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string repostAvatar
        {
            get
            {
                if (apnItem == null)
                {
                    return "";
                }
                if (apnItem.repost_of == null)
                {
                    return "";
                }
                else
                {
                    return apnItem.user.avatar_image.url;
                }
            }
        }
        

        public string Avatar { 
             get
            {
                if (apnMessage != null)
                {
                    return apnMessage.user.avatar_image.url;
                }
                else if (apnItem == null)
                {
                    return "";
                }
                else if (apnItem.repost_of == null)
                {
                    return apnItem.user.avatar_image.url;
                }
                else
                {
                    return apnItem.repost_of.user.avatar_image.url;
                }
            }
        
            set { }
        }

        public string TextContent
        {
            get
            {
                return displayText;
            }
            set
            {
                return;
            }
        }

        public decimal accountId
        {
            get
            {
                return receivingAccount.Id;
            }
            set
            {
               
            }
        }

        public SolidColorBrush accountColor
        {
            get
            {
                if (this.receivingAccount != null)
                {
                    return new SolidColorBrush(receivingAccount.accountColor);

                }
                else
                {
                    return Brushes.Red;
                }
            }
        }

        public AccountAppDotNet receivingAccount { get; set; }

        public string AuthorName
        {
            get
            {
                if (apnMessage != null)
                {
                    return apnMessage.user.name + " (@" + apnMessage.user.username + ")";
                }
                else if (apnItem != null)
                {
                    return apnItem.user.name + " (@" + apnItem.user.username + ")";
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (apnItem != null)
                {
                    apnItem.user.username = value;
                }
            }
        }
        public string repostAuthorName
        {
            get
            {
                if (apnItem != null && isReposted)
                {
                    return apnItem.repost_of.user.name + " (@" + apnItem.repost_of.user.username + ")";
                }
                else
                {
                    return "";
                }
            }
            set
            {
                
            }
        }


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
                    return CreatedAt.ToLongTimeString();
                }
            }
        }
        private string _humanReadableAgo { get; set; }


        public string Text
        {
            get
            {
                if(apnMessage != null) 
                {
                    return apnMessage.text;
                }
                else if (apnItem != null)
                {
                    if (apnItem.repost_of != null)
                    {
                        return apnItem.repost_of.text;
                    }
                    else
                    {
                        return apnItem.text;
                    }
                }
                else
                {
                    return "";
                }
            }
            set
            {
                
            }
        }

        public decimal Id
        {
            get
            {
                if (apnMessage != null)
                {
                    int itemId = 0;
                    int.TryParse(apnMessage.id, out itemId);
                    return itemId;
                }
                else if (apnItem != null)
                {
                    int itemId = 0;
                    int.TryParse(apnItem.id, out itemId);
                    return itemId;
                }
                else
                {
                    return 0;
                }

            }
            set
            {
               
            }
        }

        public string ClientName
        {
            get {
                if (apnItem != null)
                {
                    if (apnItem.source != null)
                    {
                        return apnItem.source.name;
                    }
                }
                else if (apnMessage != null)
                {
                    if (apnMessage.source != null)
                    {
                        return apnMessage.source.name;
                    }
                }
                return "Web";
                
            }
        }

        public string clientUrl
        {
            get
            {
                if (apnItem != null)
                {
                    if (apnItem.source != null)
                    {
                        return apnItem.source.link;
                    }
                }
                else if (apnMessage != null)
                {
                    if (apnMessage.source != null)
                    {
                        return apnMessage.source.link;
                    }
                }
                return "";
            }
        }
        

        public DateTime CreatedAt
        {
            get
            {
                if (apnItem != null)
                {
                    return apnItem.created_at;
                }
                else if (apnMessage != null)
                {
                    return apnMessage.created_at;
                }
                else
                {
                    return DateTime.Now;
                }
            }
            set
            {

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
                if (apnItem != null)
                {
                    apnItem.created_at = value;
                }
            }
        }

        public DateTime SortDate
        {
            get { return CreatedAt; }
        }

        public bool HumanReadableUpdateInProgress
        {
            get { return true; }
        }

        public string DebugText
        {
            get {
                return displayText;
            }
        }

        public List<TextSubTypes.ISubType> ElementsInText { get; set; }

        ~ApnItem()
        {
            apnItem = null;
            apnMessage = null;

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

        #region HumanReadableAgo

        public void timerHumanReadableAgoUpdate(Object state)
        {
            try
            {
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(apnItem.created_at, _humanReadableAgo));
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


        public IAccount ReceivingAccount
        {
            get;
            set;
        }

        private void scanForImages(List<Annotation> annotations)
        {
            imagesInPost = new ObservableCollection<AppNetDotNet.Model.Annotations.EmbeddedMedia>();
            if (annotations != null)
            {
                foreach (Annotation annotation in annotations)
                {
                    if (annotation.type == "net.app.core.oembed")
                    {
                        AppNetDotNet.Model.Annotations.EmbeddedMedia media = annotation.parsedObject as AppNetDotNet.Model.Annotations.EmbeddedMedia;
                        if (media != null)
                        {
                            if (!string.IsNullOrEmpty(media.thumbnail_url) || !string.IsNullOrEmpty(media.url))
                            {
                                if (string.IsNullOrEmpty(media.thumbnail_url))
                                {
                                    media.thumbnail_url = media.url;
                                }
                                imagesInPost.Add(media);
                                hasEmbeddedImages = true;
                            }
                        }
                    }
                }
            }
        }

    }
}
