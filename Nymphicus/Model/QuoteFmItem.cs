using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using QuoteSharp;

namespace Nymphicus.Model
{
    public class QuoteFmItem : IItem, INotifyPropertyChanged
    {
        public string CategoryTitle { get; set; }
        public string CategoryTitleDisplayText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CategoryTitle))
                {
                    return "subscribed category: " + CategoryTitle;
                }
                else
                {
                    return "";
                }
            }
        }

        public QuoteFmItem()
        {
            Source = new QuoteFmSource();

            Comments = new ThreadSaveObservableCollection<QuoteFmComment>();

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted); 
        }

        public static QuoteFmItem createFromApi(Recommendation recommendation)
        {
            if (recommendation == null)
            {
                return null;
            }
            QuoteFmItem item = new QuoteFmItem();
            // Created At is missing!
            if (recommendation.article != null)
            {
                item.CreatedAt = recommendation.article.created.Value;
            }
            else
            {
                item.CreatedAt = DateTime.Now;
            }

            item.AuthorName = recommendation.user.username;
            item.Id = recommendation.id;
            item.Text = recommendation.quote;
            item.QuoteType = QuoteTypes.Recommendation;
            item.ClientName = recommendation.domain.name;
            item.ArticleLink = recommendation.article.url;
            item.Author = QuoteFmUser.createFromApi(recommendation.user);

            return item;
        }

        public static QuoteFmItem createFromApi(Article article)
        {
            if (article == null)
            {
                return null;
            }
            if(article.topquote == null) {
                return null;
            }
            QuoteFmItem item = new QuoteFmItem();
            
            item.CreatedAt = article.created.Value;

            item.Id = article.id;
            item.AuthorName = article.topquote.user.fullname;
            item.Text = article.topquote.quote;
            item.QuoteType = QuoteTypes.Recommendation;
            item.ClientName = article.topquote.domain.name;
            item.ArticleLink = article.url;
            item.Author = QuoteFmUser.createFromApi(article.topquote.user);

            return item;
        }

        public enum QuoteTypes
        {
            Recommendation,
            Like,
            Mention,
            UsersAndPages
        }

        public bool IsArticleOfCategory { get; set; }

        public string TextContent
        {
            get
            {
                return Text;
            }
            set
            {
                
            }
        }
        public string ArticleLink { get; set; }

        public AccountQuoteFM ReceivingAccount { get; set; }
        public QuoteFmUser Author { get; set; }
        public QuoteTypes QuoteType { get; set; }
        // QuoteTypes as bool for more easy access in user interface :)
        public bool IsRecommendation
        {
            get
            {
                return QuoteType == QuoteTypes.Recommendation;
            }
        }
        public bool IsLike
        {
            get
            {
                return QuoteType == QuoteTypes.Like;
            }
        }
        public bool IsMention
        {
            get
            {
                return QuoteType == QuoteTypes.Mention;
            }
        }
        public bool IsUserAndPages
        {
            get
            {
                return QuoteType == QuoteTypes.UsersAndPages;
            }
        }

        public QuoteFmSource Source { get; set; }

        public ThreadSaveObservableCollection<QuoteFmComment> Comments { get; set; }

        public decimal accountId
        {
            get
            {
                if (ReceivingAccount != null)
                {
                    return ReceivingAccount.Id;
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
        public string AuthorName
        {
            get
            {
                return Author.Fullname;
            }
            set { }
        }
        public string Text
        {
            get;
            set;
        }
        public string QuotedText
        {
            get
            {
                return string.Format("»{0}«", Text);
            }
        }
        public decimal Id
        {
            get;
            set;
        }

        public string ClientName
        {
            get;
            set;
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

        #region Human readable ago

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
        public bool HumanReadableUpdateInProgress
        {
            get { return false; }
        }


        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;
        public void timerHumanReadableAgoUpdate(Object state)
        {
            try
            {
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

        public bool ShowTweetButton
        {
            get
            {
                return AppController.Current.HasTwitterAccounts;
            }
        }

        public bool ShowFacebookButton
        {
            get
            {
                return AppController.Current.HasFacebookAccounts;
            }
        }

        #region INotifyPropertyChanged
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

        #endregion

        #region Colors

        public System.Windows.Media.SolidColorBrush accountColor
        {
            get
            {
                if (ReceivingAccount != null)
                {
                    return new System.Windows.Media.SolidColorBrush(ReceivingAccount.accountColor);
                }
                else
                {
                    return System.Windows.Media.Brushes.LightBlue;
                }
            }
        }

        public System.Windows.Media.SolidColorBrush TextBackgroundColor
        {
            get
            {
                switch (QuoteType) {
                    case QuoteTypes.Recommendation:
                        return System.Windows.Media.Brushes.Blue;
                        

                        case QuoteTypes.Like:
                        return System.Windows.Media.Brushes.Green;
                        

                        case QuoteTypes.Mention:
                        return System.Windows.Media.Brushes.DarkGray;
                        

                        case QuoteTypes.UsersAndPages:
                        return System.Windows.Media.Brushes.Red;
                        

                    default:
                        return System.Windows.Media.Brushes.Black;
                }
                 
            }    
        }

        public string DebugText
        {
            get
            {
                try
                {
                    return "QUOTE.fm " + AuthorName + ": " + Text;
                }
                catch (Exception exp)
                {
                    return "QUOTE.fm error item: " + exp.Message;
                }
            }
        }

        #endregion


        IAccount IItem.ReceivingAccount
        {
            get;
            set;
        }
    }
}
