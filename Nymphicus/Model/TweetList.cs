using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TweetSharp;

namespace Nymphicus.Model
{
    public class TweetList
    {
        public ThreadSaveObservableCollection<TwitterItem> Items;
        public ObservableCollection<Person> Members;
        public Person person;
        public string name { get; set; }
        public string NameAndCreator { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public long Id { get; set; }
        public string Slug { get; set; }
        private decimal higehstKnownId;
        public decimal AccountId { get; set; }
        public AccountTwitter Account { get; set; }
        public bool IsOwnList { get; set; }
        private bool Busy = false;
        public decimal tweetMarkerLastKnown = 0;

        public DateTime LastStartOfBackgroundWorker { get; set; }
        public TimeSpan BusyTime
        {
            get
            {
                if (backgroundWorkerList.IsBusy)
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

        private BackgroundWorker backgroundWorkerList;

        public TweetList(AccountTwitter ParentAccount)
        {
            Items = new ThreadSaveObservableCollection<TwitterItem>();
            Members = new ObservableCollection<Person>();
            Account = ParentAccount;
            backgroundWorkerList = new BackgroundWorker();
            backgroundWorkerList.WorkerReportsProgress = true;
            backgroundWorkerList.WorkerSupportsCancellation = true;
            backgroundWorkerList.DoWork += new DoWorkEventHandler(backgroundWorkerList_DoWork);
            backgroundWorkerList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerList_RunWorkerCompleted);
            backgroundWorkerList.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerList_ProgressChanged);

            IsOwnList = true;
        }

        void backgroundWorkerList_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TwitterItem item = (TwitterItem)e.UserState;
            if (Items.Where(sItem => sItem.Id == item.Id).Count() == 0)
            {
                item.isList = true;
                item.listName = this.name;
                item.accountId = AccountId;
                try
                {
                    Items.Add(item);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile("Adding list item to collection failed");
                    AppController.Current.Logger.writeToLogfile(exp);
                    return;
                }

                if (InitialFetchDone)
                {
                    if (item.RetweetedItem != null)
                    {
                        AppController.Current.sendNotification("List " + FullName, item.RetweetedItem.Author.NameAndLogin, item.RetweetedItem.Text, item.RetweetedItem.Author.Avatar, item.RetweetedItem);
                    }
                    else
                    {
                        AppController.Current.sendNotification("List " + FullName, item.Author.NameAndLogin, item.Text, item.Author.Avatar, item);
                    }
                }

                if (item.Id > higehstKnownId)
                {
                    higehstKnownId = item.Id;
                }
            }
            else
            {
                item = null;
            }
        }

        void backgroundWorkerList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialFetchDone)
            {
                InitialFetchDone = true;
                Account.CheckIfAllFetchesAreComplete();
            }
            Busy = false;
        }

        void backgroundWorkerList_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
            AccountTwitter account = (AccountTwitter)e.Argument;
            List<TwitterItem> items;
            try
            {
                TwitterService listService = new TwitterService(Nymphicus.API.ConnectionData.twitterConsumerKey, Nymphicus.API.ConnectionData.twitterConsumerSecret, this.Account.Token, this.Account.TokenSecret);
                listService.Proxy = API.WebHelpers.getProxyString();
                listService.UserAgent = "Nymphicus for Windows";

                if (e != null)
                {
                    if (e.Cancel)
                    {
                        return;
                    }
                }
                items = API.Functions.getListItems(listService, account, person.Username, name, Id, Slug, higehstKnownId, e);
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
                backgroundWorkerList.ReportProgress(100, item);
            }
        }

        public void UpdateItems(AccountTwitter account)
        {
            if (!backgroundWorkerList.IsBusy)
            {
                Busy = false;
                LastStartOfBackgroundWorker = DateTime.Now;
                backgroundWorkerList.RunWorkerAsync(account);
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("List background thread of " + FullName + " is busy");
                if (Busy)
                {
                    AppController.Current.Logger.writeToLogfile("Retweets background thread of " + FullName + " is busy and has been before - trying to cancel");
                    backgroundWorkerList.CancelAsync();
                }
                Busy = true;
            }
        }

        public void UpdateMembers(AccountTwitter account)
        {
            Members.Clear();
            foreach (Person singlePerson in API.Functions.getListMembers(account, person.Username, name))
            {
                Members.Add(singlePerson);
            }
        }

        public override string ToString()
        {
            return NameAndCreator;
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

        public decimal getTweetMarker()
        {
            tweetMarkerLastKnown = API.Tweetmarker.getTweetMark(Account, "lists." + Id.ToString());
            return tweetMarkerLastKnown;
        }

        public void storeTweetMarker(decimal id)  {
            API.Tweetmarker.storeTweetMark(Account, "lists" + id.ToString(), id);
        }
    }
}
