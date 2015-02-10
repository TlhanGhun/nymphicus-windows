using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus;
using Nymphicus.Model;
using TweetSharp;
using System.ComponentModel;
using System.Windows.Threading;
using System.Drawing;
using System.Net;
using System.IO;

namespace Nymphicus.API
{
    public class Functions
    {
        static public List<TwitterItem> getTimeline(TwitterService service, AccountTwitter account, DoWorkEventArgs e)
        {
            return getTimeline(service, account, e, -1);
        }

        static public List<TwitterItem> getTimeline(TwitterService service, AccountTwitter account, DoWorkEventArgs e, decimal minimumId)
        {
           IEnumerable<TwitterStatus> tweets;
           List<TwitterItem> allTweets = new List<TwitterItem>();
           try
           {
               ListTweetsOnHomeTimelineOptions options = new TweetSharp.ListTweetsOnHomeTimelineOptions();
               options.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
               
               if (minimumId >= 0)
               {
                   options.SinceId = Convert.ToInt64(minimumId);
               }
               IAsyncResult result = service.BeginListTweetsOnHomeTimeline(options);
               tweets = service.EndListTweetsOnHomeTimeline(result);

               foreach (TwitterStatus status in tweets)
               {
                   if (e != null)
                    {
                        if (e.Cancel) {
                            AppController.Current.Logger.writeToLogfile("Cancel received for timeline");
                            break;
                        }
                    }
                   allTweets.Add(API.TweetSharpConverter.getItemFromStatus(status, account));
               }
           }
           catch (Exception exp)
           {
               AppController.Current.sendNotification("ERROR", exp.Message, exp.StackTrace,"",null);
           }
           return allTweets;
        }

        static public List<TwitterItem> getRetweets(TwitterService service, AccountTwitter account, DoWorkEventArgs e)
        {
            return getRetweets(service, account, e, -1);
        }

        static public List<TwitterItem> getRetweets(TwitterService service, AccountTwitter account, DoWorkEventArgs e, decimal minimumId)
        {
            IEnumerable<TwitterStatus> retweets;
            List<TwitterItem> allTweets = new List<TwitterItem>();
            try
            {
                ListRetweetsOfMyTweetsOptions options = new TweetSharp.ListRetweetsOfMyTweetsOptions();
                options.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
                if (minimumId >= 0)
                {
                    options.SinceId = Convert.ToInt64(minimumId);
                }

                IAsyncResult result = service.BeginListRetweetsOfMyTweets(options);
                retweets = service.EndListRetweetsOfMyTweets(result);

                foreach (TwitterStatus status in retweets)
                {
                    if (e != null)
                    {
                        if (e.Cancel) {
                            AppController.Current.Logger.writeToLogfile("Cancel received for retweets");
                            break;
                        }
                    }
                    allTweets.Add(API.TweetSharpConverter.getItemFromStatus(status, account));
                    foreach (TwitterItem item in allTweets)
                    {
                        item.isRetweetedToMe = true;
                    }
                }
            }
            catch
            {
                // xxx
            }
            return allTweets;
        }

        static public List<TwitterItem> getMentions(TwitterService service, AccountTwitter account, DoWorkEventArgs e)
        {
            return getMentions(service, account, e, -1);
        }

        static public List<TwitterItem> getMentions(TwitterService service, AccountTwitter account, DoWorkEventArgs e, decimal minimumId)
        {
            IEnumerable<TwitterStatus> mentions;
            List<TwitterItem> allTweets = new List<TwitterItem>();
            try
            {
                ListTweetsMentioningMeOptions options = new TweetSharp.ListTweetsMentioningMeOptions();
                options.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
                if (minimumId >= 0)
                {
             
                    options.SinceId = Convert.ToInt64(minimumId);
                    
                }

                IAsyncResult result = service.BeginListTweetsMentioningMe(options);
                mentions = service.EndListTweetsMentioningMe(result);


                foreach (TwitterStatus status in mentions)
                {
                    if (e != null)
                    {
                        if (e.Cancel) {
                            AppController.Current.Logger.writeToLogfile("Cancel received for Mentions");
                            break;
                        }
                    }
                    allTweets.Add(API.TweetSharpConverter.getItemFromStatus(status, account));
                    foreach (TwitterItem item in allTweets)
                    {
                        item.isMention = true;
                    }
                }
            }
            catch
            {
                // xxx
            }
            return allTweets;
        }

