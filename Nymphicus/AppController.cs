using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TweetSharp;
using Nymphicus.Model;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using Nymphicus.ExternalLibraries.Notifications.Snarl;
using System.Configuration;

namespace Nymphicus
{
    class AppController : INotifyPropertyChanged
    {
        public bool useAppNetTestAccount = false;
        public bool useDebugMessagesOverride = false;
        public bool isBetaVersion = false;
        public AccountAppDotNet apnAccount;

        public static AppController Current;
        public UserInterface.Mainwindow mainWindow;
        public Logging Logger;
        UserInterface.Startup startupWindow;
        UpdateStartupProgressBarDelegate updatePbDelegate;
        public bool OverrideScrollNow { get; set; }

        private delegate void UpdateStartupProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateStartupLabelDelegate(string text);
        private BackgroundWorker backgroundWorkerLoadAccounts;
        private BackgroundWorker backgroundWorkerCheckUpdate;

        public ThreadSaveObservableCollection<IAccount> AllAccounts { get; set; }
        public ThreadSaveObservableCollection<AccountTwitter> AllTwitterAccounts { get; set; }
        public ThreadSaveObservableCollection<AccountFacebook> AllFacebookAccounts { get; set; }
        public ThreadSaveObservableCollection<AccountQuoteFM> AllQuoteFmAccounts { get; set; }
        public ThreadSaveObservableCollection<AccountAppDotNet> AllApnAccounts { get; set; }
        public ThreadSaveObservableCollection<View> AllViews { get; set; }
        public ThreadSaveObservableCollection<Filter> AllFilters { get; set; }

        public ThreadSaveObservableCollection<TweetList> AllLists { get; set; }
        public ThreadSaveObservableCollection<Search> AllSearches { get; set; }

        public ThreadSaveObservableCollection<Person> AllPersons { get; set; }
        public ThreadSaveObservableCollection<string> AllKnownUsernames { get; set; }
        public ThreadSaveObservableCollection<string> AllKnownHashtags { get; set; }

        public ThreadSaveObservableCollection<ViewEntry> NewViewEntries { get; set; }
        public ThreadSaveObservableCollection<decimal> ReadTweets { get; set; }

        public ObservableCollection<API.ImageServices.IImageService> AllImageServices { get; set; }
        public API.ImageServices.IImageService ActualImageService;
        public Dictionary<string, string> AllImagesInItems { get; set; }

        public ObservableCollection<API.UrlShortener.ILinkShortener> AlllinkShortenerServices { get; set; }
        public API.UrlShortener.ILinkShortener ActualLinkShortener;
        public Dictionary<string, string> AllShortenedLinksInItems { get; set; }

        public ObservableCollection<ExternalServices.IStore> AllIStores { get; set; }

        public Dictionary<string,string> GoogleReaderFeedIcons { get; set; }

        public ObservableCollection<IAccount> NotWorkingAccounts { get; set; }
        public List<string> NotWorkingViews;

        public string appDataPath { get; set; }
        public string appProgramPath { get; set; }
        public static bool EnableDebugMessages { get; set; }

        public string UserAgent { get; private set; }

        public Themes.Theme CurrentTheme { get; set; }

        public List<char> TrimCharacters { get; set; }

        public bool HasFacebookAccounts
        {
            get
            {
                return AllFacebookAccounts.Count > 0;
            }
        }
        public bool HasTwitterAccounts
        {
            get
            {
                return AllTwitterAccounts.Count > 0;
            }
        }

        public bool HasQuoteFmAccounts
        {
            get
            {
                return AllQuoteFmAccounts.Count > 0;
            }
        }
         public bool HasApnAccounts
         {
             get
             {
                 return AllApnAccounts.Count > 0;
             }
         }



        public SnarlInterface snarl;
        enum SnarlActions
        {
            Reply = 1,
            DirectMessage,
            Favorite,
            Retweet,
            fbComment,
            fbLike,
            grMarkAsRead,
            openUrl
        }
        private Dictionary<Int32, IItem> archiveOfSentNotifications;
        private Dictionary<Int32, string> archiveOfLinksInActionMenus;
        public bool _snarlIsRunning { get; set; }

        public bool SnarlIsRunning
        {
            get { return _snarlIsRunning; }
            set { 
                _snarlIsRunning = value;
                NotifyPropertyChanged("SnarlIsRunning");
            }
        }
        

        public Dictionary<decimal, System.Windows.Media.SolidColorBrush> accountColors { get; set; }

        public string pathToIcon { get; private set; }

        UserInterface.ComposeNewTweet newTweetWindow;

        private System.Net.IWebProxy DefaultSystemProxy = System.Net.HttpWebRequest.DefaultWebProxy;

        #region Startup and initialization

        public static void Start()
        {
         
            if (Current == null)
            {
                Current = new AppController();
            }
        }

