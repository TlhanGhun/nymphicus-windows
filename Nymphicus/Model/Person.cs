using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;


namespace Nymphicus.Model
{
    public class Person
    {
        public decimal Id { get; set; }
        public string Username { get; set; }
        public string Realname { get; set; }
        public string Name
        {
            get
            {
                return Username;
            }
        }

        public bool ShowCheckboxForThisUserInList { get; set; }

        public string NameAndLogin
        {
            get
            {
                if (Username != "" && Realname != "")
                {
                    return string.Format("{0} (@{1})", Realname, Username);
                }
                else if (Username != "")
                {
                    return "@" + Username;
                }
                else if(Realname != "") {
                    return Realname;
                }
                else if (Id != 0)
                {
                    return "Twitter-ID: " +Id.ToString();
                }
                else
                {
                    return "Unknown";
                }
            }
        }
        private string _avatar {get;set;}
        public string Avatar
        {
            get
            {
                if (_avatar != null)
                {
                    return _avatar;
                }
                return "";
            }
            set
            {
                string targetPath = Path.Combine(AppController.Current.appDataPath,"IconCache",this.Id.ToString() + ".cacheImage");
                if(File.Exists(targetPath)) {
                    _avatar = targetPath;
                }
                else 
                {
                if (value.StartsWith("http"))
                {
                    try
                    {
                        System.Drawing.Image avatarImage = API.Functions.DownloadBinaryFromInternet(value);
                        if (avatarImage != null)
                        {
                            avatarImage.Save(targetPath);
                            _avatar = targetPath;
                        }
                        else
                        {
                            _avatar = value;
                        }

                    }
                    catch (Exception exp)
                    {
                        AppController.Current.Logger.writeToLogfile(exp);
                        _avatar = value;
                    }

                }
                else
                {
                    _avatar = value;
                }
                }
            }
        }

