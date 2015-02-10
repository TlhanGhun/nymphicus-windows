using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media;
using QuoteSharp;

namespace Nymphicus.Model
{
    public class AccountQuoteFM : IAccount, INotifyPropertyChanged
    {
        public ThreadSaveObservableCollection<QuoteFmItem> Recommendations;
        public ThreadSaveObservableCollection<QuoteFmItem> Likes;
        public ThreadSaveObservableCollection<QuoteFmItem> Mentions;
        public ThreadSaveObservableCollection<QuoteFmItem> UsersAndPages;

        private BackgroundWorker backgroundWorkerRecommendations;
        public bool RecommendationsIsBusy { get; set; }

        public bool InitialUpdateDoneForRecommendations { get; set; }

        public AccountQuoteFM()
        {
            Recommendations = new ThreadSaveObservableCollection<QuoteFmItem>();
            Likes = new ThreadSaveObservableCollection<QuoteFmItem>();
            Mentions = new ThreadSaveObservableCollection<QuoteFmItem>();
            UsersAndPages = new ThreadSaveObservableCollection<QuoteFmItem>();

            backgroundWorkerRecommendations = new BackgroundWorker();
            backgroundWorkerRecommendations.WorkerReportsProgress = true;
            backgroundWorkerRecommendations.WorkerSupportsCancellation = true;
            backgroundWorkerRecommendations.DoWork += new DoWorkEventHandler(backgroundWorkerRecommendations_DoWork);
            backgroundWorkerRecommendations.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerRecommendations_RunWorkerCompleted);
            backgroundWorkerRecommendations.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerRecommendations_ProgressChanged);
        }

        public decimal Id
        {
            get
            {
                if (this.User != null)
                {
                    return User.Id;
                }
                else
                {
                    return 0;
                }
            }
            set { }
        }
        public string username
        {
            get
            {
                if (this.User != null)
                {
                    return this.User.username;
                }
                else
                {
                    return _username;
                }
            }
            set
            {
                _username = value;
            }
        }
        private string _username { get; set; }

        public bool LoginSuccessfull
        {
            get;
            set;
        }
        public string Avatar
        {
            get
            {
                return this.User.Avatar;
            }
            set { }
        }
        public QuoteFmUser User { get; set; }
        public List<string> AvailableNotificationClasses
        {
            get {
                List<string> classes = new List<string>();
                classes.Add("QUOTE.fm " + User.username + " Recommendations");
                return classes;
            }
        }
        public string AccountType
        {
            get { return "QUOTE.fm"; }
        }
        public void registerAccount()
        {
            AppController.Current.Logger.writeToLogfile("Reading stored color of Quote.FM account");
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
                AppController.Current.Logger.writeToLogfile("Loading of color failed - using Quote.FM default light blue");
                AppController.Current.Logger.writeToLogfile(exp);
                if (AppController.Current.accountColors.ContainsKey(this.Id))
                {
                    AppController.Current.accountColors[this.Id] = new SolidColorBrush(Colors.LightBlue);
                }
                else
                {
                    AppController.Current.accountColors.Add(this.Id, new SolidColorBrush(Colors.LightBlue));
                }
            }
        }
        public void UpdateItems()
        {
            UpdateRecommendations();
        }

        public void UpdateRecommendations()
        {
            if (!backgroundWorkerRecommendations.IsBusy)
            {
                RecommendationsIsBusy = false;
                backgroundWorkerRecommendations.RunWorkerAsync();
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Recommendations background thread of @" + username + " is busy");
                if (RecommendationsIsBusy)
                {
                    AppController.Current.Logger.writeToLogfile("Recommendations background thread of @" + username + " is busy and has been before - trying to cancel");
                    backgroundWorkerRecommendations.CancelAsync();
                }
                RecommendationsIsBusy = true;
            }
        }

        #region Background Workers
        #region DoWorks

        void backgroundWorkerRecommendations_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
            IEnumerable<QuoteFmItem> newOwnRecos =  User.updateRecommendations();
            foreach (QuoteFmItem recommendation in newOwnRecos)
            {
                if (recommendation != null)
                {
                    recommendation.ReceivingAccount = this;
                    backgroundWorkerRecommendations.ReportProgress(50, recommendation);
                }
            }
            foreach (QuoteFmUser following in User.Followings)
            {
                IEnumerable<QuoteFmItem> newFollowedRecos = following.updateRecommendations();
                foreach (QuoteFmItem recommendation in newFollowedRecos)
                {
                    if (recommendation != null)
                    {
                        recommendation.ReceivingAccount = this;
                        backgroundWorkerRecommendations.ReportProgress(100, recommendation);
                    }
                }
            }
        }

        #endregion

        #region Report progress / Progress changed


        void backgroundWorkerRecommendations_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            QuoteFmItem item = e.UserState as QuoteFmItem;
            if(item != null) {
            Recommendations.Add(item);
                if (InitialUpdateDoneForRecommendations)
                {
                    AppController.Current.sendNotification("QUOTE.fm " + User.username + " Recommendations", item.Author.Fullname, item.QuotedText, item.Author.Avatar, item);
                }
            }
        }

        #endregion

        #region Worker completed

        void backgroundWorkerRecommendations_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialUpdateDoneForRecommendations)
            {
                InitialUpdateDoneForRecommendations = true;
            }
        }

        #endregion 
        #endregion
        public string getStorableSettings()
        {
            string delimiter = "|||";
            string storableString = "QuoteFM";
            storableString += delimiter + Crypto.EncryptString(Crypto.ToSecureString(username));
            storableString += delimiter + Crypto.EncryptString(Crypto.ToSecureString("PLACEHOLDER FOR TOKEN"));
            storableString += delimiter + getColorString(accountColor);
            return storableString;
        }
        public void readStorableSettings(string storedSettingsString)
        {
            AppController.Current.Logger.writeToLogfile("Parsing stored QuoteFM settings");
            try
            {
                string[] delimiter = { "|||" };
                string[] storedData = storedSettingsString.Split(delimiter, StringSplitOptions.None);

                if (storedData.Length < 4)
                {
                    return;
                }

                this.username = Crypto.ToInsecureString(Crypto.DecryptString(storedData[1]));
                //this._password = Crypto.ToInsecureString(Crypto.DecryptString(storedData[2]));

                accountColor = getColorFromString(storedData[3]);

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

            User user = QuoteSharp.API.getUser(username);
            if (user != null)
            {
                this.User = QuoteFmUser.createFromApi(user, getFollowings: true);
                LoginSuccessfull = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        public System.Collections.ObjectModel.ObservableCollection<SubscribableItemsCollection> subscribableItemCollections
        {
            get;
            set;
        }

        #region background color

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
                    return System.Windows.Media.Brushes.Cyan;
                }
            }
        }
    }
}