        static public List<TwitterItem> getDirectMessages(TwitterService service, AccountTwitter account, DoWorkEventArgs e)
        {
            return getDirectMessages(service, account, e, -1);
        }

        static public List<TwitterItem> getDirectMessages(TwitterService service, AccountTwitter account, DoWorkEventArgs e, decimal minimumId)
        {
            IEnumerable<TwitterDirectMessage> directMessages;
            List<TwitterItem> allTweets = new List<TwitterItem>();
            try
            {
                ListDirectMessagesReceivedOptions options = new TweetSharp.ListDirectMessagesReceivedOptions();
                
                options.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
               // options.IncludeEntities = true;
               
                if (minimumId >= 0)
                {
                    options.SinceId = Convert.ToInt64(minimumId);
                  
                }

                IAsyncResult result = service.BeginListDirectMessagesReceived(options);
                directMessages = service.EndListDirectMessagesReceived(result);

                foreach (TwitterDirectMessage directMessage in directMessages)
                {
                    if (e != null)
                    {
                        if (e.Cancel) {
                            AppController.Current.Logger.writeToLogfile("Cancel received for direct messages");
                            break;
                        }
                    }
                    allTweets.Add(API.TweetSharpConverter.getItemFromDirectMessage(directMessage, account));
                 
                }

           
            }
            catch
            {
                // xxx
            }
            return allTweets;
        }

        static public List<TwitterItem> getSentDirectMessages(TwitterService service, AccountTwitter account, DoWorkEventArgs e)
        {
            return getSentDirectMessages(service, account, e, -1);
        }

        static public List<TwitterItem> getSentDirectMessages(TwitterService service, AccountTwitter account, DoWorkEventArgs e, decimal minimumId)
        {
            IEnumerable<TwitterDirectMessage> directMessages;
            List<TwitterItem> allTweets = new List<TwitterItem>();
            try
            {
                
                ListDirectMessagesSentOptions optionsSent = new TweetSharp.ListDirectMessagesSentOptions();
               
                optionsSent.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
                //optionsSent.IncludeEntities = true;
                if (minimumId >= 0)
                {

                    optionsSent.SinceId = Convert.ToInt64(minimumId);
                }

            

                IAsyncResult result = service.BeginListDirectMessagesSent(optionsSent);
                directMessages = service.EndListDirectMessagesSent(result);

                foreach (TwitterDirectMessage directMessage in directMessages)
                {
                    if (e != null)
                    {
                        if (e.Cancel) {
                            AppController.Current.Logger.writeToLogfile("Cancel received for direct messages");
                            break;
                        }
                    }
                    allTweets.Add(API.TweetSharpConverter.getItemFromDirectMessage(directMessage, account));
                 
                }
            }
            catch
            {
                // xxx
            }
            return allTweets;
        }

        static public List<TwitterItem> getFavorites(TwitterService service, AccountTwitter account, DoWorkEventArgs e)
        {
            return getFavorites(service, account, e, -1);
        }

        static public List<TwitterItem> getFavorites(TwitterService service, AccountTwitter account, DoWorkEventArgs e, decimal minimumId)
        {
            IEnumerable<TwitterStatus> favorites;
            List<TwitterItem> allTweets = new List<TwitterItem>();
            try
            {
                ListFavoriteTweetsOptions options = new TweetSharp.ListFavoriteTweetsOptions();
                options.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
                if (minimumId >= 0)
                {

                    options.SinceId = Convert.ToInt64(minimumId);

                }

                IAsyncResult result = service.BeginListFavoriteTweets(options);
                favorites = service.EndListTweetsMentioningMe(result);


                foreach (TwitterStatus status in favorites)
                {
                    if (e != null)
                    {
                        if (e.Cancel)
                        {
                            AppController.Current.Logger.writeToLogfile("Cancel received for Mentions");
                            break;
                        }
                    }
                    allTweets.Add(API.TweetSharpConverter.getItemFromStatus(status, account));
                    foreach (TwitterItem item in allTweets)
                    {
                        item.isMention = true;
                    }
                }
            }
            catch
            {
                // xxx
            }
            return allTweets;
        }



