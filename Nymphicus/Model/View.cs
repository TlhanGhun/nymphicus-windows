using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Nymphicus.Model;
using System.ComponentModel;
using System.Windows.Threading;

namespace Nymphicus.Model
{
    public class View
    {
        public ThreadSaveObservableCollection<IItem> Items { get; set; }
        
        public ThreadSaveObservableCollection<IItem> OverflowItems { get; set; }
        public ThreadSaveObservableCollection<TwitterItem> SearchResults
        {
            get;
            private set;
        }

        private BackgroundWorker backgroundWorkerStorePositionMarker;
        private DateTime? LastStoredPositionMarker;
        private BackgroundWorker backgroundWorkerGetPositionMarker;

        private DateTime initializingTime { get; set; }
        private DateTime newestDateTimeIncluded { get; set; }

        public ThreadSaveObservableCollection<IItem> FilteredItems { get; private set; }

        public bool isTwitterOnlyView { get; set; }
        private bool AllowDuplicates { get; set; }

        public ThreadSaveObservableCollection<TwitterItem> ListResults
        {
            get;
            private set;
        }

        public string Name { get; set; }

        public List<decimal> subscribedTimelines;
        public List<decimal> subscribedMentions;
        public List<decimal> subscribedDirectMessages;
        public List<decimal> subscribedRetweets;
        public List<decimal> subscribedFavorites;
        public List<decimal> subscribedLists;
        public List<decimal> subscribedSearches;

        public List<decimal> subscribedFbStatusMessages;
        public List<decimal> subscribedFbLinks;
        public List<decimal> subscribedFbVideos;
        public List<decimal> subscribedFbPhotos;
        public List<decimal> subscribedFbEvents;
        public List<decimal> subscribedFbCheckIns;
        public List<decimal> subscribedFbNotes;
        
        public List<Filter> subscribedFilter;
        
        public List<string> subscribedGoogleReaderEasyUnreadItems { get; set; }

        public List<decimal> subscribedQuoteFmRecommendations { get; set; }
        public List<decimal> subscribedQuoteFmCategories { get; set; }

        public List<decimal> subscribedApnPersonalStreams { get; set; }
        public List<decimal> subscribedApnMentions { get; set; }
        public List<decimal> subscribedApnReposts { get; set; }
        public List<decimal> subscribedApnStars { get; set; }
        public List<decimal> subscribedApnPrivateMessages { get; set; }