        void bi_DownloadCompleted(object sender, EventArgs e)
        {
            BitmapImage bi = sender as BitmapImage;
            if(bi != null) {
                bi.Freeze();
                string imageFilePath = Path.GetTempFileName();
                System.IO.FileStream fileStream = new FileStream(imageFilePath, FileMode.Create);
                System.Windows.Media.Imaging.BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bi));
                encoder.Save(fileStream);
                fileStream.Close();
                _avatar = imageFilePath;
                imageCachePath = imageFilePath;
            }
        }

        private string imageCachePath { get; set; }
        
        public string ProfileBackgroundColorString { get; set; }

        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public bool? DoesReceiveNotifications { get; set; }
        public bool? FollowRequestSend { get; set; }
        public bool? IsContributorsEnabled { get; set; }
        public bool? IsFollowedBy { get; set; }
        public bool? IsFollowing { get; set; }
        public bool? IsGeoEnabled { get; set; }
        public bool? IsProfileBackgroundTiled { get; set; }
        public bool? IsProtected { get; set; }
        public string Language { get; set; }
        public int ListedCount { get; set; }
        public string Location { get; set; }
        public long NumberOfFavorites { get; set; }
        public long? NumberOfFollowers { get; set; }
        public long NumberOfFriends { get; set; }
        public long NumberOfItems { get; set; }
        public string Timezone { get; set; }
        public double? TimezoneOffset { get; set; }
        public bool? Verified { get; set; }
        public string Website { get; set; }
        public AccountTwitter ReceivingAccount { get; set; }

        public BitmapImage AvatarImage
        {
            get;
            private set;
        }

        private BackgroundWorker backgroundWorkerRecentTweets;

        public void UpdateRecentTweets() {
            RecentTweets.Clear();
            if (LastTweet != null)
            {
                RecentTweets.Add(LastTweet);
            }
            if (!backgroundWorkerRecentTweets.IsBusy)
            {
                backgroundWorkerRecentTweets.RunWorkerAsync();
            }
            else
            {
                backgroundWorkerRecentTweets.CancelAsync();
            }
        }

        public ObservableCollection<TwitterItem> RecentTweets {get; private set; }
        public TwitterItem LastTweet { get; set; }

        public bool isSearchUser { get; set; }

        public Person(AccountTwitter receivingAccount)
        {
            ProfileBackgroundColorString = "";
            ReceivingAccount = receivingAccount;
            RecentTweets = new ObservableCollection<TwitterItem>();
            backgroundWorkerRecentTweets = new BackgroundWorker();
            backgroundWorkerRecentTweets.WorkerReportsProgress = true;
            backgroundWorkerRecentTweets.WorkerSupportsCancellation = true;
            backgroundWorkerRecentTweets.DoWork += new DoWorkEventHandler(backgroundWorkerRecentTweets_DoWork);
            backgroundWorkerRecentTweets.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerRecentTweets_ProgressChanged);


            this.isSearchUser = false;
        }

        void backgroundWorkerRecentTweets_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;
            if (item != null)
            {
                if (RecentTweets.Where(i => i.Id == item.Id).Count() == 0)
                {
                    RecentTweets.Add(item);
                }
            }
        }

        private void backgroundWorkerRecentTweets_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this == null)
            {
                AppController.Current.Logger.writeToLogfile("Null this at rectent tweets");
                return;
            }
            if (this.Username == null)
            {
                AppController.Current.Logger.writeToLogfile("Null username at rectent tweets");
                return;
            }
            ObservableCollection<TwitterItem> items = new ObservableCollection<TwitterItem>();
            try
            {
                TwitterService searchService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.ReceivingAccount.Token, this.ReceivingAccount.TokenSecret);
                searchService.UserAgent = "Nymphicus for Windows";

                foreach (TwitterItem item in API.Functions.getItemsForUsername(searchService, this.Username, this.ReceivingAccount))
                {
                    if (e != null)
                    {
                        if (e.Cancel)
                        {
                            return;
                        }
                    }
                    backgroundWorkerRecentTweets.ReportProgress(100, item);
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Retrieving recent tweets failed for " + this.Username);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        ~Person() {
            if (!string.IsNullOrEmpty(imageCachePath))
            {
                if (File.Exists(imageCachePath))
                {
                    try
                    {
                        File.Delete(imageCachePath);
                    }
                    catch
                    {
                        
                    }
                }
            }
            if (backgroundWorkerRecentTweets != null)
            {
                backgroundWorkerRecentTweets.CancelAsync();
            }
        }

        public void createFromTweetSharp(TwitterUser user)
        {
            this.Id = user.Id;
            this.Realname = user.Name;
            this.Username = user.ScreenName;
            this.Avatar = user.ProfileImageUrl;
            if (user.ProfileBackgroundColor  != "")
            {
                this.ProfileBackgroundColorString = user.ProfileBackgroundColor;
            }

            this.CreateDate = user.CreatedDate;
            this.Description = user.Description;
            
            this.FollowRequestSend = user.FollowRequestSent;
            this.IsContributorsEnabled = user.ContributorsEnabled;
            //this.IsFollowedBy = user.;
            //this.IsFollowing = user.IsFollowing;
            this.IsGeoEnabled = user.IsGeoEnabled;
            this.IsProfileBackgroundTiled = user.IsProfileBackgroundTiled;
            this.IsProtected = user.IsProtected;
            this.Language = user.Language;
            this.ListedCount = user.ListedCount;
            this.Location = user.Location;
            this.NumberOfFavorites = user.FavouritesCount;
            this.NumberOfFollowers = user.FollowersCount;
            this.NumberOfFriends = user.FriendsCount;
            this.NumberOfItems = user.StatusesCount;
            this.Timezone = user.TimeZone;
            //this.TimezoneOffset = user.TimeZoneOffset;
            this.Verified = user.IsVerified;
            this.Website = user.Url;
        }

        public override string ToString()
        {
            return "@" + this.Username;
        }

    }

    
}
