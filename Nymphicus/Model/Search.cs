using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TweetSharp;
using System.ComponentModel;

namespace Nymphicus.Model
{
    public class Search
    {
        private TwitterSavedSearch savedSearch;
        public ThreadSaveObservableCollection<TwitterItem> Items;
        public string query { get; set; }
        public string name { get; set; }
        public decimal Id { get; set; }
        public string Positon { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal HighestKnownId { get; set; }
        public decimal AccountId { get; set; }
        public AccountTwitter Account { get; set; }
        public bool Busy = false;
        public DateTime LastStartOfBackgroundWorker { get; set; }
        public decimal tweetMarkerLastKnown { get; set; }

        public TimeSpan BusyTime
        {
            get
            {
                if (backgroundWorkerSearch.IsBusy)
                {
                    return DateTime.Now.Subtract(LastStartOfBackgroundWorker);
                }
                else
                {
                    return new TimeSpan(0, 0, 0);
                }
            }
        }
        public bool InitialFetchDone { get; private set; }

        private BackgroundWorker backgroundWorkerSearch;

        public Search(AccountTwitter ParentAccount)
        {
            Items = new ThreadSaveObservableCollection<TwitterItem>();
            name = "";
            query = "";
            CreatedAt = DateTime.Now;
            Account = ParentAccount;
            savedSearch = new TwitterSavedSearch();

            backgroundWorkerSearch = new BackgroundWorker();
            backgroundWorkerSearch.WorkerReportsProgress = true;
            backgroundWorkerSearch.WorkerSupportsCancellation = true;
            backgroundWorkerSearch.DoWork += new DoWorkEventHandler(backgroundWorkerSearch_DoWork);
            backgroundWorkerSearch.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerSearch_RunWorkerCompleted);
            backgroundWorkerSearch.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerSearch_ProgressChanged);
        }

        void backgroundWorkerSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;
            if (Items.Where(sItem => sItem.Id == item.Id).Count() == 0)
            {
                item.accountId = AccountId;
                try
                {
                    Items.Add(item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile("Adding search item to collection failed");
                    AppController.Current.Logger.writeToLogfile(exp);
                    return;
                }

                if (InitialFetchDone)
                {
                    if (item.RetweetedItem != null)
                    {
                        AppController.Current.sendNotification("Search " + name, item.RetweetedItem.Author.NameAndLogin, item.RetweetedItem.Text, item.RetweetedItem.Author.Avatar, item.RetweetedItem);
                    }
                    else
                    {
                        AppController.Current.sendNotification("Search " + name, item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                    }
                }

                if (item.Id > HighestKnownId)
                {
                    HighestKnownId = item.Id;
                }
            }
            else
            {
                item = null;
            }
        }

        void backgroundWorkerSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialFetchDone)
            {
                InitialFetchDone = true;
                Account.CheckIfAllFetchesAreComplete();
            }
            Busy = false;
        }

        void backgroundWorkerSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
            decimal maxId = (decimal)e.Argument;
            List<TwitterItem> items;
            try
            {
                TwitterService searchService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Account.Token, this.Account.TokenSecret);
                searchService.Proxy = API.WebHelpers.getProxyString();
                searchService.UserAgent = "Nymphicus for Windows";

                if (maxId > 0)
                {
                    items = API.Functions.executeSearch(searchService, query, savedSearch, this.Account);
                }
                else
                {
                    items = API.Functions.executeSearch(searchService, query, savedSearch, this.Account);
                }
            }
            catch
            {
                items = new List<TwitterItem>();
            }
            foreach (TwitterItem item in items)
            {
                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                if (item.Id == tweetMarkerLastKnown)
                {
                    item.IsTweetMarker = true;
                }
                backgroundWorkerSearch.ReportProgress(100, item);
            }

        }

        public void Update()
        {

            if (!backgroundWorkerSearch.IsBusy)
            {
                Busy = false;
                LastStartOfBackgroundWorker = DateTime.Now;
                AppController.Current.Logger.writeToLogfile("Search background thread of " + name + " initiated");
                backgroundWorkerSearch.RunWorkerAsync(HighestKnownId);
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Search background thread of " + name + " is busy");
                if (Busy)
                {
                    AppController.Current.Logger.writeToLogfile("Search background thread of " + name + " is busy and has been before - trying to cancel");
                    backgroundWorkerSearch.CancelAsync();
                }
                Busy = true;
            }
        }

        public void DeleteTweetFromEverywhere(TwitterItem item, List<TwitterItem> KnownInstances)
        {
            if (item != null)
            {
                List<TwitterItem> toBeDeletedItems = new List<TwitterItem>();
                foreach (TwitterItem toBeDeletedItem in Items.Where(i => i.Id == item.Id))
                {
                    toBeDeletedItems.Add(toBeDeletedItem);
                }
                foreach (TwitterItem toBeDeletedItem in toBeDeletedItems)
                {
                    try
                    {
                        Items.Remove(toBeDeletedItem);

                        if (!KnownInstances.Contains(toBeDeletedItem))
                        {
                            KnownInstances.Add(toBeDeletedItem);
                        }
                    }
                    catch { }

                }
            }
        }

        #region TweetMarker
        public decimal getTweetMarker()
        {
            return 0;
            tweetMarkerLastKnown = API.Tweetmarker.getTweetMark(Account, "searches." + Id.ToString());
            return tweetMarkerLastKnown;
        }

        public void storeTweetMarker(decimal id)
        {
            return;
            API.Tweetmarker.storeTweetMark(Account, "searches" + id.ToString(), id);
        }
        #endregion

        ~Search()
        {
            if(backgroundWorkerSearch != null) {
                backgroundWorkerSearch.CancelAsync();}
        }
    }
}