        public View()
        {
            Items = new ThreadSaveObservableCollection<IItem>();
            Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
            OverflowItems = new ThreadSaveObservableCollection<IItem>();
            FilteredItems = new ThreadSaveObservableCollection<IItem>();
            initializingTime = DateTime.Now;
            LastStoredPositionMarker = null;

            SearchResults = new ThreadSaveObservableCollection<TwitterItem>();
            ListResults = new ThreadSaveObservableCollection<TwitterItem>();

            subscribedTimelines = new List<decimal>();
            subscribedMentions = new List<decimal>();
            subscribedDirectMessages = new List<decimal>();
            subscribedRetweets = new List<decimal>();
            subscribedFavorites = new List<decimal>();

            subscribedLists = new List<decimal>();
            subscribedSearches = new List<decimal>();

            subscribedFilter = new List<Filter>();

            subscribedFbStatusMessages = new List<decimal>();
            subscribedFbLinks = new List<decimal>();
            subscribedFbVideos = new List<decimal>();
            subscribedFbPhotos = new List<decimal>();
            subscribedFbEvents = new List<decimal>();
            subscribedFbCheckIns = new List<decimal>();
            subscribedFbNotes = new List<decimal>();

            subscribedGoogleReaderEasyUnreadItems = new List<string>();

            subscribedQuoteFmRecommendations = new List<decimal>();
            subscribedQuoteFmCategories = new List<decimal>();

            subscribedApnPersonalStreams = new List<decimal>();
            subscribedApnMentions = new List<decimal>();
            subscribedApnPrivateMessages = new List<decimal>();
            subscribedApnReposts = new List<decimal>();
            subscribedApnStars = new List<decimal>();

            Name = "No name";
            AllowDuplicates = false;

            AppController.Current.AllLists.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllLists_CollectionChanged);
            AppController.Current.AllSearches.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllSearches_CollectionChanged);
            AppController.Current.AllAccounts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllAccounts_CollectionChanged);


            backgroundWorkerStorePositionMarker = new BackgroundWorker();
            backgroundWorkerStorePositionMarker.DoWork += new DoWorkEventHandler(backgroundWorkerStoreTweetMarker_DoWork);
            backgroundWorkerStorePositionMarker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerStoreTweetMarker_RunWorkerCompleted);

            backgroundWorkerGetPositionMarker = new BackgroundWorker();

            backgroundWorkerGetPositionMarker.WorkerReportsProgress = true;
            backgroundWorkerGetPositionMarker.DoWork += backgroundWorkerGetPositionMarker_DoWork;
            backgroundWorkerGetPositionMarker.ProgressChanged += backgroundWorkerGetPositionMarker_ProgressChanged;
            backgroundWorkerGetPositionMarker.RunWorkerCompleted += backgroundWorkerGetPositionMarker_RunWorkerCompleted;
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AppController.Current.CurrentView == this)
            {
                AppController.Current.CheckScrollPosition();
            }
        }

        void backgroundWorkerStoreTweetMarker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AppController.Current.scrollToPositionMarkerNow();
        }

        void backgroundWorkerStoreTweetMarker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null)
            {
                return;
            }
            DateTime? NewestItemDateCurrentlyDisplayed = e.Argument as DateTime?;
            if (NewestItemDateCurrentlyDisplayed == null)
            {
                return;
            }
            if (LastStoredPositionMarker != null)
            {
                if (LastStoredPositionMarker == NewestItemDateCurrentlyDisplayed)
                {
                    // those items have been stored already
                    return;
                }
            }

            LastStoredPositionMarker = NewestItemDateCurrentlyDisplayed;

            #region TweetMarker

            // Timelines
            foreach (decimal timelineId in subscribedTimelines)
            {
                AccountTwitter account = AppController.Current.getAccountForId(timelineId);
                if (account != null)
                {
                    account.tweetMarkerTimeline = saveOneTypeToTweetMarker("timeline", account, account.Timeline, NewestItemDateCurrentlyDisplayed);
                }
            }

            // Mentions
            foreach (decimal mentionsId in subscribedMentions)
            {
                AccountTwitter account = AppController.Current.getAccountForId(mentionsId);
                if (account != null)
                {
                    account.tweetMarkerMentions = saveOneTypeToTweetMarker("mentions", account, account.Mentions, NewestItemDateCurrentlyDisplayed);
                }
            }

            // Direct messages
            foreach (decimal dmsId in subscribedDirectMessages)
            {
                AccountTwitter account = AppController.Current.getAccountForId(dmsId);
                if (account != null)
                {
                    account.tweetMarkerDMs = saveOneTypeToTweetMarker("messages", account, account.DirectMessages, NewestItemDateCurrentlyDisplayed);
                }
            }

            // Favorites
            foreach (decimal favId in subscribedFavorites)
            {
                AccountTwitter account = AppController.Current.getAccountForId(favId);
                if (account != null)
                {
                    account.tweetMarkerFavorites = saveOneTypeToTweetMarker("favorites", account, account.Favorites, NewestItemDateCurrentlyDisplayed);
                }
            }


            // Lists
            foreach (decimal listId in subscribedLists)
            {
                TweetList list = AppController.Current.getListForId(listId);
                if (list != null)
                {
                    list.tweetMarkerLastKnown = saveOneTypeToTweetMarker("lists." + list.Id.ToString(), list.Account, list.Items, NewestItemDateCurrentlyDisplayed);
                }
            }

            // Searches
            foreach (decimal searchId in subscribedSearches)
            {
                Search search = AppController.Current.getSearchForId(searchId);
                if (search != null)
                {
                    search.tweetMarkerLastKnown = saveOneTypeToTweetMarker("search." + search.Id.ToString(), search.Account, search.Items, NewestItemDateCurrentlyDisplayed);
                }
            }

            #endregion

            // App.net 
            
            // personal stream
            foreach (decimal myStreamId in subscribedApnPersonalStreams)
            {
                AccountAppDotNet account = AppController.Current.AllApnAccounts.Where(a => a.Id == myStreamId).First();
                if (account != null)
                {
                    account.storeMarkerIdMyStream = saveOneTypeToStreamMarker("my_stream", account, account.PersonalStream, NewestItemDateCurrentlyDisplayed).ToString();
                }
            }

            // personal stream
            foreach (decimal myStreamId in subscribedApnMentions)
            {
                AccountAppDotNet account = AppController.Current.AllApnAccounts.Where(a => a.Id == myStreamId).First();
                if (account != null)
                {
                    account.storeMarkerIdMentions = saveOneTypeToStreamMarker("mentions", account, account.PersonalStream, NewestItemDateCurrentlyDisplayed).ToString();
                }
            }
        }

       
        public override string ToString()
        {
            return Name;
        }

        void AllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (e.NewItems.Count > 0)
                {
                    foreach (IAccount account in e.NewItems)
                    {
                        if (account != null)
                        {
                            if (account.GetType() == typeof(AccountTwitter))
                            {
                                if (subscribedTimelines.Contains(account.Id))
                                {
                                    subscribeToTimeline(account as AccountTwitter);
                                }
                                if (subscribedMentions.Contains(account.Id))
                                {
                                    subscribeToMentions(account as AccountTwitter);
                                }
                                if (subscribedDirectMessages.Contains(account.Id))
                                {
                                    subscribeToDirectMessages(account as AccountTwitter);
                                }
                                if (subscribedRetweets.Contains(account.Id))
                                {
                                    subscribeToRetweets(account as AccountTwitter);
                                }
                                if (subscribedFavorites.Contains(account.Id))
                                {
                                    // subscribeToRetweets(account as AccountTwitter);
                                }
                            }
                            else if (account.GetType() == typeof(AccountFacebook))
                            {
                                if (subscribedFbStatusMessages.Contains(account.Id))
                                {
                                    subscribeToFbStatusMessages(account as AccountFacebook);
                                }
                                if (subscribedFbLinks.Contains(account.Id))
                                {
                                    subscribeToFbLinks(account as AccountFacebook);
                                }
                                if (subscribedFbPhotos.Contains(account.Id))
                                {
                                    subscribeToFbPhotos(account as AccountFacebook);
                                }
                                if (subscribedFbVideos.Contains(account.Id))
                                {
                                    subscribeToFbVideos(account as AccountFacebook);
                                }
                                if (subscribedFbEvents.Contains(account.Id))
                                {
                                    subscribeToFbEvents(account as AccountFacebook);
                                }
                                if (subscribedFbCheckIns.Contains(account.Id))
                                {
                                    subscribeToFbCheckIns(account as AccountFacebook);
                                }
                                if (subscribedFbNotes.Contains(account.Id))
                                {
                                    subscribeToFbNotes(account as AccountFacebook);
                                }
                            }

                            else if (account.GetType() == typeof(AccountQuoteFM))
                            {
                                if (subscribedQuoteFmRecommendations.Contains(account.Id))
                                {
                                    subscribeToQuoteFmRecommendations(account.Id);
                                }
                            }
                        }
                    }
                }
            }
        }

        private AccountTwitter findAccountById(decimal id)
        {
            return AppController.Current.AllAccounts.Where(account => account.Id == id).FirstOrDefault() as AccountTwitter;
        }

        private AccountAppDotNet findAppNetAccountById(decimal id)
        {
            return AppController.Current.AllApnAccounts.Where(account => account.Id == id).FirstOrDefault() as AccountAppDotNet;
        }

        private TweetList findListById(decimal id)
        {
            return AppController.Current.AllLists.Where(list => list.Id == id).FirstOrDefault();
        }

        private Search findSearchById(decimal id)
        {
            return AppController.Current.AllSearches.Where(search => search.Id == id).FirstOrDefault();
        }
        private Filter findFilterById(decimal id)
        {
            return AppController.Current.AllFilters.Where(filter => filter.Id == id).FirstOrDefault();
        }

        private AccountFacebook findFbAccountById(decimal id)
        {
            return AppController.Current.AllAccounts.Where(account => account.Id == id).FirstOrDefault() as AccountFacebook;
        }

        #region Subscribe and unsubscribe

        #region Twitter

        #region Timeline

        public void subscribeToTimeline(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedTimelines.Contains(account.Login.Id))
            {
                subscribedTimelines.Add(account.Login.Id);
            }
            addItems(account.Timeline);
            account.Timeline.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }

        public void subscribeToTimeline(decimal accountId)
        {
            subscribeToTimeline(findAccountById(accountId));
        }


        public void unsubsribeFromTimeline(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedTimelines.Contains(account.Login.Id))
            {
                subscribedTimelines.Remove(account.Login.Id);
                account.Timeline.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                foreach (IItem item in twitterIItems)
                {
                    twitterItems.Add(item as TwitterItem);
                }
                IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => !item.isMention && !item.isDirectMessage && !item.isRetweetedToMe && item.accountId == account.Login.Id);
                List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromTimeline(decimal accountId)
        {
            unsubsribeFromTimeline(findAccountById(accountId));
        }

        #endregion

        #region Mentions

        public void subscribeToMentions(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }

            if (!subscribedMentions.Contains(account.Login.Id))
            {
                subscribedMentions.Add(account.Login.Id);
            }
            addItems(account.Mentions);
            account.Mentions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToMentions(decimal accountId)
        {
            subscribeToMentions(findAccountById(accountId));
        }
        public void unsubsribeFromMentions(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedMentions.Contains(account.Login.Id))
            {
                subscribedMentions.Remove(account.Login.Id);
                account.Mentions.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                foreach (IItem item in twitterIItems)
                {
                    twitterItems.Add(item as TwitterItem);
                }
                IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => item.isMention && item.accountId == account.Login.Id);
                List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromMentions(decimal accountId)
        {
            unsubsribeFromMentions(findAccountById(accountId));
        }

        #endregion

        #region Direct Messages

        public void subscribeToDirectMessages(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedDirectMessages.Contains(account.Login.Id))
            {
                subscribedDirectMessages.Add(account.Login.Id);
            }
            addItems(account.DirectMessages);
            account.DirectMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToDirectMessages(decimal accountId)
        {
            subscribeToDirectMessages(findAccountById(accountId));
        }
        public void unsubsribeFromDirectMessages(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedDirectMessages.Contains(account.Login.Id))
            {
                subscribedDirectMessages.Remove(account.Login.Id);
                account.DirectMessages.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                foreach (IItem item in twitterIItems)
                {
                    twitterItems.Add(item as TwitterItem);
                }
                IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => item.isDirectMessage && item.accountId == account.Login.Id);
                List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromDirectMessages(decimal accountId)
        {
            unsubsribeFromDirectMessages(findAccountById(accountId));
        }

        #endregion

        #region Retweets

        public void subscribeToRetweets(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedRetweets.Contains(account.Login.Id))
            {
                subscribedRetweets.Add(account.Login.Id);
            }
            addItems(account.Retweets);
            account.Retweets.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToRetweets(decimal accountId)
        {
            subscribeToRetweets(findAccountById(accountId));
        }
        public void unsubsribeFromRetweets(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedRetweets.Contains(account.Login.Id))
            {
                subscribedRetweets.Remove(account.Login.Id);
                account.Retweets.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                foreach (IItem item in twitterIItems)
                {
                    twitterItems.Add(item as TwitterItem);
                }
                IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => item.isRetweetedToMe && item.accountId == account.Login.Id);
                List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromRetweets(decimal accountId)
        {
            unsubsribeFromRetweets(findAccountById(accountId));
        }


        #endregion

        #region Favorites

        public void subscribeToFavorites(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFavorites.Contains(account.Login.Id))
            {
                subscribedFavorites.Add(account.Login.Id);
            }
            addItems(account.Favorites);
            account.Favorites.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFavorites(decimal accountId)
        {
            subscribeToFavorites(findAccountById(accountId));
        }
        public void unsubsribeFromFavorites(AccountTwitter account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFavorites.Contains(account.Login.Id))
            {
                subscribedFavorites.Remove(account.Login.Id);
                account.Favorites.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                foreach (IItem item in twitterIItems)
                {
                    twitterItems.Add(item as TwitterItem);
                }
                IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => item.isFavorited && item.accountId == account.Login.Id);
                List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFavorites(decimal accountId)
        {
            unsubsribeFromFavorites(findAccountById(accountId));
        }


        #endregion




        #region Lists

        public void subscribeToList(TweetList list)
        {
            if (list == null)
            {
                return;
            }
            if (!subscribedLists.Contains(list.Id))
            {
                subscribedLists.Add(list.Id);
            }
            addItems(list.Items);
            list.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToList(decimal listId)
        {
            subscribeToList(findListById(listId));
        }
        public void unsubsribeFromList(TweetList list)
        {
            if (list == null)
            {
                return;
            }
            if (subscribedLists.Contains(list.Id))
            {
                subscribedLists.Remove(list.Id);
                list.Items.CollectionChanged -= Subscribed_CollectionChanged;
                try
                {
                    IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                    ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                    foreach (IItem item in twitterIItems)
                    {
                        twitterItems.Add(item as TwitterItem);
                    }
                    IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => item.SourceListId == list.Id);
                    List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                    foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                    {
                        Items.Remove(toBeRemovedItem);
                    }
                }
                catch { }
            }
        }
        public void unsubsribeFromList(decimal listId)
        {
            unsubsribeFromList(findListById(listId));
        }

        #endregion

        #region Searches

        public void subscribeToSearch(Search search)
        {
            if (search == null)
            {
                return;
            }
            if (!subscribedSearches.Contains(search.Id))
            {
                subscribedSearches.Add(search.Id);
            }
            addItems(search.Items);
            search.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToSearch(decimal searchId)
        {
            subscribeToSearch(findSearchById(searchId));
        }
        public void unsubsribeFromSearch(Search search)
        {
            if (search == null)
            {
                return;
            }
            if (subscribedSearches.Contains(search.Id))
            {
                subscribedSearches.Remove(search.Id);
                search.Items.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                foreach (IItem item in twitterIItems)
                {
                    twitterItems.Add(item as TwitterItem);
                }
                IEnumerable<TwitterItem> toBeRemovedItemsEnumerable = twitterItems.Where(item => item.SourceSearchId == search.Id);
                List<TwitterItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<TwitterItem>();
                foreach (TwitterItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromSearch(decimal searchId)
        {
            unsubsribeFromSearch(findSearchById(searchId));
        }

        #endregion

        #region Filter

        public void subscribeToFilter(Filter filter)
        {
            if (filter == null)
            {
                return;
            }
            if (subscribedFilter.Where(f => f.Id == filter.Id).Count() == 0)
            {
                subscribedFilter.Add(filter);
                List<IItem> ToBeFilteredItems = new List<IItem>();
                foreach (IItem item in Items)
                {
                    if (!filter.ShallItemBeDisplayed(item))
                    {
                        ToBeFilteredItems.Add(item);
                    }
                }
                foreach (IItem item in ToBeFilteredItems)
                {
                    FilteredItems.Add(item);
                    Items.Remove(item);
                }
            }

        }
        public void subscribeToFilter(decimal filterId)
        {
            subscribeToFilter(findFilterById(filterId));
        }

        public void unsubsribeFromFilter(Filter filter)
        {
            if (filter == null)
            {
                return;
            }
            if (subscribedFilter.Contains(filter))
            {
                subscribedFilter.Remove(filter);
                List<IItem> items = new List<IItem>();
                foreach (IItem item in FilteredItems)
                {
                    if(!items.Contains(item)) {
                        items.Add(item);
                    }
                }
                foreach (IItem item in items)
                {
                    foreach (Filter sfilter in subscribedFilter)
                    {
                        if (!sfilter.ShallItemBeDisplayed(item))
                        {
                            return;
                        }
                    }
                    if (!Items.Contains(item))
                    {
                        Items.Add(item);

                        try
                        {
                            FilteredItems.Remove(item);
                        }
                        catch (Exception exp)
                        {
                            AppController.Current.Logger.writeToLogfile(exp);
                        }
                    }
                }
            }
        }
        public void unsubsribeFromFilter(decimal filterId)
        {
            unsubsribeFromFilter(findFilterById(filterId));
        }
        #endregion

        #endregion

        #region Facebook

        #region Status Messages

        public void subscribeToFbStatusMessages(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbStatusMessages.Contains(account.Id))
            {
                subscribedFbStatusMessages.Add(account.Id);
            }
            addItems(account.StatusMessages);
            account.StatusMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbStatusMessages(decimal accountId)
        {
            subscribeToFbStatusMessages(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbStatusMessages(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbStatusMessages.Contains(account.Id))
            {
                subscribedFbStatusMessages.Remove(account.Id);
                account.StatusMessages.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.StatusMessage));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbStatusMessages(decimal accountId)
        {
            unsubsribeFromFbStatusMessages(findFbAccountById(accountId));
        }

        #endregion

        #region Links

        public void subscribeToFbLinks(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbLinks.Contains(account.Id))
            {
                subscribedFbLinks.Add(account.Id);
            }
            addItems(account.Links);
            account.Links.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbLinks(decimal accountId)
        {
            subscribeToFbLinks(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbLinks(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbLinks.Contains(account.Id))
            {
                subscribedFbLinks.Remove(account.Id);
                account.Links.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.Link));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbLinks(decimal accountId)
        {
            unsubsribeFromFbLinks(findFbAccountById(accountId));
        }


        #endregion

        #region Photos

        public void subscribeToFbPhotos(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbPhotos.Contains(account.Id))
            {
                subscribedFbPhotos.Add(account.Id);
            }
            addItems(account.Photos);
            account.Photos.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbPhotos(decimal accountId)
        {
            subscribeToFbPhotos(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbPhotos(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbPhotos.Contains(account.Id))
            {
                subscribedFbPhotos.Remove(account.Id);
                account.Photos.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.Photo));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbPhotos(decimal accountId)
        {
            unsubsribeFromFbPhotos(findFbAccountById(accountId));
        }

        #endregion
        

        

        #region Videos

        public void subscribeToFbVideos(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbVideos.Contains(account.Id))
            {
                subscribedFbVideos.Add(account.Id);
            }
            addItems(account.Videos);
            account.Videos.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbVideos(decimal accountId)
        {
            subscribeToFbVideos(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbVideos(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbVideos.Contains(account.Id))
            {
                subscribedFbVideos.Remove(account.Id);
                account.Videos.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.Video));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbVideos(decimal accountId)
        {
            unsubsribeFromFbVideos(findFbAccountById(accountId));
        }

        #endregion

        #region CheckIns

        public void subscribeToFbCheckIns(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbCheckIns.Contains(account.Id))
            {
                subscribedFbCheckIns.Add(account.Id);
            }
            addItems(account.CheckIns);
            account.CheckIns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbCheckIns(decimal accountId)
        {
            subscribeToFbCheckIns(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbCheckIns(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbCheckIns.Contains(account.Id))
            {
                subscribedFbCheckIns.Remove(account.Id);
                account.CheckIns.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.CheckIn));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbCheckIns(decimal accountId)
        {
            unsubsribeFromFbCheckIns(findFbAccountById(accountId));
        }


        #endregion

        #region Events

        public void subscribeToFbEvents(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbEvents.Contains(account.Id))
            {
                subscribedFbEvents.Add(account.Id);
            }
            addItems(account.Events);
            account.Events.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbEvents(decimal accountId)
        {
            subscribeToFbEvents(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbEvents(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbEvents.Contains(account.Id))
            {
                subscribedFbEvents.Remove(account.Id);
                account.Events.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.Event));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbEvents(decimal accountId)
        {
            unsubsribeFromFbEvents(findFbAccountById(accountId));
        }


        #endregion

        #region Notes

        public void subscribeToFbNotes(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedFbNotes.Contains(account.Id))
            {
                subscribedFbNotes.Add(account.Id);
            }
            addItems(account.Notes);
            account.Notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }
        public void subscribeToFbNotes(decimal accountId)
        {
            subscribeToFbNotes(findFbAccountById(accountId));
        }
        public void unsubsribeFromFbNotes(AccountFacebook account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedFbNotes.Contains(account.Id))
            {
                subscribedFbNotes.Remove(account.Id);
                account.Notes.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> fbIItems = Items.Where(item => item.GetType() == typeof(FacebookItem));
                ThreadSaveObservableCollection<FacebookItem> fbItems = new ThreadSaveObservableCollection<FacebookItem>();
                foreach (IItem item in fbIItems)
                {
                    fbItems.Add(item as FacebookItem);
                }
                IEnumerable<FacebookItem> toBeRemovedItemsEnumerable = fbItems.Where(item => (item.accountId == account.Id && item.MessageType == FacebookItem.MessageTypes.Note));
                List<FacebookItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<FacebookItem>();
                foreach (FacebookItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromFbNotes(decimal accountId)
        {
            unsubsribeFromFbNotes(findFbAccountById(accountId));
        }


        #endregion




        #endregion



        #region Quote.fm

        public void subscribeToQuoteFmRecommendations(decimal accountId)
        {
            subscribeToQuoteFmRecommendations(findQuoteFmAccountById(accountId));
        }
        public void subscribeToQuoteFmRecommendations(AccountQuoteFM account)
        {
            if (account == null)
            {
                return;
            }
            if (!subscribedQuoteFmRecommendations.Contains(account.Id))
            {
                subscribedQuoteFmRecommendations.Add(account.Id);
            }
            addItems(account.Recommendations);
            account.Recommendations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }

        public void subscribeToQuoteFmCategory(decimal catId)
        {
            if (!subscribedQuoteFmCategories.Contains(catId))
            {
                subscribedQuoteFmCategories.Add(catId);
            }
            ThreadSaveObservableCollection<QuoteFmItem> collection = QuoteFmCategories.Categories.Collections[catId];
            addItems(collection);
            collection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }

        private AccountQuoteFM findQuoteFmAccountById(decimal id)
        {
            return AppController.Current.AllAccounts.Where(account => account.Id == id && account.GetType() == typeof(AccountQuoteFM)).FirstOrDefault() as AccountQuoteFM;

        }

        public void unsubsribeFromQuoteFmRecommendations(AccountQuoteFM account)
        {
            if (account == null)
            {
                return;
            }
            if (subscribedQuoteFmRecommendations.Contains(account.Id))
            {
                subscribedQuoteFmRecommendations.Remove(account.Id);
                account.Recommendations.CollectionChanged -= Subscribed_CollectionChanged;
                IEnumerable<IItem> quoteFmIItems = Items.Where(item => item.GetType() == typeof(QuoteFmItem));
                ThreadSaveObservableCollection<QuoteFmItem> quoteFmItems = new ThreadSaveObservableCollection<QuoteFmItem>();
                foreach (IItem item in quoteFmIItems)
                {
                    quoteFmItems.Add(item as QuoteFmItem);
                }
                IEnumerable<QuoteFmItem> toBeRemovedItemsEnumerable = quoteFmItems.Where(item => (item.accountId == account.Id));
                List<QuoteFmItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<QuoteFmItem>();
                foreach (QuoteFmItem toBeRemovedItem in toBeRemovedItems)
                {
                    Items.Remove(toBeRemovedItem);
                }
            }
        }
        public void unsubsribeFromQuoteFmRecommendations(decimal id)
        {
            unsubsribeFromQuoteFmRecommendations(findQuoteFmAccountById(id));
        }

        public void unsubsribeFromQuoteFmCategory(decimal catId)
        {
            if (subscribedQuoteFmCategories.Contains(catId))
            {
                subscribedQuoteFmCategories.Remove(catId);
                ThreadSaveObservableCollection<QuoteFmItem> collection = QuoteFmCategories.Categories.Collections[catId];
                collection.CollectionChanged -= Subscribed_CollectionChanged;
                foreach (QuoteFmItem catItem in collection)
                {
                    if (Items.Contains(catItem))
                    {
                        Items.Remove(catItem);
                    }
                }
            }
        }

        #endregion

        #region App.Net

        // my stream

        public void subscribeToApnPersonalStream(decimal accountId)
        {
            if (AppController.Current.useAppNetTestAccount)
            {
                subscribeToApnPersonalStream(AppController.Current.apnAccount);
            }
            else
            {
                subscribeToApnPersonalStream(findAppNetAccountById(accountId));
            }
        }
        public void subscribeToApnPersonalStream(AccountAppDotNet account)
        {
            if (AppController.Current.useAppNetTestAccount)
            {
                account = AppController.Current.apnAccount;
            }
            if (account == null)
            {
                return;
            }
            if (!subscribedApnPersonalStreams.Contains(account.Id))
            {
                subscribedApnPersonalStreams.Add(account.Id);
            }
            addItems(account.PersonalStream);
            account.PersonalStream.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }

        public void unsubsribeFromApnPersonalStream(AccountAppDotNet account)
        {
            if (account == null)
            {
                return;
            }
            try
            {
                if (subscribedApnPersonalStreams.Contains(account.Id))
                {
                    subscribedApnPersonalStreams.Remove(account.Id);
                    account.PersonalStream.CollectionChanged -= Subscribed_CollectionChanged;
                    IEnumerable<IItem> apnIItems = Items.Where(item => item.GetType() == typeof(ApnItem));
                    ThreadSaveObservableCollection<ApnItem> apnItems = new ThreadSaveObservableCollection<ApnItem>();
                    foreach (IItem item in apnIItems)
                    {
                        apnItems.Add(item as ApnItem);
                    }
                    IEnumerable<ApnItem> toBeRemovedItemsEnumerable = apnItems.Where(item => (item.accountId == account.Id) && account.PersonalStream.Contains(item));
                    List<ApnItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<ApnItem>();
                    foreach (ApnItem toBeRemovedItem in toBeRemovedItems)
                    {
                        Items.Remove(toBeRemovedItem);
                    }
                }
            }
            catch
            {
                // uuu das kann nicht so bleiben - wo kommen die null her...
            }
        }
        public void unsubsribeFromApnPersonalStream(decimal accountId)
        {
            unsubsribeFromApnPersonalStream(findAppNetAccountById(accountId));
        }

        // Mentions

        public void subscribeToApnMentions(decimal accountId)
        {
            if (AppController.Current.useAppNetTestAccount)
            {
                subscribeToApnMentions(AppController.Current.apnAccount);
            }
            else
            {
                subscribeToApnMentions(findAppNetAccountById(accountId));
            }
        }
        public void subscribeToApnMentions(AccountAppDotNet account)
        {
            if (AppController.Current.useAppNetTestAccount)
            {
                account = AppController.Current.apnAccount;
            }
            if (account == null)
            {
                return;
            }
            if (!subscribedApnMentions.Contains(account.Id))
            {
                subscribedApnMentions.Add(account.Id);
            }
            addItems(account.Mentions);
            account.Mentions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }

        public void unsubsribeFromApnMentions(AccountAppDotNet account)
        {
            if (account == null)
            {
                return;
            }
            try
            {
                if (subscribedApnMentions.Contains(account.Id))
                {
                    subscribedApnMentions.Remove(account.Id);
                    account.Mentions.CollectionChanged -= Subscribed_CollectionChanged;
                    IEnumerable<IItem> apnIItems = Items.Where(item => item.GetType() == typeof(ApnItem));
                    ThreadSaveObservableCollection<ApnItem> apnItems = new ThreadSaveObservableCollection<ApnItem>();
                    foreach (IItem item in apnIItems)
                    {
                        apnItems.Add(item as ApnItem);
                    }
                    IEnumerable<ApnItem> toBeRemovedItemsEnumerable = apnItems.Where(item => (item.accountId == account.Id) && account.Mentions.Contains(item));
                    List<ApnItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<ApnItem>();
                    foreach (ApnItem toBeRemovedItem in toBeRemovedItems)
                    {
                        Items.Remove(toBeRemovedItem);
                    }
                }
            }
            catch { }
        }
        public void unsubsribeFromApnMentions(decimal accountId)
        {
            unsubsribeFromApnMentions(findAppNetAccountById(accountId));
        }

        // private messages

        public void subscribeToApnPrivateMessages(decimal accountId)
        {
            if (AppController.Current.useAppNetTestAccount)
            {
                subscribeToApnPrivateMessages(AppController.Current.apnAccount);
            }
            else
            {
                subscribeToApnPrivateMessages(findAppNetAccountById(accountId));
            }
        }
        public void subscribeToApnPrivateMessages(AccountAppDotNet account)
        {
            if (AppController.Current.useAppNetTestAccount)
            {
                account = AppController.Current.apnAccount;
            }
            if (account == null)
            {
                return;
            }
            if (!subscribedApnPrivateMessages.Contains(account.Id))
            {
                subscribedApnPrivateMessages.Add(account.Id);
            }
            addItems(account.PrivateMessages);
            account.PrivateMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Subscribed_CollectionChanged);
        }

        public void unsubsribeFromApnPrivateMessages(AccountAppDotNet account)
        {
            if (account == null)
            {
                return;
            }
            try
            {
                if (subscribedApnPrivateMessages.Contains(account.Id))
                {
                    subscribedApnPrivateMessages.Remove(account.Id);
                    account.PrivateMessages.CollectionChanged -= Subscribed_CollectionChanged;
                    IEnumerable<IItem> apnIItems = Items.Where(item => item.GetType() == typeof(ApnItem));
                    ThreadSaveObservableCollection<ApnItem> apnItems = new ThreadSaveObservableCollection<ApnItem>();
                    foreach (IItem item in apnIItems)
                    {
                        apnItems.Add(item as ApnItem);
                    }
                    IEnumerable<ApnItem> toBeRemovedItemsEnumerable = apnItems.Where(item => (item.accountId == account.Id) && account.PrivateMessages.Contains(item));
                    List<ApnItem> toBeRemovedItems = toBeRemovedItemsEnumerable.ToList<ApnItem>();
                    foreach (ApnItem toBeRemovedItem in toBeRemovedItems)
                    {
                        Items.Remove(toBeRemovedItem);
                    }
                }
            }
            catch
            {
                // uuu das kann nicht so bleiben - wo kommen die null her...
            }
        }
        public void unsubsribeFromApnPrivateMessages(decimal accountId)
        {
            unsubsribeFromApnPrivateMessages(findAppNetAccountById(accountId));
        }

        #endregion

        #endregion




        void AllSearches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (e.NewItems.Count != 0)
                {
                    foreach (Search search in e.NewItems)
                    {
                        if (subscribedSearches.Contains(search.Id))
                        {
                            foreach (TwitterItem item in search.Items)
                            {
                                SearchResults.Add(item);
                                addItem(item);
                            }
                        }
                    }
                }
            }

            if (e.OldItems != null)
            {
                if (e.OldItems.Count != 0)
                {
                    foreach (Search search in e.NewItems)
                    {
                        if (subscribedSearches.Contains(search.Id) && search.Id != 0)
                        {
                            IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                            ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                            foreach (IItem item in twitterIItems)
                            {
                                twitterItems.Add(item as TwitterItem);
                            }
                            IEnumerable<TwitterItem> obsoleteItems = twitterItems.Where(item => item.SourceSearchId == search.Id);
                            foreach (TwitterItem obsoleteItem in obsoleteItems)
                            {
                                Items.Remove(obsoleteItem);
                            }
                            obsoleteItems = SearchResults.Where(item => item.SourceSearchId == search.Id);
                            foreach (TwitterItem obsoleteItem in obsoleteItems)
                            {
                                SearchResults.Remove(obsoleteItem);
                            }
                            subscribedSearches.Remove(search.Id);
                        }
                    }
                }
            }
        }

        void AllLists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (e.NewItems.Count != 0)
                {
                    foreach (TweetList list in e.NewItems)
                    {
                        if (subscribedLists.Contains(list.Id))
                        {
                            foreach (TwitterItem item in list.Items)
                            {
                                ListResults.Add(item);
                                addItem(item);
                            }
                        }
                    }
                }
            }

            if (e.OldItems != null)
            {
                if (e.OldItems.Count != 0)
                {
                    foreach (TweetList list in e.NewItems)
                    {
                        if (subscribedLists.Contains(list.Id))
                        {
                            IEnumerable<IItem> twitterIItems = Items.Where(item => item.GetType() == typeof(TwitterItem));
                            ThreadSaveObservableCollection<TwitterItem> twitterItems = new ThreadSaveObservableCollection<TwitterItem>();
                            foreach (IItem item in twitterIItems)
                            {
                                twitterItems.Add(item as TwitterItem);
                            }
                            IEnumerable<TwitterItem> obsoleteItems = twitterItems.Where(item => item.SourceListId == list.Id);
                            foreach (TwitterItem obsoleteItem in obsoleteItems)
                            {
                                Items.Remove(obsoleteItem);
                            }
                            obsoleteItems = ListResults.Where(item => item.SourceListId == list.Id);
                            foreach (TwitterItem obsoleteItem in obsoleteItems)
                            {
                                ListResults.Remove(obsoleteItem);
                            }
                            subscribedLists.Remove(list.Id);
                        }
                    }
                }
            }
        }



        void Subscribed_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IItem item in e.OldItems)
                {
                    if (Items.Contains(item))
                    {
                        Items.Remove(item);
                    }
                }
            }
            if (e.NewItems != null)
            {
                if (e.NewItems.Count != 0)
                {
                    addItems(e.NewItems);
                }
            }
        }

        private void addItems(System.Collections.IList items)
        {
            foreach (IItem item in items)
            {
                addItem(item);
            }
        }

        private void addItems(List<TwitterItem> items)
        {
            foreach (IItem item in items)
            {
                addItem(item);
            }
        }

        private void addItems(ThreadSaveObservableCollection<TwitterItem> items)
        {
            foreach (IItem item in items)
            {
                addItem(item);
            }
        }

        private void addItem(IItem newItem)
        {
            AppController.Current.Logger.addDebugMessage("Adding item to View", this.Name, view: this, item:newItem, type: DebugMessage.DebugMessageTypes.View);
            if (newItem == null) {
                AppController.Current.Logger.addDebugMessage("Null item wanted to be added to View", this.Name, view: this, item: newItem, type: DebugMessage.DebugMessageTypes.View);
                return; 
            }
            if (!Items.Contains(newItem))
            {
                foreach (Filter filter in subscribedFilter)
                {
                    if (!filter.ShallItemBeDisplayed(newItem))
                    {
                        try
                        {
                            AppController.Current.Logger.addDebugMessage("Items has been filtered", filter.Name, view: this, item: newItem, type: DebugMessage.DebugMessageTypes.View);
                            FilteredItems.Add(newItem);
                        }
                        catch (Exception exp)
                        {
                            AppController.Current.Logger.writeToLogfile("Adding filtered item to collection failed");
                            AppController.Current.Logger.writeToLogfile(exp);
                            return;
                        }
                        return;
                    }
                }
                if (AllowDuplicates)
                {
                    try
                    {
                        Items.Add(newItem);
                        AppController.Current.newItemsHasBeenAddedToView(this, newItem);
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.Logger.writeToLogfile("Adding item to collection failed in allow duplicates");
                        AppController.Current.Logger.writeToLogfile(exp);
                        return;
                    }
                    AppController.Current.Logger.addDebugMessage("Checking if max number is reached - max is " + Properties.Settings.Default.MaxNumberOfItemsAtOnce.ToString(), Items.Count.ToString(), view: this, item: newItem, type: DebugMessage.DebugMessageTypes.View);
                    if (Items.Count > Properties.Settings.Default.MaxNumberOfItemsAtOnce && initializingTime.AddMinutes(2) < DateTime.Now)
                    {

                        while (Items.Count > Properties.Settings.Default.MaxNumberOfItemsAtOnce)
                        {
                            DateTime oldestItemTime = Items.Min(i => i.CreatedAt);
                            IItem oldestItem = Items.Where(i => i.CreatedAt == oldestItemTime).First() as IItem;
                            if (oldestItem != null)
                            {
                                try
                                {
                                    OverflowItems.Add(newItem);
                                }
                                catch (Exception exp)
                                {
                                    AppController.Current.Logger.writeToLogfile("Adding overflow item to collection failed");
                                    AppController.Current.Logger.writeToLogfile(exp);
                                    return;
                                }
                                try
                                {
                                    Items.Remove(oldestItem);
                                }
                                catch (Exception exp)
                                {
                                    AppController.Current.Logger.writeToLogfile("Removing item from collection failed in overflow handling");
                                    AppController.Current.Logger.writeToLogfile(exp);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        AppController.Current.Logger.addDebugMessage("Max number is reached - max is " + Properties.Settings.Default.MaxNumberOfItemsAtOnce.ToString(), Items.Count.ToString(), view: this, item: newItem, type: DebugMessage.DebugMessageTypes.View);
                    }
                }
                else
                {
                    try
                    {
                        if (Items.Where(i => i.Id == newItem.Id).Count() == 0)
                        {
                            try
                            {
                                Items.Add(newItem);
                                AppController.Current.newItemsHasBeenAddedToView(this, newItem);
                            }
                            catch (Exception exp)
                            {
                                AppController.Current.Logger.writeToLogfile("Adding item to collection failed");
                                AppController.Current.Logger.writeToLogfile(exp);
                                return;
                            }
                            if (Items.Count > Properties.Settings.Default.MaxNumberOfItemsAtOnce && initializingTime.AddMinutes(2) < DateTime.Now)
                            {
                                bool breakNow = false;
                                while (Items.Count > Properties.Settings.Default.MaxNumberOfItemsAtOnce && !breakNow)
                                {

                                    DateTime oldestItemTime = Items.Min(i => i.CreatedAt);
                                    IItem oldestItem = Items.Where(i => i.CreatedAt == oldestItemTime).First() as IItem;
                                    if (oldestItem != null)
                                    {
                                        try
                                        {
                                            OverflowItems.Add(newItem);
                                        }
                                        catch (Exception exp)
                                        {
                                            AppController.Current.Logger.writeToLogfile("Adding overflow item to collection failed");
                                            AppController.Current.Logger.writeToLogfile(exp);
                                            return;
                                        }
                                        try
                                        {
                                            Items.Remove(oldestItem);
                                        }
                                        catch (Exception exp)
                                        {
                                            AppController.Current.Logger.writeToLogfile("Removing item from collection failed in overflow handling");
                                            AppController.Current.Logger.writeToLogfile(exp);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        breakNow = true;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        try
                        {
                            Items.Add(newItem);
                        }
                        catch (Exception exp)
                        {
                            AppController.Current.Logger.addDebugMessage("Adding item to View failed with exception", exp.Message, view: this, item: newItem, type: DebugMessage.DebugMessageTypes.View);
                        }
                    }
                }
            }
            else
            {
                AppController.Current.Logger.addDebugMessage("Item already in View", this.Name, view: this, item: newItem, type: DebugMessage.DebugMessageTypes.View);
            }
        }

        private bool checkNewestItemDateTimeAndScrollPosition_ShouldBeScrolledToOlderOne(IItem item)
        {
            bool shouldScrollToSecondEntry = false;
            if (newestDateTimeIncluded != null)
            {
                if (item.CreatedAt > newestDateTimeIncluded)
                {
                    newestDateTimeIncluded = item.CreatedAt;
                }

                else
                {
                    newestDateTimeIncluded = item.CreatedAt;
                }
            }
            if (AppController.Current.CurrentView == this)
            {

            }
            return shouldScrollToSecondEntry;
        }

        public string getStorableSettings()
        {
            string delimiter = "|||";
            string storableString = this.Name;
            storableString += delimiter + string.Join("%%%", subscribedTimelines.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedMentions.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedRetweets.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedDirectMessages.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedLists.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedSearches.ToArray());

            storableString += delimiter;
            foreach (Filter filter in subscribedFilter)
            {
                storableString += filter.Id.ToString() + "%%%";
            }
            //storableString += "FACEBOOK START%%%";

            // Facebook
            storableString += delimiter + string.Join("%%%", subscribedFbStatusMessages.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedFbPhotos.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedFbVideos.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedFbLinks.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedFbCheckIns.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedFbEvents.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedFbNotes.ToArray());

            // GREasy
            storableString += delimiter + string.Join("%%%", subscribedGoogleReaderEasyUnreadItems.ToArray());

            // Twitter favorites - I do hate this storage format...
            storableString += delimiter + string.Join("%%%", subscribedFavorites.ToArray());

            // QUOTE.fm
            storableString += delimiter + string.Join("%%%", subscribedQuoteFmRecommendations.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedQuoteFmCategories.ToArray());

            // App.net
            storableString += delimiter + string.Join("%%%", subscribedApnPersonalStreams.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedApnMentions.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedApnReposts.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedApnStars.ToArray());
            storableString += delimiter + string.Join("%%%", subscribedApnPrivateMessages.ToArray());

            // Twitter only View or not
            if (isTwitterOnlyView)
            {
                storableString += delimiter + "1";
            }
            else
            {
                storableString += delimiter + "0";
            }

            storableString.TrimEnd('%');
            return storableString;
        }

        public void readStorableSettings(string storedSettingsString)
        {
            try
            {
                string[] delimiter = { "|||" };
                string[] storedViews = storedSettingsString.Split(delimiter, StringSplitOptions.None);
                string[] innerDelimiter = { "%%%" };
                if (storedViews.Length < 7)
                {
                    return;
                }
                Name = storedViews[0];

                AppController.Current.Logger.addDebugMessage("Started loading of stored View", Name, type: DebugMessage.DebugMessageTypes.View);

                if (storedViews.Length > 7)
                {
                    string[] filterIds = storedViews[7].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string filterString in filterIds)
                    {
                        if (filterString.StartsWith("FACEBOOK")) { continue; }
                        decimal filterId = Convert.ToDecimal(filterString);
                        AppController.Current.Logger.addDebugMessage("Reading stored filter id", filterString, view: this, type: DebugMessage.DebugMessageTypes.View);
                        Filter filter = AppController.Current.AllFilters.Where(f => f.Id == filterId).First();
                        if (filter != null)
                        {
                            AppController.Current.Logger.addDebugMessage("Adding filter to View", filterString, view: this, type: DebugMessage.DebugMessageTypes.View);
                            subscribedFilter.Add(filter);
                        }

                    }
                }

                string[] timelines = storedViews[1].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string timeline in timelines)
                {
                    AppController.Current.Logger.addDebugMessage("Adding timeline to View", timeline, view: this, type: DebugMessage.DebugMessageTypes.View);
                    subscribeToTimeline(Convert.ToDecimal(timeline));
                }

                string[] mentions = storedViews[2].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string mention in mentions)
                {
                    AppController.Current.Logger.addDebugMessage("Adding mentions to View", mention, view: this, type: DebugMessage.DebugMessageTypes.View);
                    subscribeToMentions(Convert.ToDecimal(mention));
                }

                string[] retweets = storedViews[3].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string retweet in retweets)
                {
                    AppController.Current.Logger.addDebugMessage("Adding retweets to View", retweet, view: this, type: DebugMessage.DebugMessageTypes.View);
                    subscribeToRetweets(Convert.ToDecimal(retweet));
                }

                string[] directMessages = storedViews[4].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string directMessage in directMessages)
                {
                    AppController.Current.Logger.addDebugMessage("Adding dms to View", directMessage, view: this, type: DebugMessage.DebugMessageTypes.View);
                    subscribeToDirectMessages(Convert.ToDecimal(directMessage));
                }

                string[] lists = storedViews[5].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string list in lists)
                {
                    AppController.Current.Logger.addDebugMessage("Adding list to View", list, view: this, type: DebugMessage.DebugMessageTypes.View);
                    subscribeToList(Convert.ToDecimal(list));
                }

                string[] searches = storedViews[6].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string search in searches)
                {
                    AppController.Current.Logger.addDebugMessage("Adding search to View", search, view: this, type: DebugMessage.DebugMessageTypes.View);
                    subscribeToSearch(Convert.ToDecimal(search));
                }

                if (storedViews.Length >= 15)
                {
                    string[] fbStatusMessages = storedViews[8].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string status in fbStatusMessages)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb statuses to View", status, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbStatusMessages(Convert.ToDecimal(status));
                    }

                    string[] fbPhotos = storedViews[9].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string photos in fbPhotos)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb photos to View", photos, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbPhotos(Convert.ToDecimal(photos));
                    }

                    string[] fbVideos = storedViews[10].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string videos in fbVideos)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb videos to View", videos, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbVideos(Convert.ToDecimal(videos));
                    }

                    string[] fbLinks = storedViews[11].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string links in fbLinks)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb links to View", links, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbLinks(Convert.ToDecimal(links));
                    }

                    string[] fbCheckIns = storedViews[12].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string checkIns in fbCheckIns)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb checkins to View", checkIns, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbCheckIns(Convert.ToDecimal(checkIns));
                    }

                    string[] fbEvents = storedViews[13].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string events in fbEvents)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb events to View", events, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbEvents(Convert.ToDecimal(events));
                    }

                    string[] fbNotes = storedViews[14].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string notes in fbNotes)
                    {
                        AppController.Current.Logger.addDebugMessage("Adding fb notes to View", notes, view: this, type: DebugMessage.DebugMessageTypes.View);
                        subscribeToFbNotes(Convert.ToDecimal(notes));
                    }

                    if (storedViews.Length >= 18)
                    {
                        string[] favorites = storedViews[16].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string favorite in favorites)
                        {
                            AppController.Current.Logger.addDebugMessage("Adding favorites to View", favorite, view: this, type: DebugMessage.DebugMessageTypes.View);
                            subscribeToFavorites(Convert.ToDecimal(favorite));
                        }

                        string[] qfmRecos = storedViews[17].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string qfmReco in qfmRecos)
                        {
                            AppController.Current.Logger.addDebugMessage("Adding Quote.FM recos to View", qfmReco, view: this, type: DebugMessage.DebugMessageTypes.View);
                            subscribeToQuoteFmRecommendations(Convert.ToDecimal(qfmReco));
                        }
                    }

                    if (storedViews.Length >= 19)
                    {
                        string[] qfmCats = storedViews[18].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string qfmCat in qfmCats)
                        {
                            AppController.Current.Logger.addDebugMessage("Adding Quote.FM category to View", qfmCat, view: this, type: DebugMessage.DebugMessageTypes.View);
                            subscribeToQuoteFmCategory(Convert.ToDecimal(qfmCat));
                        }
                    }

                    if (storedViews.Length >= 20)
                    {
                        string[] apnPersonalStreams = storedViews[19].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string apnPersonalStream in apnPersonalStreams)
                        {
                            AppController.Current.Logger.addDebugMessage("Adding APN Personal stream to View", apnPersonalStream, view: this, type: DebugMessage.DebugMessageTypes.View);
                            subscribeToApnPersonalStream(Convert.ToDecimal(apnPersonalStream));
                        }

                        // migration issue
                        if (storedViews[20] != "Mentions")
                        {
                            string[] apnMentions = storedViews[20].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string apnMention in apnMentions)
                            {
                                AppController.Current.Logger.addDebugMessage("Adding APN Mentions to View", apnMention, view: this, type: DebugMessage.DebugMessageTypes.View);
                                subscribeToApnMentions(Convert.ToDecimal(apnMention));
                            }
                        }

                        // migration issue
                        try
                        {
                            string[] apnMessages = storedViews[23].Split(innerDelimiter, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string apnMessage in apnMessages)
                            {
                                AppController.Current.Logger.addDebugMessage("Adding APN Messages to View", apnMessage, view: this, type: DebugMessage.DebugMessageTypes.View);
                                subscribeToApnPrivateMessages(Convert.ToDecimal(apnMessage));
                            }

                        }
                        catch { }
                        try
                        {
                            string isTwitterView = storedViews[24];
                            if (isTwitterView == "1")
                            {
                                isTwitterOnlyView = true;
                            }
                            else
                            {
                                isTwitterOnlyView = false;
                            }

                        }
                        catch {
                            if (this.subscribedTimelines.Count() > 0 ||
                                this.subscribedSearches.Count() > 0 ||
                                this.subscribedRetweets.Count() > 0 ||
                                this.subscribedMentions.Count() > 0 ||
                                this.subscribedLists.Count() > 0 ||
                                this.subscribedDirectMessages.Count() > 0)
                            {
                                this.isTwitterOnlyView = true;
                            }
                            else
                            {
                                this.isTwitterOnlyView = false;
                            }
                        }
                    }
                }
                else
                {
                    AppController.Current.Logger.writeToLogfile("Incomplete Facebook view setting - maybe first start after upgrade to 1.2");
                }

            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("View load error: " + exp.Message);
                AppController.Current.Logger.writeToLogfile(exp);
                this.Name = "ERROR";
            }
        }

        public void DeleteTweetFromEverywhere(TwitterItem item, List<TwitterItem> KnownInstances)
        {
            try
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
                                try
                                {
                                    KnownInstances.Add(toBeDeletedItem);
                                }
                                catch { }
                            }
                        }
                        catch { }

                    }
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile(exp, true);
            }
        }

        #region TweetMarker

        public void GetPositionMarkerItem()
        {
            if (!AppController.Current.AllAccountsHaveInitialFetchDone) { return; }
            AppController.Current.Logger.addDebugMessage("Start getting TweetMarkerItem", "All initial Twitter fetches are complete", view: this, type: DebugMessage.DebugMessageTypes.TweetMarker);
            if (!backgroundWorkerGetPositionMarker.IsBusy)
            {
                backgroundWorkerGetPositionMarker.RunWorkerAsync();
            }
        }

        public void saveToTweetMarker(DateTime? NewestItemDateCurrentlyDisplayed)
        {
            if (!backgroundWorkerStorePositionMarker.IsBusy)
            {
                backgroundWorkerStorePositionMarker.RunWorkerAsync(NewestItemDateCurrentlyDisplayed);
            }
        }
        private decimal saveOneTypeToTweetMarker(string type, AccountTwitter account, ThreadSaveObservableCollection<IItem> ItemsCollection, DateTime? NewestItemDateCurrentlyDisplayed)
        {
            if (!Properties.Settings.Default.UseTweetmarker)
            {
                return 0;
            }
            if (NewestItemDateCurrentlyDisplayed == null) { return 0; }
            IEnumerable<IItem> items = ItemsCollection.Where(i => i.CreatedAt < NewestItemDateCurrentlyDisplayed.Value.AddSeconds(2));
            if (items.Count() > 0)
            {
                DateTime NewestItemOlderThanMaxDate = items.Max(i => i.CreatedAt);
                TwitterItem item = items.Where(i => i.CreatedAt == NewestItemOlderThanMaxDate).First() as TwitterItem;
                if (item.CreatedAt == NewestItemOlderThanMaxDate)
                {
                    List<TwitterItem> twitterItems = new List<TwitterItem>();
                    foreach(TwitterItem twitterItem in ItemsCollection) {
                        if(twitterItem != null) {
                            if(twitterItem.IsTweetMarker) {
                                twitterItem.IsTweetMarker = false;
                            }
                        }
                    }
                    
                    item.IsTweetMarker = true;
                    API.Tweetmarker.storeTweetMark(account, type, item.Id);
                    return item.Id;
                }
               
            }
            return 0;
        }
        private decimal saveOneTypeToTweetMarker(string type, AccountTwitter account, ThreadSaveObservableCollection<TwitterItem> ItemsCollection, DateTime? NewestItemDateCurrentlyDisplayed)
        {
            ThreadSaveObservableCollection<IItem> Items = new ThreadSaveObservableCollection<IItem>();
            foreach (TwitterItem item in ItemsCollection)
            {
                Items.Add(item);
            }

            return saveOneTypeToTweetMarker(type, account, Items, NewestItemDateCurrentlyDisplayed);
        }

        void backgroundWorkerGetPositionMarker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e != null) {
                if(e.ProgressPercentage == 100) {
                    IItem positionMarkerItem = e.UserState as IItem;
                    if(positionMarkerItem != null) {
                        // hhh AppController.Current.sendNotification("General", "Updated position marker read", positionMarkerItem.Text, "", positionMarkerItem);
                        AppController.Current.scrollToItem(positionMarkerItem,true);
                    }
                }
            }
        }

        void backgroundWorkerGetPositionMarker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        void backgroundWorkerGetPositionMarker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<IItem> positionMarkerItems = new List<IItem>();

            try
            {

                #region Twitter TweetMarker

                AppController.Current.Logger.addDebugMessage("Start getting TweetMarkerItem", "All initial Twitter fetches are complete", view: this, type: DebugMessage.DebugMessageTypes.TweetMarker);

                // Timelines
                foreach (decimal timelineId in subscribedTimelines)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(timelineId);
                    if (account != null)
                    {
                        if (account.tweetMarkerTimeline == 0) { continue; }
                        IEnumerable<IItem> timelineItems = account.Timeline.Where(i => i.Id == account.tweetMarkerTimeline);
                        if (timelineItems.Count() > 0)
                        {
                            TwitterItem timelineItem = timelineItems.First() as TwitterItem;
                            if (timelineItem != null)
                            {
                                if (timelineItem.Id < account.Timeline.Max(i => i.Id))
                                {
                                    // this should be possible with a short LINQ statement, shouldn't it...?
                                    decimal idOfOldestUnreadItem = account.Timeline.Where(item => item.Id > timelineItem.Id).Min(item => item.Id);
                                    IItem oldestUnreadItem = account.Timeline.Where(item => item.Id == idOfOldestUnreadItem).First();
                                    if (oldestUnreadItem != null)
                                    {
                                        positionMarkerItems.Add(oldestUnreadItem as TwitterItem);
                                        AppController.Current.Logger.addDebugMessage("Found TweetMarker in this Views", "Timeline", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: timelineItem);
                                    }
                                }
                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found TweetMarker is anyway the newest one known", "Timeline", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: timelineItem);
                                }
                            }
                        }
                    }
                }

                // Mentions
                foreach (decimal mentionId in subscribedMentions)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(mentionId);
                    if (account != null)
                    {
                        if (account.tweetMarkerMentions == 0) { continue; }
                        IEnumerable<IItem> items = account.Mentions.Where(i => i.Id == account.tweetMarkerMentions);
                        if (items.Count() > 0)
                        {
                            TwitterItem item = items.First() as TwitterItem;
                            if (item != null)
                            {
                                if (item.Id < account.Mentions.Max(i => i.Id))
                                {
                                    decimal idOfOldestUnreadItem = account.Timeline.Where(i => i.Id > item.Id).Min(i => i.Id);
                                    IItem oldestUnreadItem = account.Timeline.Where(i => i.Id == idOfOldestUnreadItem).First();
                                    if (oldestUnreadItem != null)
                                    {
                                        positionMarkerItems.Add(oldestUnreadItem as TwitterItem);
                                        AppController.Current.Logger.addDebugMessage("Found TweetMarker in this Views", "Mentions", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                    }
                                }
                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found TweetMarker is anyway the newest one known", "Mentions", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                            }
                        }
                    }
                }

                // DMs
                foreach (decimal dmId in subscribedDirectMessages)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(dmId);
                    if (account != null)
                    {
                        if (account.tweetMarkerDMs == 0) { continue; }
                        IEnumerable<IItem> items = account.DirectMessages.Where(i => i.Id == account.tweetMarkerDMs);
                        if (items.Count() > 0)
                        {
                            TwitterItem item = items.First() as TwitterItem;
                            if (item != null)
                            {
                                if (item.Id < account.DirectMessages.Max(i => i.Id))
                                {
                                    decimal idOfOldestUnreadItem = account.Timeline.Where(i => i.Id > item.Id).Min(i => i.Id);
                                    IItem oldestUnreadItem = account.Timeline.Where(i => i.Id == idOfOldestUnreadItem).First();
                                    if (oldestUnreadItem != null)
                                    {
                                        positionMarkerItems.Add(oldestUnreadItem as TwitterItem);
                                        AppController.Current.Logger.addDebugMessage("Found TweetMarker in this DMs", "Mentions", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                    }
                                }

                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found TweetMarker is anyway the newest one known", "DMs", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                            }
                        }
                    }
                }

                // Favorites
                foreach (decimal favId in subscribedFavorites)
                {
                    AccountTwitter account = AppController.Current.getAccountForId(favId);
                    if (account != null)
                    {
                        if (account.tweetMarkerFavorites == 0) { continue; }
                        IEnumerable<IItem> items = account.Favorites.Where(i => i.Id == account.tweetMarkerFavorites);
                        if (items.Count() > 0)
                        {
                            TwitterItem item = items.First() as TwitterItem;
                            if (item != null)
                            {
                                if (item.Id < account.Favorites.Max(i => i.Id))
                                {
                                    decimal idOfOldestUnreadItem = account.Timeline.Where(i => i.Id > item.Id).Min(i => i.Id);
                                    IItem oldestUnreadItem = account.Timeline.Where(i => i.Id == idOfOldestUnreadItem).First();
                                    if (oldestUnreadItem != null)
                                    {
                                        positionMarkerItems.Add(oldestUnreadItem as TwitterItem);
                                        AppController.Current.Logger.addDebugMessage("Found TweetMarker in this Views", "Favs", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                    }
                                }
                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found TweetMarker is anyway the newest one known", "Favs", account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                            }
                        }
                    }
                }

                // lists
                foreach (decimal listId in subscribedLists)
                {

                    TweetList list = AppController.Current.getListForId(listId);
                    if (list != null)
                    {
                        if (list.tweetMarkerLastKnown == 0) { continue; }
                        IEnumerable<IItem> items = list.Items.Where(i => i.Id == list.tweetMarkerLastKnown);
                        if (items.Count() > 0)
                        {
                            TwitterItem item = items.First() as TwitterItem;
                            if (item != null)
                            {
                                if (item.Id < list.Items.Max(i => i.Id))
                                {
                                    decimal idOfOldestUnreadItem = list.Items.Where(i => i.Id > item.Id).Min(i => i.Id);
                                    IItem oldestUnreadItem = list.Items.Where(i => i.Id == idOfOldestUnreadItem).First();
                                    if (oldestUnreadItem != null)
                                    {
                                        positionMarkerItems.Add(oldestUnreadItem as TwitterItem);
                                        AppController.Current.Logger.addDebugMessage("Found TweetMarker in this Views", "List", view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                    }
                                }
                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found TweetMarker is anyway the newest one known", "List: " + list.name, account: list.Account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                            }
                        }
                    }
                }

                // searches
                foreach (decimal seaarchId in subscribedSearches)
                {

                    Search search = AppController.Current.getSearchForId(seaarchId);
                    if (search != null)
                    {
                        if (search.tweetMarkerLastKnown == 0) { continue; }
                        IEnumerable<IItem> items = search.Items.Where(i => i.Id == search.tweetMarkerLastKnown);
                        if (items.Count() > 0)
                        {
                            TwitterItem item = items.First() as TwitterItem;
                            if (item != null)
                            {
                                if (item.Id < search.Items.Max(i => i.Id))
                                {
                                    decimal idOfOldestUnreadItem = search.Items.Where(i => i.Id > item.Id).Min(i => i.Id);
                                    IItem oldestUnreadItem = search.Items.Where(i => i.Id == idOfOldestUnreadItem).First();
                                    if (oldestUnreadItem != null)
                                    {
                                        positionMarkerItems.Add(oldestUnreadItem as TwitterItem);
                                        AppController.Current.Logger.addDebugMessage("Found TweetMarker in this Views", "Search", view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                    }
                                }
                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found TweetMarker is anyway the newest one known", "Search: " + search.name, account: search.Account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                            }
                        }
                    }
                }

            
                #endregion

                #region App.net Stream Markers
                // App.net my stream
                foreach (decimal personalStream in subscribedApnPersonalStreams)
                {

                    AccountAppDotNet account = findAppNetAccountById(personalStream);
                    if (account != null)
                    {
                        if (string.IsNullOrEmpty(account.storeMarkerIdMyStream)) { continue; }
                        IEnumerable<IItem> items = account.PersonalStream.Where(i => i.Id.ToString() == account.storeMarkerIdMyStream);
                        if (items.Count() > 0)
                        {
                            ApnItem item = items.First() as ApnItem;
                            if (item != null)
                            {
                                if (item.Id < account.PersonalStream.Max(i => i.Id))
                                {
                                    positionMarkerItems.Add(item);
                                    AppController.Current.Logger.addDebugMessage("Found Stram Marker in this App.net personal stream", "Personal stream", view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                                else
                                {
                                    AppController.Current.Logger.addDebugMessage("Found Stream Marker is anyway the newest one known", "Personal stream: " + account.username, account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                                }
                            }
                        }
                    }
                }

                // App.net mentions
                foreach (decimal mentions in subscribedApnMentions)
                {

                    AccountAppDotNet account = findAppNetAccountById(mentions);
                    if (account != null)
                    {
                        if (string.IsNullOrEmpty(account.storeMarkerIdMyStream)) { continue; }
                        IEnumerable<IItem> items = account.Mentions.Where(i => i.Id.ToString() == account.storeMarkerIdMentions);
                        if (items.Count() > 0)
                        {
                            ApnItem item = items.First() as ApnItem;
                            if (item != null)
                            {
                                if (item.Id < account.Mentions.Max(i => i.Id))
                                {
                                positionMarkerItems.Add(item);
                                AppController.Current.Logger.addDebugMessage("Found Stram Marker in this App.net mentions", "Mentions", view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                            }
                            else
                            {
                                AppController.Current.Logger.addDebugMessage("Found Stream Marker is anyway the newest one known", "Personal stream: " + account.username, account: account, view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: item);
                            }
                            
                            }
                        }
                    }
                }

                #endregion

                if (positionMarkerItems.Count() == 0) { backgroundWorkerGetPositionMarker.ReportProgress(100, null); }
                //uuu min durch max ersetzt
                DateTime minDateTime = positionMarkerItems.Min(i => i.CreatedAt);
                AppController.Current.Logger.addDebugMessage("Minimum datetime in position marker", minDateTime.ToLongTimeString(), view: this, type: DebugMessage.DebugMessageTypes.TweetMarker);
                IItem minItem = positionMarkerItems.Where(i => i.CreatedAt == minDateTime).First();
                AppController.Current.Logger.addDebugMessage("Oldest relevant item found", minItem.CreatedAt.ToLongTimeString(), view: this, type: DebugMessage.DebugMessageTypes.TweetMarker, item: minItem);
                backgroundWorkerGetPositionMarker.ReportProgress(100, minItem);
            }
            catch { }
        }

        #endregion

        #region App.net stream marker

        private decimal saveOneTypeToStreamMarker(string type, AccountAppDotNet account, ThreadSaveObservableCollection<IItem> ItemsCollection, DateTime? NewestItemDateCurrentlyDisplayed)
        {
            if (NewestItemDateCurrentlyDisplayed == null) { return 0; }
            IEnumerable<IItem> items = ItemsCollection.Where(i => i.CreatedAt < NewestItemDateCurrentlyDisplayed.Value.AddSeconds(2));
            if (items.Count() > 0)
            {
                DateTime NewestItemOlderThanMaxDate = items.Max(i => i.CreatedAt);
                ApnItem item = items.Where(i => i.CreatedAt == NewestItemOlderThanMaxDate).First() as ApnItem;
                if (item.CreatedAt == NewestItemOlderThanMaxDate)
                {
                    List<ApnItem> apnItems = new List<ApnItem>();
                    foreach (ApnItem apnItem in ItemsCollection)
                    {
                        if (apnItem != null)
                        {
                            if (apnItem.isStreamMarker)
                            {
                                apnItem.isStreamMarker = false;
                            }
                        }
                    }

                    
                    item.isStreamMarker = true;
                    if (type == "my_stream")
                    {
                        account.streamMarkerMyStreamOwnUpdate = true;
                    }
                    else if (type == "mentions")
                    {
                        account.streamMarkerMentionsOwnUpdate = true;
                    }
                    AppNetDotNet.ApiCalls.StreamMarkers.set(account.accessToken, type, item.Id.ToString(), 100);
                    return item.Id;
                }

            }
            return 0;
        }

        private decimal saveOneTypeToStreamMarker(string type, AccountAppDotNet account, ThreadSaveObservableCollection<ApnItem> ItemsCollection, DateTime? NewestItemDateCurrentlyDisplayed)
        {
            ThreadSaveObservableCollection<IItem> Items = new ThreadSaveObservableCollection<IItem>();
            foreach (ApnItem item in ItemsCollection)
            {
                Items.Add(item);
            }

            return saveOneTypeToStreamMarker(type, account, Items, NewestItemDateCurrentlyDisplayed);
        }

        public void liveUpdateScrollPositionAsStreamMarkerChanged()
        {
            List<ApnItem> updatedItems = new List<ApnItem>();

            List<AccountAppDotNet> mentionsAccounts = new List<AccountAppDotNet>();
            List<AccountAppDotNet> myStreamAccounts = new List<AccountAppDotNet>();

            foreach (decimal myStream in subscribedApnPersonalStreams)
            {

                AccountAppDotNet account = findAppNetAccountById(myStream);
                if (account != null)
                {
                    myStreamAccounts.Add(account);
                }
            }

            foreach (decimal mentions in subscribedApnMentions)
            {

                AccountAppDotNet account = findAppNetAccountById(mentions);
                if (account != null)
                {
                    mentionsAccounts.Add(account);
                    if (account.mentionItemWithUpdatedStreamMarker != null)
                    {
                        updatedItems.Add(account.mentionItemWithUpdatedStreamMarker);
                    }
                }
            }

            if(myStreamAccounts.TrueForAll(a => a.lastFetechCompleted) && mentionsAccounts.TrueForAll(b => b.lastFetechCompleted)) {
                foreach (AccountAppDotNet account in myStreamAccounts)
                {
                    if (account.myStreamItemWithUpdatedStreamMarker != null)
                    {
                        updatedItems.Add(account.myStreamItemWithUpdatedStreamMarker);
                        account.myStreamItemWithUpdatedStreamMarker = null;
                    }
                }
                foreach (AccountAppDotNet account in mentionsAccounts)
                {
                    if (account.mentionItemWithUpdatedStreamMarker != null)
                    {
                        updatedItems.Add(account.mentionItemWithUpdatedStreamMarker);
                        account.mentionItemWithUpdatedStreamMarker = null;
                    }
                }
            }
            if (updatedItems.Count > 0)
            {
                ApnItem oldestUpdatedItem = updatedItems.Where(i => i.Id == (updatedItems.Min(m => m.Id))).First();
                //AppController.Current.sendNotification("General", "Updated stream marker position found", oldestUpdatedItem.AuthorName + "\n\n" + oldestUpdatedItem.Text, oldestUpdatedItem.Avatar, oldestUpdatedItem);
                AppController.Current.scrollToItem(oldestUpdatedItem, true);
            }
        }

        #endregion

        #region containing account types

        public bool hasTwitterContent
        {
            get
            {
                return isTwitterOnlyView;
            }
        }

        public bool hasApnContents
        {
            get
            {
                return (subscribedApnMentions.Count() > 0 || subscribedApnPersonalStreams.Count() > 0 || subscribedApnPrivateMessages.Count() > 0 || subscribedApnReposts.Count() > 0 || subscribedApnStars.Count() > 0); 
            }
        }

        public bool hasFacebookContents
        {
            get
            {
                return (subscribedFbCheckIns.Count() > 0 || subscribedFbCheckIns.Count() > 0 || subscribedFbEvents.Count() > 0 || subscribedFbLinks.Count() > 0 || subscribedFbNotes.Count() > 0 || subscribedFbPhotos.Count() > 0 || subscribedFbStatusMessages.Count() > 0 || subscribedFbVideos.Count() > 0);
            }
        }

        public bool hasQfmContents
        {
            get
            {
                return (subscribedQuoteFmCategories.Count() > 0 || subscribedQuoteFmRecommendations.Count() > 0);
            }
        }

        #endregion 
    }
}