        private AppController()
        {
            if (useAppNetTestAccount)
            {
                apnAccount = new AccountAppDotNet();
            }



            startupWindow = new UserInterface.Startup();
            startupWindow.progressBarStartup.Maximum = 10;
            startupWindow.Show();



            Properties.Settings.Default.IsValidLicense = false;
            // TweetMarker aus
            Properties.Settings.Default.UseTweetmarker = false;
            Properties.Settings.Default.DontScrollTopItemOutOfWindow = false;

            updatePbDelegate = new UpdateStartupProgressBarDelegate(startupWindow.progressBarStartup.SetValue);
            //UpdateStartupLabelDelegate updateStartupLabelDelegate = new UpdateStartupLabelDelegate("Initialzing");

            if (Current == null)
            {
                Current = this;
            }
            OverrideScrollNow = true;
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\liGhun\\Nymphicus\\";
            appProgramPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

            Logger = new Logging(Path.Combine(appDataPath, "log.txt"));

            if (!System.IO.Directory.Exists(appDataPath))
            {
                System.IO.Directory.CreateDirectory(appDataPath);
            }
            if (!System.IO.Directory.Exists(Path.Combine(appDataPath, "IconCache")))
            {
                System.IO.Directory.CreateDirectory(Path.Combine(appDataPath, "IconCache"));
            }
            else
            {
                Logger.writeToLogfile("Starting to clean old icon cache if available");
                string[] oldIconCaches = System.IO.Directory.GetFiles(Path.Combine(appDataPath, "IconCache"), "*.cacheImage");
                foreach (string oldIcon in oldIconCaches)
                {
                    try
                    {
                        System.IO.File.Delete(oldIcon);
                    }
                    catch (Exception exp) {
                        Logger.writeToLogfile(exp);
                    }
                }
            }

           if(useDebugMessagesOverride || File.Exists(Path.Combine(appDataPath,"DebugMessages.enabled"))) {
                AppController.EnableDebugMessages = true;
                UserInterface.ShowDebugMessages debugMessages = new UserInterface.ShowDebugMessages();
                debugMessages.Show();
           }


            try
            {
                string settings = System.IO.File.ReadAllText(AppController.GetDefaultExeConfigPath(ConfigurationUserLevel.PerUserRoamingAndLocal));
                Logger.addDebugMessage("All stored settings ", Convert.ToBase64String(Encoding.UTF8.GetBytes(settings)), type: DebugMessage.DebugMessageTypes.General);
            }
            catch (Exception exp)
            {
                Logger.addDebugMessage("Could not read XML config file", exp.Message, type: DebugMessage.DebugMessageTypes.General);
            }
     
            string alreadyMigratedSettingsTriggerFile = appDataPath + "\\PreferencesMigrated-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ".upd";
            if (!System.IO.File.Exists(alreadyMigratedSettingsTriggerFile))
            {
                try
                {
                    Properties.Settings.Default.Upgrade();
                }
                catch { }
                System.IO.File.Create(alreadyMigratedSettingsTriggerFile);
            }

            Properties.Settings.Default.Save();

            TrimCharacters = new List<char>();
            TrimCharacters.Add(',');
            TrimCharacters.Add(';');
            TrimCharacters.Add(':');
            TrimCharacters.Add('.');
            TrimCharacters.Add(')');

            CurrentTheme = new Themes.Theme();

            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            pathToIcon = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\images\\Nymphicus_icon_512_freigestellt.png";
            accountColors = new Dictionary<decimal, System.Windows.Media.SolidColorBrush>();

            ApplyProxySettings();

            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 1.0 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Initializing data stores"));

            Version installedVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            UserAgent = "Nymphicus for Windows " + Converter.prettyVersion.getNiceVersionString(installedVersion.ToString()) + " (http://www.nymphicusapp.com/windows/)";

            AllAccounts = new ThreadSaveObservableCollection<IAccount>();
            if (useAppNetTestAccount)
            {
                AllAccounts.Add(apnAccount);
            }
            AllTwitterAccounts = new ThreadSaveObservableCollection<AccountTwitter>();
            AllFacebookAccounts = new ThreadSaveObservableCollection<AccountFacebook>();
            AllQuoteFmAccounts = new ThreadSaveObservableCollection<AccountQuoteFM>();
            AllApnAccounts = new ThreadSaveObservableCollection<AccountAppDotNet>();
            AllAccounts.CollectionChanged +=new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllAccounts_CollectionChanged);
            AllLists = new ThreadSaveObservableCollection<TweetList>();
            AllSearches = new ThreadSaveObservableCollection<Search>();
            AllViews = new ThreadSaveObservableCollection<View>();
            AllFilters = new ThreadSaveObservableCollection<Filter>();
            AllPersons = new ThreadSaveObservableCollection<Person>();
            AllKnownUsernames = new ThreadSaveObservableCollection<string>();
            AllKnownHashtags = new ThreadSaveObservableCollection<string>();
            AllPersons.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllPersons_CollectionChanged);
            NotWorkingAccounts = new ObservableCollection<IAccount>();
            NotWorkingViews = new List<string>();
            GoogleReaderFeedIcons = new Dictionary<string, string>();

            NewViewEntries = new ThreadSaveObservableCollection<ViewEntry>();
            NewViewEntries.CollectionChanged +=new System.Collections.Specialized.NotifyCollectionChangedEventHandler(NewViewEntries_CollectionChanged);
            ReadTweets = new ThreadSaveObservableCollection<decimal>();

            #region old read stuff
            if (false && !string.IsNullOrEmpty(Properties.Settings.Default.ReadTweets))
            {
                string[] delimiterIds = { "," };
                string[] readTweetsStringArray = Properties.Settings.Default.ReadTweets.Split(delimiterIds, StringSplitOptions.RemoveEmptyEntries);
                foreach (string readTweetString in readTweetsStringArray)
                {
                    decimal readTweetId = 0;
                    bool success = decimal.TryParse(readTweetString, out readTweetId);
                    if (success)
                    {
                        if (readTweetId > 0)
                        {
                            if (!ReadTweets.Contains(readTweetId))
                            {
                                ReadTweets.Add(readTweetId);
                            }
                        }
                    }
                }
            }

            #endregion

            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 2.0 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Initializing image services"));

            AllImageServices = new ObservableCollection<API.ImageServices.IImageService>();
            AllImageServices.Add(new API.ImageServices.imgLy());
            API.ImageServices.TwitPic twitPicService = new API.ImageServices.TwitPic();
            AllImageServices.Add(twitPicService);
            Properties.Settings.Default.DefaultImageService = "TwitPic";
            //AllImageServices.Add(new API.ImageServices.TwitPic());
            AllImageServices.Add(new API.ImageServices.yfrog());
            AllImageServices.Add(new API.ImageServices.Instagram());
            AllImageServices.Add(new API.ImageServices.Plixi());
            //selectImageService();
            ActualImageService = twitPicService;
            AllImagesInItems = new Dictionary<string, string>();

            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 3.0 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Initializing Url shortener"));

            AlllinkShortenerServices = new ObservableCollection<API.UrlShortener.ILinkShortener>();
            AlllinkShortenerServices.Add(new API.UrlShortener.BitLy());
            AlllinkShortenerServices.Add(new API.UrlShortener.IsGd());
            AlllinkShortenerServices.Add(new API.UrlShortener.TinyUrl());
            selectUrlShortener();
            AllShortenedLinksInItems = new Dictionary<string, string>();

            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 3.5 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Initializing external services"));
            AllIStores = new ObservableCollection<ExternalServices.IStore>();
            AllIStores.Add(new ExternalServices.Pocket());
            AllIStores.Add(new ExternalServices.Instapaper());
            AllIStores.Add(new ExternalServices.Delicious());
            AllIStores.Add(new ExternalServices.Pinboard());



            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 4.0 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Registering with Snarl"));

            archiveOfSentNotifications = new Dictionary<int, IItem>();
            archiveOfLinksInActionMenus = new Dictionary<int, string>();
            snarl = new ExternalLibraries.Notifications.Snarl.SnarlInterface();
            if (ExternalLibraries.Notifications.Snarl.SnarlInterface.IsSnarlRunning())
            {
                SnarlIsRunning = true;
            }
            snarl.RegisterWithEvents("Nymphicus", "Nymphicus", pathToIcon, "", IntPtr.Zero, (int)(SnarlInterface.WindowsMessage.WM_USER + 23));
            snarl.CallbackEvent += new ExternalLibraries.Notifications.Snarl.SnarlInterface.CallbackEventHandler(snarl_CallbackEvent);
            snarl.GlobalSnarlEvent += new SnarlInterface.GlobalEventHandler(snarl_GlobalSnarlEvent);
            snarl.AddClass("General info", "General info");
            //snarl.AddClass("Error", "Error");


            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 5.0 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Loading stored filters"));

            string[] delimiter = { "$$$" };

            foreach (string filterString in Properties.Settings.Default.Filter.Split(delimiter, StringSplitOptions.RemoveEmptyEntries))
            {
                Filter filter = new Filter();
                filter.readStorableSettings(filterString);
                if (filter.Name != "### ERROR ###")
                {
                    AllFilters.Add(filter);
                }
            }

            


            startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 6.0 });
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Initializing connections"));

            Logger.writeToLogfile("Starting to verify accounts");

            // t11
            // Properties.Settings.Default.Accounts = "";

            if (Properties.Settings.Default.Accounts != "")
            {
                backgroundWorkerLoadAccounts = new System.ComponentModel.BackgroundWorker();
                backgroundWorkerLoadAccounts.WorkerSupportsCancellation = true;
                backgroundWorkerLoadAccounts.WorkerReportsProgress = true;
                backgroundWorkerLoadAccounts.DoWork += new DoWorkEventHandler(backgroundWorkerLoadAccounts_DoWork);
                backgroundWorkerLoadAccounts.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerLoadAccounts_RunWorkerCompleted);
                backgroundWorkerLoadAccounts.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerLoadAccounts_ProgressChanged);
                backgroundWorkerLoadAccounts.RunWorkerAsync(Properties.Settings.Default.Accounts);
            }
            else
            {
                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 9.0 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Opening first start wizard"));
                UserInterface.FirstStartWizard firstStartWizard = new UserInterface.FirstStartWizard();
                firstStartWizard.Show();
                startupWindow.Close();
                return;

            }
        }


        void AllPersons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.NewItems != null)
                {
                    foreach (Person person in e.NewItems)
                    {
                        if (person != null)
                        {
                            if (person.Username != null)
                            {
                                if (!AllKnownUsernames.Contains("@" + person.Username))
                                {
                                    AllKnownUsernames.Add("@" + person.Username);
                                }
                            }
                        }
                    }
                }
            }
        }

        void snarl_GlobalSnarlEvent(SnarlInterface sender, SnarlInterface.GlobalEventArgs args)
        {
            if (args.GlobalEvent == SnarlInterface.GlobalEvent.SnarlLaunched)
            {
                SnarlIsRunning = true;
                snarl.RegisterWithEvents("Nymphicus", "Nymphicus", pathToIcon, "", IntPtr.Zero, (int)(SnarlInterface.WindowsMessage.WM_USER + 23));
                snarl.AddClass("General info", "General info");
                foreach (IAccount iaccount in AllAccounts)
                {
                    foreach (string className in iaccount.AvailableNotificationClasses)
                    {
                        snarl.AddClass(className, className);
                    }
                }
            }
            else if (args.GlobalEvent == SnarlInterface.GlobalEvent.SnarlQuit)
            {
                SnarlIsRunning = false;
            }
        }

        void backgroundWorkerLoadAccounts_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            if (progress == 666)
            {
                finishInitialLoading();
            }
            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Loading accounts"));
            IAccount account = (IAccount)e.UserState;
            

            if (account != null)
            {
                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, (Math.Min(6.0 + progress,7.0)) });
                if (account.LoginSuccessfull)
                {
                    Logger.writeToLogfile("Account @" + account.username + " working");
                    if (account.GetType() == typeof(AccountTwitter))
                    {
                        startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupSubWorkstep.Content = "@" + account.username + " loaded and verified"));
                    }
                    else if(account.GetType() == typeof(AccountFacebook))
                    {
                        AccountFacebook fbAccount = account as AccountFacebook;
                        if(fbAccount != null) {
                            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupSubWorkstep.Content = "Facebook " + fbAccount.FullName + " loaded and verified"));
                        }
                    }
                    else if (account.GetType() == typeof(AccountQuoteFM))
                    {
                        AccountQuoteFM qfmAccount = account as AccountQuoteFM;
                        if (qfmAccount != null)
                        {
                            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupSubWorkstep.Content = "QUOTE.fm " + qfmAccount.username + " loaded and verified"));
                        }
                    }
                    else if (account.GetType() == typeof(AccountAppDotNet))
                    {
                        AccountAppDotNet apnAccount = account as AccountAppDotNet;
                        if (apnAccount != null)
                        {
                            startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupSubWorkstep.Content = "App.net @" + apnAccount.username + " loaded and verified"));
                            if (apnAccount.token != null)
                            {
                                if (!apnAccount.token.scopes.Contains("files"))
                                {
                                    startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupSubWorkstep.Content = "App.net @" + apnAccount.username + " old token - starting reauthorization"));
                                    AllAccounts.Remove(apnAccount);
                                    AccountAppDotNet.authorizeNewAccount();
                                }
                            }
                        }
                    }

                    Logger.writeToLogfile("Account " + account.username + " adding to AllAccounts list");
                    AllAccounts.Add(account);
                    Logger.writeToLogfile("Account " + account.username + " registering account");
                    account.registerAccount();

                }
                else
                {
                    Logger.writeToLogfile("Account @" + account.username + " NOT working");
                    startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupSubWorkstep.Content = "Could not verify @" + account.username));

                    NotWorkingAccounts.Add(account);
                    
                    if (account.GetType() == typeof(AccountAppDotNet))
                    {
                        AccountAppDotNet.authorizeNewAccount();
                    }
                }

            }
            
        }


        void backgroundWorkerLoadAccounts_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
         
            // überholt die ProgressChanged events am Ende
        }

        void finishInitialLoading()
        {
           // Console.WriteLine("Account loading completed - going ahead");
           

            if (NotWorkingAccounts.Count > 0)
            {
                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Normal, new object[] { ProgressBar.ValueProperty, 9.0 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Not all accounts have been read..."));
                UserInterface.ReenableAccounts reenableAccounts = new UserInterface.ReenableAccounts();
               // reenableAccounts.Show();
            }
            
            

                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 7.0 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Loading views"));

                loadStoredViews();

                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 7.5 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Initiating items fetch"));

                updateAllAccounts();

                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Normal, new object[] { ProgressBar.ValueProperty, 8.0 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Opening main window"));

                openMainWindow();

                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Normal, new object[] { ProgressBar.ValueProperty, 9.0 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Checking license"));

                Properties.Settings.Default.IsValidLicense = true;

            
            try
            {
                startupWindow.Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, 10.0 });
                startupWindow.Dispatcher.BeginInvoke(new Action(() => startupWindow.labelStartupWorkstep.Content = "Checking for updates"));
            }
            catch { }

            AllAccounts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllAccounts_CollectionChanged);

            backgroundWorkerCheckUpdate = new BackgroundWorker();
            backgroundWorkerCheckUpdate.WorkerSupportsCancellation = true;
            backgroundWorkerCheckUpdate.DoWork += new DoWorkEventHandler(backgroundWorkerCheckUpdate_DoWork);
            backgroundWorkerCheckUpdate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerCheckUpdate_RunWorkerCompleted);
            backgroundWorkerCheckUpdate.RunWorkerAsync();

            startupWindow.Close();
        }

    


        public void loadStoredViews()
        {
            string[] delimiter = { "$$$" };

            foreach (string viewString in Properties.Settings.Default.Views.Split(delimiter, StringSplitOptions.RemoveEmptyEntries))
            {
                View view = new View();
                view.readStorableSettings(viewString);
                if (view.Name != "ERROR")
                {
                    view.subscribeToApnPersonalStream(0);
                    AllViews.Add(view);
                }
                else
                {
                    NotWorkingViews.Add(viewString);
                }
            }
        }

        void backgroundWorkerCheckUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UserInterface.UpdateAvailable.updateCheckDoneEventArgs doneArgs = e.Result as UserInterface.UpdateAvailable.updateCheckDoneEventArgs;
            if (doneArgs.updateFound)
            {
                UserInterface.UpdateAvailable myUpdateCheck = new UserInterface.UpdateAvailable(doneArgs.toBeIgnoredVersion, doneArgs.title, doneArgs.text);
                myUpdateCheck.updateCheckDone += new UserInterface.UpdateAvailable.updateCheckDoneEventHandler(myUpdateCheck_updateCheckDone);
            }
        }

        public void BlockUser(AccountTwitter account, Person person)
        {
            BlockUserOptions options = new BlockUserOptions();
            options.ScreenName = person.Username;
            TwitterUser user = account.twitterService.BlockUser(options);
            if (user != null)
            {

                AppController.Current.sendNotification("General info", "User has been blocked", "User @" + person.Username + " has been blocked successfully", null, null);
            }
            else
            {
                AppController.Current.sendNotification("General info", "Blocking failed", "", null, null);
            }
                 
        }

        public void ReportUser(AccountTwitter account, Person person)
        {
            ReportSpamOptions options = new ReportSpamOptions();
            options.ScreenName = person.Username;
            TwitterUser user = account.twitterService.ReportSpam(options);

            if (user != null)
            {
                AppController.Current.sendNotification("General info", "User has been reported", "User @" + person.Username + " has been reported as being a spammer successfully", null, null);
            }
            else
            {
                AppController.Current.sendNotification("General info", "Spam reporting failed", "", null, null);
            }
                 
        }

        void backgroundWorkerCheckUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            UserInterface.UpdateAvailable.updateCheckDoneEventArgs doneArgs = UserInterface.UpdateAvailable.checkNow();
            e.Result = doneArgs;
        }

        void myUpdateCheck_updateCheckDone(object sender, UserInterface.UpdateAvailable.updateCheckDoneEventArgs e)
        {
            if (!e.tryNextTimeAgain)
            {
                Properties.Settings.Default.IgnoredNewVersion = e.toBeIgnoredVersion;
            }
            else if (e.closeApp)
            {
                App.Current.Shutdown();
            }
        }

        void backgroundWorkerLoadAccounts_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] delimiter = { "$$$" };
            string allAccountsString = e.Argument as string;
           
            string[] allAccountsArray = allAccountsString.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string accountString in allAccountsArray)
            {
                i++;
                AppController.Current.Logger.writeToLogfile("Checking account number " + i.ToString() + " of " + allAccountsArray.Length.ToString() + " with string <<<" + accountString + ">>>\r\n");                
                if (accountString.StartsWith("Facebook"))
                {
                    AccountFacebook account = new AccountFacebook();
                    account.readStorableSettings(accountString);
                    AppController.Current.Logger.writeToLogfile("Facebook Account " + account.username + " has been read");
                    backgroundWorkerLoadAccounts.ReportProgress((i * 100) / allAccountsArray.Length, account);
                }
                else if (accountString.StartsWith("QuoteFM"))
                {
                    AccountQuoteFM account = new AccountQuoteFM();
                    account.readStorableSettings(accountString);
                    AppController.Current.Logger.writeToLogfile("QUOTE.fm Account " + account.username + " has been read");
                    backgroundWorkerLoadAccounts.ReportProgress((i * 100) / allAccountsArray.Length, account);
                }
                else if (accountString.StartsWith("App.Net"))
                {
                    AccountAppDotNet account = new AccountAppDotNet();
                    account.readStorableSettings(accountString);
                    AppController.Current.Logger.writeToLogfile("App.net account @" + account.username + " has been read");
                    backgroundWorkerLoadAccounts.ReportProgress((i * 100) / allAccountsArray.Length, account);
                }
                else
                {
                    AccountTwitter account = new AccountTwitter();
                    account.readStorableSettings(accountString);
                    AppController.Current.Logger.writeToLogfile("Twitter Account " + account.username + " has been read");
                    backgroundWorkerLoadAccounts.ReportProgress((i * 100) / allAccountsArray.Length, account);
                }
            }


            while (AppController.Current.AllAccounts.Count() + AppController.Current.NotWorkingAccounts.Count() < allAccountsArray.Length)
            {
                System.Threading.Thread.Sleep(250);
            }
            backgroundWorkerLoadAccounts.ReportProgress(666);
        }

        void AllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IAccount iaccount in e.NewItems)
                {


                    if (iaccount.GetType() == typeof(AccountTwitter))
                    {
                        AccountTwitter account = (AccountTwitter)iaccount;
                        AllTwitterAccounts.Add(account);
                    }
                    else if (iaccount.GetType() == typeof(AccountFacebook))
                    {
                        AccountFacebook account = (AccountFacebook)iaccount;
                        AllFacebookAccounts.Add(account);
                    }
                    else if (iaccount.GetType() == typeof(AccountQuoteFM))
                    {
                        AccountQuoteFM account = (AccountQuoteFM)iaccount;
                        AllQuoteFmAccounts.Add(account);
                        QuoteFmCategories.Create();
                    }
                    else if (iaccount.GetType() == typeof(AccountAppDotNet))
                    {
                        AccountAppDotNet account = (AccountAppDotNet)iaccount;
                        AllApnAccounts.Add(account);
                    }
                    else
                    {
                        AppController.Current.Logger.writeToLogfile("Unknown account type added: " + iaccount.GetType().ToString());
                    }
                }
            }

            if (e.OldItems != null)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    foreach (IAccount iaccount in e.OldItems)
                    {
                        if (iaccount.GetType() == typeof(AccountTwitter))
                        {
                            AccountTwitter account = (AccountTwitter)iaccount;
                            if (AllTwitterAccounts.Contains(account))
                            {
                                AllTwitterAccounts.Remove(account);
                            }
                        }
                        else if (iaccount.GetType() == typeof(AccountFacebook))
                        {
                            AccountFacebook account = (AccountFacebook)iaccount;
                            if (AllFacebookAccounts.Contains(account))
                            {
                                AllFacebookAccounts.Remove(account);
                            }
                        }

                        else if (iaccount.GetType() == typeof(AccountQuoteFM))
                        {
                            AccountQuoteFM account = (AccountQuoteFM)iaccount;
                            if (AllQuoteFmAccounts.Contains(account))
                            {
                                AllQuoteFmAccounts.Remove(account);
                            }
                        }
                        else if (iaccount.GetType() == typeof(AccountAppDotNet))
                        {
                            AccountAppDotNet account = (AccountAppDotNet)iaccount;
                            if (AllApnAccounts.Contains(account))
                            {
                                AllApnAccounts.Remove(account);
                            }
                        }
                        else
                        {
                            AppController.Current.Logger.writeToLogfile("Unknown account type removed: " + iaccount.GetType().ToString());
                        }
                    }
            }
            
            NotifyPropertyChanged("HasFacebookAccounts");
            NotifyPropertyChanged("HasTwitterAccounts");

            NotifyPropertyChanged("HasQuoteFmAccounts");
            NotifyPropertyChanged("HasApnAccounts");


        }

       
      
        public void updateAllAccounts()
        {
            foreach (IAccount account in AllAccounts)
            {
                account.UpdateItems();
            }
            if (HasQuoteFmAccounts && QuoteFmCategories.Categories != null)
            {
                QuoteFmCategories.Categories.UpdateItems();
            }
            saveAccountsAndViews();
            if (AllAccountsHaveInitialFetchDone)
            {
                scrollToPositionMarkerNow();
            }
        }



        #endregion

        public void openMainWindow()
        {
            if (mainWindow == null)
            {
                createMainWindow();
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastUsedView))
            {
                try
                {
                    View lastSelectedView = AllViews.Where(v => v.Name == Properties.Settings.Default.LastUsedView).First() as View;
                    if (lastSelectedView != null)
                    {
                        mainWindow.comboBoxViews.SelectedItem = lastSelectedView;
                    }

                }
                catch
                { }
            }
            mainWindow.ListOfItems.listView_Tweets.Items.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Descending));
            mainWindow.Show();
            mainWindow.Focus();
            if (Properties.Settings.Default.StartMinimizedToTray)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        private void createMainWindow()
        {
            mainWindow = new UserInterface.Mainwindow();
            mainWindow.DataContext = this;
            View myView = new View();
            //AllViews.Add(myView);
            mainWindow.comboBoxViews.ItemsSource = AllViews;
            if (AllViews.Count > 0)
            {
                mainWindow.comboBoxViews.SelectedIndex = 0;
            }
            TimeSpan myTimespan = new TimeSpan(0, 1, 30);
            mainWindow.dispatcherTimer.Interval = myTimespan;
            mainWindow.dispatcherTimer.Start();
            saveAccountsAndViews();
        }

        private void loadStoredPreferences() {

        }


        public AccountTwitter addAccountFromTwitterTokens(OAuthAccessToken token)
        {
            AccountTwitter newAccount = new AccountTwitter();
            newAccount.createFromTweetSharp(token);

            AllAccounts.Add(newAccount);
            
            return newAccount;
        }

        public void removeAccount(IAccount account)
        {
            if(AllAccounts.Contains(account)) {
                if (AllAccounts.Count == 1)
                {
                    // we close the main window as no account could be displayed
                    mainWindow.Close();
                    Properties.Settings.Default.Accounts = "";
                    mainWindow = null;
                }
                AllAccounts.Remove(account);
            }
        }

        public TwitterItem writeNewTweet(AccountTwitter account, string text, string upload_image_path = null)
        {
            TwitterItem item = API.Functions.writeNewTweet(account, text, upload_image_path);
            if (item != null)
            {
                account.UpdateTimeline();
                return item;
            }
            else
            {
                return null;
            }
        }

        public TwitterItem replyToTweet(AccountTwitter account, string text, decimal inReplyToId, string upload_image_paht = null)
        {
            TwitterItem item = API.Functions.replyToItem(account, inReplyToId, text, upload_image_paht);
            if (item != null)
            {
                account.UpdateTimeline();
                return item;
            }
            else
            {
                return null;
            }
        }

        public TwitterItem writeDirectMessage(AccountTwitter account, string text, string receiver)
        {
            TwitterItem item = API.Functions.writeDirectMessage(account, receiver, text);
            if (item != null)
            {
                item.isDirectMessage = true;
                item.IsSentDirectMessage = true;
                if (!account.IsStreamingAccount)
                {
                    account.AddItemToCollection(account.DirectMessages,item,"Direct messages");
                }
                return item;
            }
            else
            {
                return null;
            }
        }

        public void retweet(AccountTwitter account, TwitterItem item)
        {
            decimal itemId = item.Id;
            if (item.RetweetedItem != null)
            {
                 itemId = item.RetweetedItem.Id;
            }
            TwitterItem retweetedItem = API.Functions.retweetItem(account, item);


            if (retweetedItem != null)
            {
                retweetedItem.isRetweetedByMe = true;

                if (account.Timeline.Contains(item))
                {
                    account.Timeline.Remove(item);
                }
                account.AddItemToCollection(account.Retweets, item, "Retweets");
                account.UpdateRetweets();
                AppController.Current.sendNotification("General info", "Item has been retweeted", "Retweet of item successfull using account @" + account.Login.Username, null, null);
            }
            else
            {
                AppController.Current.sendNotification("General info", "Retweet failed", "Retweeting failed with an unknown error", null, null);
            }

               
        }

        public void favoriteItem(TwitterItem item)
        {
           
            decimal itemId = item.Id;
            bool currentlyFaved = false;
            if (item.RetweetedItem != null)
            {
                itemId = item.RetweetedItem.Id;
                currentlyFaved = item.RetweetedItem.isFavorited;
            }
            else
            {
                currentlyFaved = item.isFavorited;
            }
            AccountTwitter account = getAccountForId(item.accountId);
            if (account == null)
            {
                AppController.Current.Logger.writeToLogfile("Can not fav as didn't get the account");

            }
            else
            {
                if (!currentlyFaved)
                {
                    bool success = API.Functions.setFavoritState(account, item, true);

                    item.isFavorited = true;
                    account.AddItemToCollection(account.Favorites, item, "Favorites");
                    AppController.Current.sendNotification("General info", "Item has been favorited", "Fav of item successfull using account @" + account.Login.Username, null, null);

                }
                else
                {

                    bool success = API.Functions.setFavoritState(account, item, true);

                    item.isFavorited = true;
                    if (account.Favorites.Contains(item))
                    {
                        account.Favorites.Remove(item);
                    }

                    AppController.Current.sendNotification("General info", "Item has been unfavorited", "Unfaving of item successfull using account @" + account.Login.Username, null, null);

                }
            }
        }

        public AccountTwitter getAccountForId(decimal id)
        {
            return AllAccounts.Where(account => account.Id == id).FirstOrDefault() as AccountTwitter;
        }

        public TweetList getListForId(decimal id)
        {
            return AllLists.Where(list => list.Id == id).FirstOrDefault() as TweetList;
        }

        public Search getSearchForId(decimal id)
        {
            return AllSearches.Where(search => search.Id == id).FirstOrDefault() as Search;
        }

        public void refreshMainWindow()
        {
            if (mainWindow != null)
            {
                mainWindow.ListOfItems.listView_Tweets.Items.Refresh();
            }
        }

        public static void Stop() {
            if (Current != null)
            {
                if (Current.mainWindow != null)
                {
                    Current.mainWindow.Close();
                }
                Current = null;
            }
            App.Current.Shutdown();    
        }

        ~AppController()
        {
            snarl.UnregisterCallbackWindow();
            snarl.Unregister();
            saveAccountsAndViews();
        }

        public void saveAccountsAndViews()
        {
            if (AllAccounts.Count > 0)
            {
                List<string> accounts = new List<string>();
                foreach (IAccount account in AllAccounts)
                {
                    if (!string.IsNullOrEmpty(account.getStorableSettings()))
                    {
                        accounts.Add(account.getStorableSettings());
                    }
                }
                foreach (IAccount notWorkingAccount in NotWorkingAccounts)
                {
                    //  accounts.Add(notWorkingAccount.getStorableSettings());
                }

                Properties.Settings.Default.ReadTweets = string.Join(",", ReadTweets);

                Properties.Settings.Default.Accounts = string.Join("$$$", accounts);
                Properties.Settings.Default.Save();

                List<string> views = new List<string>();

                foreach (View view in AllViews)
                {
                    views.Add(view.getStorableSettings());
                }
                foreach (string notWorkingView in NotWorkingViews)
                {
                    // views.Add(notWorkingView);
                }
                Properties.Settings.Default.Views = string.Join("$$$", views);

                List<string> filters = new List<string>();

                foreach (Filter filter in AllFilters)
                {
                    filters.Add(filter.getStorableSettings());
                }
                Properties.Settings.Default.Filter = string.Join("$$$", filters);

                Properties.Settings.Default.Save();

            }
        }

        public void openPreferences()
        {

                UserInterface.Preferences preferences = new UserInterface.Preferences();
                preferences.Show();

        }

 
        public void openComposeNewTweet(AccountTwitter account)
        {
            openComposeNewTweet(account, null, null);
        }
        public void openComposeNewTweet(AccountTwitter account, TwitterItem inReplyToItem)
        {
            openComposeNewTweet(account, inReplyToItem, null);
        }
        public void openComposeNewTweet(AccountTwitter account, string directMessageReceiver)
        {
            openComposeNewTweet(account, null, directMessageReceiver);
        }
        public void openComposeNewTweet(AccountTwitter account, TwitterItem inReplyToItem, string directMessageReceiver)
        {
            newTweetWindow = new UserInterface.ComposeNewTweet();
            if (inReplyToItem != null)
            {
                newTweetWindow.isReplyToItem(inReplyToItem);
            }
            else if (directMessageReceiver != null)
            {
                newTweetWindow.isDirectMessage(directMessageReceiver);
            }
            if (account != null)
            {
                newTweetWindow.comboBoxAccount.comboBoxAccounts.SelectedItem = account;
            }
            newTweetWindow.Show();
            newTweetWindow.Focus();
        }
        public void openSearchWindow(string keyword, AccountTwitter account)
        {
            UserInterface.SearchResultsWindow searchWindow = new UserInterface.SearchResultsWindow(account);
            searchWindow.searchTextboxKeyword.textboxSearchString.Text = keyword;
            searchWindow.Show();
            searchWindow.executeSearch(keyword, account);

        }

        public void changeView(View view)
        {
            if (view == null)
            {
                return;
            }
            mainWindow.ListOfItems.listView_Tweets.ItemsSource = view.Items;
            mainWindow.update_filter_button(view);
        }

        public void listAdded(TweetList list)
        {
            if(!AllLists.Contains(list)) {
                AllLists.Add(list);
            }
        }
        public void listRemoved(TweetList list)
        {
            if(AllLists.Contains(list)) {
                AllLists.Remove(list);
            }
        }
        public void searchAdded(Search search)
        {
            if(!AllSearches.Contains(search)) {
                AllSearches.Add(search);
            }
        }
        public void searchRemoved(Search search)
        {
            if(AllSearches.Contains(search)) {
                AllSearches.Remove(search);
            }
        }

        #region Notifications

        public void registerTwitterAccountForNotifications(string accountName)
        {
            snarl.AddClass(accountName + " Timeline", accountName + " Timeline");
            snarl.AddClass(accountName + " Mentions", accountName + " Mentions");
            snarl.AddClass(accountName + " Retweets", accountName + " Retweets");
            snarl.AddClass(accountName + " Direct Messages", accountName + " Direct Messages");
            snarl.AddClass(accountName + " New follower", accountName + " New follower");
        }

        public void registerNotificationClass(string name)
        {
            snarl.AddClass(name, name);
        }

        public void sendNotification(string className, string title, string text, string icon,IItem iitem)
        {
            sendNotification(className, title, text, icon, 10, iitem);
        }
        public void sendNotification(string className, string title, string text, string icon, int timeout, IItem iitem)
        {
            if (className.ToLower() == "error")
            {
                Logger.writeToLogfile(title + ":::" + text);
            }
            else
            {
                if (ExternalLibraries.Notifications.Snarl.SnarlInterface.IsSnarlRunning())
                {
                    BackgroundNotifier notifyThread = new BackgroundNotifier(snarl, className, title, text, icon, timeout, iitem);
                    System.Threading.Thread newThrd = new System.Threading.Thread(new System.Threading.ThreadStart(notifyThread.run));
                    newThrd.Start();
                }
            }
        }

        private class BackgroundNotifier {
            string ClassName;
            string Title;
            string Text;
            string Icon;
            int Timeout;
            IItem CurrentItem;
            ExternalLibraries.Notifications.Snarl.SnarlInterface Snarl;

            public BackgroundNotifier(ExternalLibraries.Notifications.Snarl.SnarlInterface snarl, string className, string title, string text, string icon, int timeout, IItem iitem) {
                Snarl = snarl;
                ClassName = className;
                Title = title;
                Text = text;
                Icon = icon;
                Timeout = timeout;
                CurrentItem = iitem;
            }
            public void run() {
                if (string.IsNullOrEmpty(Icon))
                {
                    Icon = AppController.Current.pathToIcon;
                }

                if (Properties.Settings.Default.ShowClassNameInSnarlNotification)
                {
                    Text += "\r\n\r\n" + ClassName;
                }

                Int32 msgToken = Snarl.Notify(ClassName, Title, Text, Timeout, Icon, "");
                if (ClassName.ToLower() != "error" && ClassName.ToLower() != "general info" && CurrentItem != null)
                {
                    if (CurrentItem != null)
                    {
                        if (CurrentItem.GetType() == typeof(TwitterItem))
                        {
                            Snarl.AddAction(msgToken, "Reply", "@" + (int)SnarlActions.Reply);
                            Snarl.AddAction(msgToken, "Direct Message", "@" + (int)SnarlActions.DirectMessage);
                            Snarl.AddAction(msgToken, "Retweet", "@" + (int)SnarlActions.Retweet);
                            Snarl.AddAction(msgToken, "Favorite", "@" + (int)SnarlActions.Favorite);
                            if (msgToken > 0)
                            {
                                if (!AppController.Current.archiveOfSentNotifications.ContainsKey(msgToken))
                                {
                                    try
                                    {
                                        TwitterItem item = CurrentItem as TwitterItem;
                                        AppController.Current.archiveOfSentNotifications.Add(msgToken, item);
                                    }
                                    catch
                                    {
                                        // warum auch immer hier manchmal Nullpointer kommen...!?
                                    }
                                }
                            }
                        }
                        else if (CurrentItem.GetType() == typeof(FacebookItem))
                        {
                            Snarl.AddAction(msgToken, "Like", "@" + (int)SnarlActions.fbLike);
                            Snarl.AddAction(msgToken, "Write comment", "@" + (int)SnarlActions.fbComment);
                            
                            if (msgToken > 0)
                            {
                                if (!AppController.Current.archiveOfSentNotifications.ContainsKey(msgToken))
                                {
                                    try
                                    {
                                        FacebookItem item = CurrentItem as FacebookItem;
                                        AppController.Current.archiveOfSentNotifications.Add(msgToken, item);
                                    }
                                    catch
                                    {
                                        // warum auch immer hier manchmal Nullpointer kommen...!?
                                    }
                                }
                            }
                        }
                        else if (CurrentItem.GetType() == typeof(ApnItem))
                        {
                            Snarl.AddAction(msgToken, "Reply", "@" + (int)SnarlActions.Reply);
                            Snarl.AddAction(msgToken, "Repost", "@" + (int)SnarlActions.Retweet);
                            Snarl.AddAction(msgToken, "Star", "@" + (int)SnarlActions.Favorite);
                            if (msgToken > 0)
                            {
                                if (!AppController.Current.archiveOfSentNotifications.ContainsKey(msgToken))
                                {
                                    try
                                    {
                                        ApnItem item = CurrentItem as ApnItem;
                                        AppController.Current.archiveOfSentNotifications.Add(msgToken, item);
                                    }
                                    catch
                                    {
                                        // warum auch immer hier manchmal Nullpointer kommen...!?
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        void snarl_CallbackEvent(ExternalLibraries.Notifications.Snarl.SnarlInterface sender, ExternalLibraries.Notifications.Snarl.SnarlInterface.CallbackEventArgs e)
        {
            
            switch (e.SnarlEvent)
            {
                case SnarlInterface.SnarlStatus.NotifyAction:
                    HandleActionCallback(e.Parameter, e.MessageToken);
                    if (archiveOfSentNotifications.ContainsKey(e.MessageToken))
                    {
                        archiveOfSentNotifications.Remove(e.MessageToken);
                    }
                    break;

                        case SnarlInterface.SnarlStatus.CallbackInvoked:
                        case SnarlInterface.SnarlStatus.CallbackClosed:
                        case SnarlInterface.SnarlStatus.CallbackMiddleClick:
                        case SnarlInterface.SnarlStatus.CallbackTimedOut:

                    if (archiveOfSentNotifications.ContainsKey(e.MessageToken))
                    {
                        archiveOfSentNotifications.Remove(e.MessageToken);
                    }
                    break;
            }
        }

        private void HandleActionCallback(UInt16 actionData, int msgToken)
        {
            if (archiveOfSentNotifications.ContainsKey(msgToken))
            {
                IItem iitem = archiveOfSentNotifications[msgToken];
                if (iitem == null) { return; }

                if (iitem.GetType() == typeof(TwitterItem))
                {
                    TwitterItem item = iitem as TwitterItem;

                    switch ((SnarlActions)actionData)
                    {
                        case SnarlActions.Reply:
                            openComposeNewTweet(getAccountForId(item.accountId), item);
                            break;

                        case SnarlActions.Retweet:
                            retweet(getAccountForId(item.accountId), item);
                            break;

                        case SnarlActions.DirectMessage:
                            openComposeNewTweet(getAccountForId(item.accountId), item.Author.Username);
                            break;

                        case SnarlActions.Favorite:
                            favoriteItem(item);
                            item.isFavorited = true;
                            break;
                    }
                }
                else if (iitem.GetType() == typeof(FacebookItem))
                {
                    FacebookItem item = iitem as FacebookItem;

                    switch ((SnarlActions)actionData)
                    {
                        case SnarlActions.fbLike:
                            item.LikeThisItem();
                            break;

                        case SnarlActions.fbComment:
                            UserInterface.Facebook.ComposeNewComment commentComposeWindow = new UserInterface.Facebook.ComposeNewComment(item);
                            commentComposeWindow.Show();
                            break;
                    }
                }

        
                else if (iitem.GetType() == typeof(ApnItem))
                {
                    ApnItem item = iitem as ApnItem;

                    switch ((SnarlActions)actionData)
                    {
                        case SnarlActions.Reply:
                            UserInterface.ComposeNewApnPost composeWindow = new UserInterface.ComposeNewApnPost();
                            composeWindow.isReplyToItem(item);
                            composeWindow.Show();
                            composeWindow.Focus();
                            break;

                        case SnarlActions.Retweet:
                            AppNetDotNet.ApiCalls.Posts.repost(item.receivingAccount.accessToken, item.Id.ToString());
                            break;

                        case SnarlActions.Favorite:
                            AppNetDotNet.ApiCalls.Posts.star(item.receivingAccount.accessToken, item.Id.ToString());
                            item.apnItem.you_starred = true;
                            break;
                    }
                }

            }
            else
            {
                //string displayString = 
            }
        }


        #endregion

        public void filterForText(string filterText)
        {
            if (mainWindow != null)
            {
                // xxx den Zustand der Filterbuttons berücksichtigen!
                if(string.IsNullOrEmpty(filterText)) {
                    mainWindow.ListOfItems.listView_Tweets.Items.Filter = null;
                }
                mainWindow.ListOfItems.listView_Tweets.Items.Filter = delegate(object obj)
                {
                    IItem item = obj as IItem;
                    if (item != null)
                    {
                        if (!string.IsNullOrEmpty(item.Text) && !string.IsNullOrEmpty(item.AuthorName))
                        {
                            if (item.Text.ToLower().Contains(filterText.ToLower()) || item.AuthorName.ToLower().Contains(filterText.ToLower()))
                            {
                                AppController.Current.Logger.addDebugMessage("Item has been decided to be shown in text search", filterText.ToLower(), type: DebugMessage.DebugMessageTypes.Display, item: item);
                                return true;
                            }
                            else
                            {
                                AppController.Current.Logger.addDebugMessage("Item has been decided to be NOT shown in text search", filterText.ToLower(), type: DebugMessage.DebugMessageTypes.Display, item: item);
                                return false;
                            }
                        }
                        else
                        {
                            AppController.Current.Logger.addDebugMessage("Item has been decided to be NOT shown in filter", "Item text or AuthorName is null", type: DebugMessage.DebugMessageTypes.Display, item: item);
                            return false;
                        }
                    }
                    else
                    {
                        AppController.Current.Logger.addDebugMessage("Item has been decided to be NOT shown in filter", "It is null", type: DebugMessage.DebugMessageTypes.Display, item: item);
                        return false;
                    }
                };
            }
        }

        public void ApplyProxySettings()
        {
            AppController.Current.Logger.addDebugMessage("Proxy setting: Enabled", Nymphicus.Properties.Settings.Default.ProxyEnabled.ToString(), type: DebugMessage.DebugMessageTypes.Settings);
            AppController.Current.Logger.addDebugMessage("Proxy setting: Server", Nymphicus.Properties.Settings.Default.ProxyServer, type: DebugMessage.DebugMessageTypes.Settings);
            AppController.Current.Logger.addDebugMessage("Proxy setting: Port", Nymphicus.Properties.Settings.Default.ProxyPort.ToString(), type: DebugMessage.DebugMessageTypes.Settings);
            AppController.Current.Logger.addDebugMessage("Proxy setting: User", Nymphicus.Properties.Settings.Default.ProxyUser, type: DebugMessage.DebugMessageTypes.Settings);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ProxyPassword))
            {
                AppController.Current.Logger.addDebugMessage("Proxy setting: Password", "Password ist set ********", type: DebugMessage.DebugMessageTypes.Settings);
            }
            
            if (Nymphicus.Properties.Settings.Default.ProxyEnabled && Nymphicus.Properties.Settings.Default.ProxyServer != "" && Nymphicus.Properties.Settings.Default.ProxyPort != 0)
            {
                AppController.Current.Logger.addDebugMessage("Applying proxy settings", "...", type: DebugMessage.DebugMessageTypes.Settings);
                System.Net.WebProxy myProxy = new System.Net.WebProxy(Nymphicus.Properties.Settings.Default.ProxyServer, Nymphicus.Properties.Settings.Default.ProxyPort);
                if (Nymphicus.Properties.Settings.Default.ProxyUser != "")
                {
                    System.Net.NetworkCredential myCredentials = new System.Net.NetworkCredential(Nymphicus.Properties.Settings.Default.ProxyUser, Nymphicus.Properties.Settings.Default.ProxyPassword);
                    myProxy.Credentials = myCredentials;
                }
                System.Net.WebRequest.DefaultWebProxy = myProxy;
                System.Net.HttpWebRequest.DefaultWebProxy = myProxy;
                System.Net.WebRequest.DefaultWebProxy = myProxy;
            }
            else
            {
                AppController.Current.Logger.addDebugMessage("Proxy settings empty on apply", "-", type: DebugMessage.DebugMessageTypes.Settings);
                System.Net.WebRequest.DefaultWebProxy = DefaultSystemProxy;
                System.Net.HttpWebRequest.DefaultWebProxy = DefaultSystemProxy;
                System.Net.WebRequest.DefaultWebProxy = DefaultSystemProxy;
            }
            if (AllTwitterAccounts != null)
            {
                foreach (AccountTwitter account in AllTwitterAccounts)
                {
                    account.twitterService.Proxy = API.WebHelpers.getProxyString();
                }
            }

        }

        public void CheckScrollPosition()
        {
            if (CurrentView != null)
            {
                if (mainWindow.ListOfItems.CurrentlyTopMostShownItem != null)
                {
                    mainWindow.ScrollToItemInListbox(mainWindow.ListOfItems.CurrentlyTopMostShownItem as IItem);
                }
                else
                {
                    mainWindow.ListOfItems.GetCurrentlyTopmostShownItem();
                }
            }
        }

        public void selectFirstView()
        {
            if (mainWindow != null)
            {
                if (mainWindow.comboBoxViews != null)
                {
                    if (mainWindow.comboBoxViews.SelectedItem == null)
                    {
                        if (mainWindow.comboBoxViews.Items.Count > 0)
                        {
                            mainWindow.comboBoxViews.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        public void selectUrlShortener() {
            ActualLinkShortener = AlllinkShortenerServices.Where(s => s.Name == Properties.Settings.Default.DefaultUrlShortener).FirstOrDefault();
        }

        #region PropertyChangedStuff

        public void selectImageService()
        {
            ActualImageService = AllImageServices.Where(s => s.Name == Properties.Settings.Default.DefaultImageService).FirstOrDefault();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
            {
                PropertyChanged += new PropertyChangedEventHandler(AppController_PropertyChanged);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        void AppController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        #endregion

        public void DeleteTweetFromEverywhere(Nymphicus.Model.TwitterItem item)
        {
            List<Nymphicus.Model.TwitterItem> KnownInstances = new List<TwitterItem>();
            if (item != null)
            {
                KnownInstances.Add(item);
                foreach (View view in AllViews)
                {
                    view.DeleteTweetFromEverywhere(item, KnownInstances);
                }
                foreach (AccountTwitter account in AllTwitterAccounts)
                {
                    account.DeleteTweetFromEverywhere(item, KnownInstances);
                }
                if (archiveOfSentNotifications.Where(i => i.Value.Id == item.Id).Count() > 0)
                {
                    List<IItem> toBeDeletedItems = new List<IItem>();
                    
                }
                
            }

            KnownInstances = null;
        }

        public bool AllAccountsHaveInitialFetchDone { get; set; }

        private IItem ToBeScrolledToItem { get; set; }

        public void CheckIfAllAccountsHaveInitialFetchDone()
        {
            if (AllTwitterAccounts.Count == 0) { AllAccountsHaveInitialFetchDone = true; }
            if (AllAccountsHaveInitialFetchDone) { return; }
            foreach (AccountTwitter account in AllTwitterAccounts)
            {
                if (!account.AllInitalFetchesCompleted)
                {
                    return;
                }
            }
            foreach (AccountAppDotNet account in AllApnAccounts)
            {
                if (!account.allInitialFetchesHaveBeenCompleted)
                {
                    return;
                }
            }
            AllAccountsHaveInitialFetchDone = true;
            scrollToPositionMarkerNow();
            
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ToBeScrolledToItem == null) { return; }
            if (mainWindow.ListOfItems.listView_Tweets.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                var info = mainWindow.ListOfItems.listView_Tweets.Items[mainWindow.ListOfItems.listView_Tweets.Items.Count - 1] as IItem;
                if (info == null)
                    return;

                if (ToBeScrolledToItem != null)
                {
                    try
                    {
                        mainWindow.ScrollToItemInListbox(ToBeScrolledToItem);
                        ToBeScrolledToItem = null;
                    }
                    catch { }
                }
            }
        }

        public void scrollToPositionMarkerNow()
        {
            if(AllAccountsHaveInitialFetchDone && mainWindow != null)
            {

                View currentlySelectedView = AppController.Current.CurrentView;
                if (currentlySelectedView != null)
                {
                    currentlySelectedView.GetPositionMarkerItem();
                }
            }
        }
        public void scrollToItem(IItem item, bool scrollInAnyCase = false)
        {
            if (item != null)
            {
                if ((mainWindow.lastOpenedView != CurrentView) || OverrideScrollNow || scrollInAnyCase)
                {
                    //sendNotification("General", "Scroll to item started", item.AuthorName + "\n" + item.Text, "", item);
                    mainWindow.TopMostShownItem = item;
                    mainWindow.ScrollToItemInListbox(item);
                    ToBeScrolledToItem = item;
                    OverrideScrollNow = false;
                }
            }
        }

        public int NewItemsNotShownAlready { get; set; }

        public void newItemsHasBeenAddedToView(View view, IItem item)
        {    
            NewViewEntries.Add(new ViewEntry(view,item));
        }

        void NewViewEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //uuu
            return;
            try
            {
                mainWindow.Title = "Nymphicus";
                if (e != null)
                {
                    if (e.NewItems != null)
                        if (!CurrentlyInBackground || !AllAccountsHaveInitialFetchDone)
                        {
                            NewViewEntries.Clear();
                            NewItemsNotShownAlready = 0;
                            mainWindow.clearOverlay();
                        }
                        else
                        {
                            foreach (ViewEntry viewEntry in e.NewItems)
                            {
                                if (viewEntry.view == CurrentView && AllAccountsHaveInitialFetchDone)
                                {
                                    NewItemsNotShownAlready++;
                                    if (NewItemsNotShownAlready > 1)
                                    {
                                        mainWindow.Title = "Nymphicus (" + NewItemsNotShownAlready.ToString() + " new items)";
                                    }
                                    else
                                    {
                                        mainWindow.Title = "Nymphicus (" + NewItemsNotShownAlready.ToString() + " new item)";
                                    }
                                    mainWindow.setIconOverlayForNewItems(NewItemsNotShownAlready);
                                }
                            }
                        }
                }
            }
            catch {}
        }


        public bool CurrentlyInBackground { get; set; }

        public View CurrentView
        {
            get
            {
                if (mainWindow != null)
                {
                    try
                    {
                        return mainWindow.comboBoxViews.SelectedItem as View;
                    }
                    catch {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static string GetDefaultExeConfigPath(ConfigurationUserLevel userLevel)
        {
            try
            {
                var UserConfig = ConfigurationManager.OpenExeConfiguration(userLevel);
                return UserConfig.FilePath;
            }
            catch (ConfigurationException e)
            {
                return e.Filename;
            }
        }

        public void liveUpdateScrollPosition()
        {
            if (CurrentView != null)
            {
                CurrentView.liveUpdateScrollPositionAsStreamMarkerChanged();
            }
        }
    }

}
