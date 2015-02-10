using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace Nymphicus.Model
{
    public class FacebookComment : INotifyPropertyChanged
    {
        public FacebookUser User { get; set; }
        public string Text
        {
            get
            {
                if (_text == null)
                {
                    return "";
                }
                return _text;
            }
            set
            {
                _text = value;
            }
        }
        private string _text { get; set; }
        public AccountFacebook Account { get; set; }
        public string Id { get; set; }

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

            }
        }
        private DateTime _createdAt { get; set; }
        public int LikesCount
        {
            get { return _likesCount; }

            set
            {
                _likesCount = value;
                NotifyPropertyChanged("LikesCount");
                NotifyPropertyChanged("LikesDisplayText");
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
                    returnValue = "One user likes this comment";
                }
                else if (LikesCount > 1)
                {
                    returnValue = LikesCount.ToString() + " users like this status";
                }

                if (_isLiked)
                {
                    returnValue += " (including yourself)";
                }

                return returnValue;
            }
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
        public string Avatar
        {
            get
            {
                if (User != null)
                {
                    return User.Avatar;
                }
                return "";
            }
        }


        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;

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

        public FacebookComment()
        {
            _init();
        }

        private void _init() {
            User = new FacebookUser();

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted);
        }

        public FacebookComment(dynamic data)
        {
            _init();
            CreateFromDynamic(data);
        }

        public void CreateFromDynamic(dynamic data)
        {
            if (data != null)
            {
                this.Text = data.message;
                this.Id = data.id;
                DateTime createdDate;
                DateTime.TryParse(data.created_time as string, out createdDate);
                this.CreatedAt = createdDate;
                this.User = FacebookUser.CreateFromDynamic(data.from, this.Account);
                if (data.likes != null)
                {
                    try
                    {
                        this.LikesCount = Convert.ToInt32(data.likes);
                    }
                    catch (Exception exp) {
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
                if (data.user_likes != null)
                {
                    this.isLiked = true;
                }
            }
        }


        public void LikeThisComment()
        {
            if (!isLiked)
            {
                Account.LikeComment(this);
                LikesCount++;
                isLiked = true;
            }
            else
            {
                Account.UnlikeComment(this);
                LikesCount--;
                isLiked = false;
            }
        }

        #region TIme background process

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
                        timerHumanReadableAgo.Dispose();
                        timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, Math.Max(1000, nextUpdateInMilliSeconds), 0);
                    }
                    catch
                    {
                        _humanReadableAgo = null;
                    }

                }
                else
                {
                    timerHumanReadableAgo.Dispose();
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

        public event PropertyChangedEventHandler PropertyChanged;

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

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

        ~FacebookComment()
        {
            User = null;
            if (backgroundWorkerTimeAgo != null)
            {
                backgroundWorkerTimeAgo.CancelAsync();
            }
            if (timerHumanReadableAgo != null)
            {
                timerHumanReadableAgo.Dispose();
            }
        }

    }
}