        // ------------------------------

        public static TwitterItem retweetItem(AccountTwitter account, TwitterItem item)
        {
            try
            {
                RetweetOptions options = new TweetSharp.RetweetOptions();
                if (item.RetweetedItem != null)
                {
                    options.Id = Convert.ToInt64(item.RetweetedItem.Id);
                }
                else
                {
                    options.Id = Convert.ToInt64(item.Id);
                }


                TwitterStatus retweet = account.twitterService.Retweet(options);

                if (retweet != null)
                {
                    item.isRetweetedByMe = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("Retweet failed", "Unknown error");
                }
                return item;
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show("Retweet failed", exp.Message);
                return null;
            }
        }



        static Person getPerson(string login, AccountTwitter account)
        {
            return API.TweetSharpConverter.getPersonFromLogin(login, account);
        }

        public static List<TwitterItem> getItemsForUsername(TwitterService service, string username, AccountTwitter account)
        {
            return executeSearch(service, "From:" + username,null, account);
        }

        public static bool followUser(AccountTwitter account, string username)
        {
            FollowUserOptions options = new TweetSharp.FollowUserOptions();
            options.ScreenName = username;
            options.Follow = true;
            TwitterUser user = account.twitterService.FollowUser(options);
            
            if (user != null)
            {
                
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool unfollowUser(AccountTwitter account, string username)
        {
            FollowUserOptions options = new TweetSharp.FollowUserOptions();
            options.ScreenName = username;
            options.Follow = false;
            TwitterUser user = account.twitterService.FollowUser(options);

            if (user != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<TwitterItem> executeSearch(TwitterService service, string keyword, TwitterSavedSearch search, AccountTwitter account)
        {
            AppController.Current.Logger.writeToLogfile("Search for " + keyword + " background process started");
            TwitterSearchResult searchResult;

            List<TwitterItem> searchResults = new List<TwitterItem>();
            if (account == null)
            {
                if (AppController.Current.AllTwitterAccounts.Count > 0)
                {
                    account = AppController.Current.AllTwitterAccounts[0] as AccountTwitter;
                }
                else
                {
                    return searchResults;
                }
            }

            SearchOptions options = new SearchOptions();
            if (search != null)
            {
                options.Q = search.Query;
            }
            else
            {
                search = new TwitterSavedSearch();
            }
            if(string.IsNullOrEmpty(options.Q)) {
                options.Q = keyword;
            }
            searchResult = service.Search(options);

            
            if (searchResult != null)
            {
                AppController.Current.Logger.writeToLogfile("Search for " + keyword + " found " + searchResult.Statuses.Count() + " items");
                foreach (TwitterStatus found in searchResult.Statuses)
                {
                    AppController.Current.Logger.writeToLogfile("Converting search result for " + keyword + " with id " + found.Id.ToString() + " to Nymphicus item");
                    TwitterItem item = API.TweetSharpConverter.getItemFromTwitterSearchResult(found, account);
                    AppController.Current.Logger.writeToLogfile("Converted successfully search result " + found.Id.ToString() + " to Nymphicus item");
                    if (search != null)
                    {
                        item.SourceSearchId = search.Id;
                        item.isSearchResult = true;
                        item.searchName = keyword;
                    }
                    searchResults.Add(item);
                }
            }
            else
            {
                AppController.Current.Logger.writeToLogfile("Search for " + keyword + " failed");
            }



            return searchResults;
        }

        public static List<Person> executePersonSearch(string keyword, AccountTwitter account)
        {
            List<Person> searchResults = new List<Person>();
            SearchForUserOptions options = new TweetSharp.SearchForUserOptions();
            options.Q = keyword;

            IEnumerable<TwitterUser> users = account.twitterService.SearchForUser(options);

           
            if (users != null)
            {
                foreach (TwitterUser user in users)
                {
                    Person person = API.TweetSharpConverter.getPersonFromUser(user, account);
                    searchResults.Add(person);
                }
            }

            return searchResults;
        }


        public static OAuthAccessToken getOAuthTokens(AccountTwitter account)
        {
            OAuthAccessToken tokens = new TweetSharp.OAuthAccessToken();
            tokens.Token = account.Token;
            tokens.TokenSecret = account.TokenSecret;
            
            return tokens;
        }

        static public TwitterItem writeNewTweet(AccountTwitter account, string text, string upload_image_path = null)
        {
            try {
                TwitterStatus status = null;
                if(string.IsNullOrEmpty(upload_image_path)) {
                    SendTweetOptions options = new TweetSharp.SendTweetOptions();
                    options.Status = text;
                    status = account.twitterService.SendTweet(options);
                }
                else
                {
                    
                    SendTweetWithMediaOptions media_options = new SendTweetWithMediaOptions(); 
                    media_options.Status = text;
                    FileStream file_stream = System.IO.File.OpenRead(upload_image_path);
                    Dictionary<string, Stream> image_dictionary = new Dictionary<string, Stream>();
                    image_dictionary.Add(upload_image_path, file_stream);
                    media_options.Images = image_dictionary;
                    status = account.twitterService.SendTweetWithMedia(media_options);
                }


                if (status != null)
                {
                    return API.TweetSharpConverter.getItemFromStatus(status, account);
                }
                else
                {
                    System.Windows.MessageBox.Show("Failed", "Sending of tweet failed");
                    return null;
                }
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Sending of tweet failed");
                return null;
            }
        }

        public static TwitterItem replyToItem(AccountTwitter account, decimal inReplyToId, string text, string upload_image_path = null)
        {
            try
            {
                SendTweetOptions options = new TweetSharp.SendTweetOptions();
                options.Status = text;
                options.InReplyToStatusId = Convert.ToInt64(inReplyToId);
                TwitterStatus status = account.twitterService.SendTweet(options);


                if (status != null)
                {
                    return API.TweetSharpConverter.getItemFromStatus(status, account);
                }
                else
                {
                    System.Windows.MessageBox.Show("Failed", "Sending of tweet failed");
                    return null;
                }
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Sending of tweet failed");
                return null;
            }
        }

        public static TwitterItem writeDirectMessage(AccountTwitter account, string receiver, string text)
        {
            try
            {
                SendDirectMessageOptions options = new SendDirectMessageOptions();
                options.Text = text;
                options.ScreenName = receiver;
                TwitterDirectMessage status = account.twitterService.SendDirectMessage(options);

                if (status != null)
                {
                    return API.TweetSharpConverter.getItemFromDirectMessage(status, account);
                }
                else
                {
                    System.Windows.MessageBox.Show("Failed", "Sending of dm failed");
                    return null;
                }
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Sending of dm failed");
                return null;
            }
        }

        public static bool setFavoritState(AccountTwitter account, TwitterItem item, bool newState)
        {
            try
            {
                
                

                TwitterStatus status;
                if (newState)
                {
                    FavoriteTweetOptions options = new FavoriteTweetOptions();
                    options.Id = Convert.ToInt64(item.Id);
                    status = account.twitterService.FavoriteTweet(options);
                }
                else
                {
                    UnfavoriteTweetOptions options = new TweetSharp.UnfavoriteTweetOptions();
                    options.Id = Convert.ToInt64(item.Id);
                    status = account.twitterService.UnfavoriteTweet(options);
                }

                if (status != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Sending of dm failed");
                return false;
            }
        }

        public static string decodeHtml(string sourceString)
        {
            sourceString = sourceString.Replace("&lt;", "<");
            sourceString = sourceString.Replace("&gt;", ">");
            sourceString = sourceString.Replace("&quot;", "\"");
            sourceString = sourceString.Replace("&amp;", "&");
            return sourceString;
        }

        public static string RemoveEmptyLines(string hasEmptyLines)
        {
            string emptyLinesRemoved = string.Empty;
            string[] lines = hasEmptyLines.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                emptyLinesRemoved += line + Environment.NewLine;
            }
            return emptyLinesRemoved;
        }

        #region Lists

        public static List<TwitterItem> getListItems(
            TwitterService service, 
            AccountTwitter account, 
            string username, 
            string listNameOrId, 
            decimal listId,  
            string slug, 
            decimal sinceId, 
            DoWorkEventArgs e)
        {
            IEnumerable<TwitterStatus> tweets;
            List<TwitterItem> allTweets = new List<TwitterItem>();
            try
            {
                if (service == null)
                {
                    return new List<TwitterItem>();
                }
                ListTweetsOnListOptions options = new TweetSharp.ListTweetsOnListOptions();
                options.Slug = slug;
                options.ListId = Convert.ToInt64(listId);
                options.OwnerScreenName = username;
                service.TraceEnabled = true;
                options.Count = Properties.Settings.Default.TwitterItemsFetchInPast;
                if (sinceId > 0)
                {
                    options.SinceId = Convert.ToInt64(sinceId);
                }

                IAsyncResult result = service.BeginListTweetsOnList(options);
                tweets = service.EndListTweetsOnList(result);

                if (tweets != null)
                {
                    foreach (TwitterStatus status in tweets)
                    {
                        if (e != null)
                        {
                            if (e.Cancel)
                            {
                                AppController.Current.Logger.writeToLogfile("Cancel received for timeline");
                                break;
                            }
                        }
                        if (status.Id < Convert.ToInt64(sinceId))
                        {
                            continue;
                        }
                        else
                        {
                            TwitterItem item = API.TweetSharpConverter.getItemFromStatus(status, account);
                            if (item != null)
                            {
                                item.SourceListId = listId;
                                allTweets.Add(item);
                            }

                        }
                    }
                }
                else
                {
                    TwitterError error = service.Response.Error;
                    Console.WriteLine();
                }
            }
            catch (Exception exp)
            {
                TwitterError error = service.Response.Error;
                AppController.Current.sendNotification("ERROR", exp.Message, exp.StackTrace, "", null);
                
            }
            return allTweets;

        }

        public static List<Person> getListMembers(AccountTwitter account, string username, string listNameOrId)
        {
            ListListMembersOptions options = new TweetSharp.ListListMembersOptions();
            IEnumerable<TwitterUser> users;
            List<Person> members = new List<Person>();
            options.OwnerScreenName = username;
            try
            {
                Int64 id = 0;
                if (Int64.TryParse(listNameOrId, out id))
                {
                    options.ListId = id;
                }
                IAsyncResult result = account.twitterService.BeginListListMembers(options);
                users = account.twitterService.EndListListMembers(result);

                if (users != null)
                {
                    foreach (TwitterUser user in users)
                    {
                        members.Add(TweetSharpConverter.getPersonFromUser(user, account));
                    }
                }
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("ERROR", "Fetching list members failed", exp.Message, "", null);
            }

            return members;
        }

        public static void addSearchToAccount(AccountTwitter account, string query)
        {
            CreateSavedSearchOptions options = new TweetSharp.CreateSavedSearchOptions();
            options.Query = query;

              IAsyncResult result = account.twitterService.BeginCreateSavedSearch(options);
                TwitterSavedSearch savedSearch = account.twitterService.EndCreateSavedSearch(result);

           
            if (savedSearch != null)
            {
                account.addSearch(TweetSharpConverter.getSearchFromTwitterSavedSearch(account, savedSearch));
            }
        }

        /*
        public static List<Person> getPersonsFromId(AccountTwitter account, List<decimal> ids)
        {
            /*
            TweetSharp.ListUserProfilesForOptions options = new TweetSharp.ListUserProfilesForOptions();
            options.UserId = 

            List<Person> persons = new List<Person>();
            Person person = new Person(account);
            LookupUsersOptions options = new LookupUsersOptions();
            options.UseSSL = true;
            options.UserIds = ids;
            TwitterResponse<TwitterUserCollection> response = TwitterUser.Lookup(account.Tokens, options);
            if (response.Result == RequestResult.Success)
            {
                foreach (TwitterUser user in response.ResponseObject)
                {
                    persons.Add(TweetSharpConverter.getPersonFromUser(user, account));
                }
            }
            return persons;
        }
         */

        public static List<TweetList> getLists(AccountTwitter account, string username)
        {
            ListListsForOptions options = new TweetSharp.ListListsForOptions();
            options.ScreenName = username;
            List<TweetList> Lists = new List<TweetList>();


            if (!string.IsNullOrEmpty(username))
            {
                IAsyncResult result = account.twitterService.BeginListListsFor(options);
                IEnumerable<TwitterList> lists = account.twitterService.EndListListsFor(result);

                if (lists != null)
                {
                    foreach (TwitterList list in lists)
                    {
                        Lists.Add(API.TweetSharpConverter.getTweetListFromList(list, account));
                    }
                }

            }
            else
            {
                AppController.Current.Logger.addDebugMessage("getLists called without username provided", "", account: account, type: DebugMessage.DebugMessageTypes.LogMessage);
            }
            return Lists;
        }

        public static TweetList createList(AccountTwitter account, string name, string description, bool publicVisible)
        {
            CreateListOptions options = new TweetSharp.CreateListOptions();
            options.Name = name;
            options.Description = description;
            if(publicVisible) {
                options.Mode = TweetSharp.TwitterListMode.Public;
            }
            else
            {
                options.Mode = TweetSharp.TwitterListMode.Private;
            }

           TwitterList list =  account.twitterService.CreateList(options);

           
            if (list != null)
            {
                TweetList newList = API.TweetSharpConverter.getTweetListFromList(list, account);
                newList.IsOwnList = true;
                account.Lists.Add(newList);
                return newList;
            }
            else
            {
                System.Windows.MessageBox.Show("Twitter returned the following info: \"failed\"", "List creation failed");
                return null;
            }
        }

        public static bool deleteList(AccountTwitter account, TweetList list)
        {
            DeleteListOptions options = new TweetSharp.DeleteListOptions();
            options.ListId = Convert.ToInt64(list.Id);
            options.OwnerScreenName = account.Login.Username;
            options.Slug = list.Slug;

            TwitterList tlist = null;

            if (list.IsOwnList)
            {
                IAsyncResult result = account.twitterService.BeginDeleteList(options);
                tlist = account.twitterService.EndDeleteList(result);
            }
            else
            {
                // xxx hä? wie kann man eine Liste unsubscriben...?
            }
            if (tlist != null)
            {
                account.Lists.Remove(list);
                AppController.Current.AllLists.Remove(list);
                list = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool addPersonToList(Person person, TweetList list, AccountTwitter account)
        {
            AddListMemberOptions options = new TweetSharp.AddListMemberOptions();
            options.OwnerScreenName = account.username;
            options.Slug = list.Slug;
            options.UserId = Convert.ToInt64(person.Id);
            TwitterUser user = account.twitterService.AddListMember(options);
            
            if (user != null)
            {
                list.Members.Add(person);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool removePersonFromList(Person person, TweetList list, AccountTwitter account)
        {
            RemoveListMemberOptions options = new TweetSharp.RemoveListMemberOptions();
            options.OwnerScreenName = account.username;
            options.Slug = list.Slug;
            options.UserId = Convert.ToInt64(person.Id);

            TwitterUser user = account.twitterService.RemoveListMember(options);

            if (user != null)
            {
                if(list.Members.Contains(person)) {
                    list.Members.Remove(person);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Searches

        public static List<Search> getSearches(AccountTwitter account)
        {
            return getSearches(account, null);
        }

        public static List<Search> getSearches(AccountTwitter account, DoWorkEventArgs e)
        {
            List<Search> searches = new List<Search>();
           
            IAsyncResult result = account.twitterService.BeginListSavedSearches();
            IEnumerable<TwitterSavedSearch> tsearches = account.twitterService.EndListSavedSearches(result);

            
            if (tsearches != null) 
            {
                foreach (TwitterSavedSearch search in tsearches)
                {
                    if (e != null)
                    {
                        if (e.Cancel) {
                            AppController.Current.Logger.writeToLogfile("Cancel received for search");
                            break;
                        }
                    }
                    searches.Add(API.TweetSharpConverter.getSearchFromTwitterSavedSearch(account, search));
                }
            }
            return searches;
        }

        #endregion

        public static  Image DownloadBinaryFromInternet(string Inurl)
        {
            Uri url = new Uri(Inurl);
            string urlHost = url.Host;
            Image BookmarkIcon = null;
            if (url.HostNameType == UriHostNameType.Dns)
            {
                string iconUrl = Inurl;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(iconUrl);
                    System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    System.IO.Stream stream = response.GetResponseStream();
                    BookmarkIcon = Image.FromStream(stream);
                }
                catch (Exception exp)
                {
                    AppController.Current.Logger.writeToLogfile(exp);
                }
                return BookmarkIcon;
            }
            else
            {
                return null;
            }
        }
    
    }
}
