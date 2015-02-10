using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.Model;
using TweetSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Nymphicus.API
{
    public class TweetSharpConverter
    {
        public static Person getPersonFromUser(TwitterUser user, AccountTwitter account)
        {
            Person person = new Person(account);
            person.Username = user.ScreenName;
            person.Id = user.Id;
            person.Realname = user.Name;
            person.ProfileBackgroundColorString = user.ProfileBackgroundColor;
            person.Avatar = user.ProfileImageUrl;
            person.CreateDate = user.CreatedDate.ToLocalTime();
            person.Description = user.Description;
            person.FollowRequestSend = user.FollowRequestSent;
            person.IsContributorsEnabled = user.ContributorsEnabled;
            //person.IsFollowedBy = user.IsFollowedBy;
            //person.IsFollowing = user.IsFollowing;
            person.IsGeoEnabled = user.IsGeoEnabled;
            person.IsProfileBackgroundTiled = user.IsProfileBackgroundTiled;
            person.IsProtected = user.IsProtected;
            person.Language = user.Language;
            person.ListedCount = user.ListedCount;
            person.Location = user.Location;
            person.NumberOfFavorites = user.FavouritesCount;
            person.NumberOfFollowers = user.FollowersCount;
            person.NumberOfFriends = user.FriendsCount;
            person.NumberOfItems = user.FriendsCount;
            person.Timezone = user.TimeZone;
            
            person.Verified = user.IsVerified;
            person.Website = user.Url;
            //person.LastTweet = getItemFromStatus(user.Status);
            return person;
        }

        public static TwitterItem  getItemFromStatus(TwitterStatus status, AccountTwitter account)
        {
            if (status == null)
            {
                return null;
            }
            try
            {
                TwitterItem item = new TwitterItem();
                item.RetrievingAccount = account;
                string text;
                if (ExternalServices.Twitlonger.IsTwitLongerText(Functions.decodeHtml(status.Text)))
                {
                    ExternalServices.Twitlonger.TwitLongerResponse twitLongerResponse = ExternalServices.Twitlonger.GetLongText(Functions.decodeHtml(status.Text));
                    text = twitLongerResponse.MessageText;
                    item.IsTwitLongerItem = true;
                }
                else
                {
                    text = Functions.decodeHtml(status.Text);
                }
            
                if(string.IsNullOrEmpty(text)) {
                    return null;
                }

                try
                {
                    if (AppController.Current.AllTwitterAccounts.Where(a => a.Login.Id == status.User.Id).Count() > 0)
                    {
                        Nymphicus.Model.AccountTwitter ownaccount = AppController.Current.AllTwitterAccounts.Where(a => a.Login.Id == status.User.Id).First();
                        item.OwnAccountHavingWrittenThisTweet = ownaccount;
                    }
                }
                catch { }

                if (item.Author == null)
                {
                    item.Author = getPersonFromUser(status.User, account);
                    if (item.Author != null)
                    {
                        try
                        {
                            AppController.Current.AllPersons.Add(item.Author);
                        }
                        catch { }
                    }
                }
                item.Id = status.Id;

                
                item.SourceString = status.Source;
                item.CreatedAt = status.CreatedDate.ToLocalTime();

                item.isFavorited = status.IsFavorited;
                
                item.Entities = status.Entities;
                if (status.InReplyToStatusId.HasValue)
                {
                    item.InReplyToStatusId = status.InReplyToStatusId.Value;
                }
                if (item.Entities != null)
                {
                    if (item.Entities.Urls != null)
                    {
                        foreach (TwitterUrl urlEntity in item.Entities.Urls)
                        {
                            if (!string.IsNullOrEmpty(urlEntity.ExpandedValue))
                            {
                                text = text.Replace(urlEntity.Value, urlEntity.ExpandedValue);
                                if (!AppController.Current.AllShortenedLinksInItems.ContainsKey(urlEntity.Value))
                                {
                                    try
                                    {
                                        AppController.Current.AllShortenedLinksInItems.Add(urlEntity.Value, urlEntity.ExpandedValue);
                                    }
                                    catch
                                    {
                                        // as we are asynchron there might be another thread having added it already...
                                    }
                                }
                            }
                        }
                    }

                    if (item.Entities.Media != null)
                    {
                        foreach (TwitterMedia mediaEntity in item.Entities.Media)
                        {
                            if (mediaEntity.MediaType == TwitterMediaType.Photo)
                            {
                                if (!string.IsNullOrEmpty(mediaEntity.ExpandedUrl))
                                {
                                    text = text.Replace(mediaEntity.Url, mediaEntity.ExpandedUrl);
                                    TwitterItem.embedded_image image = new TwitterItem.embedded_image();
                                    image.url = mediaEntity.ExpandedUrl;
                                    image.thumbnail_url = mediaEntity.MediaUrl;
                                    item.imagesInPost.Add(image);
                                    if (!AppController.Current.AllImagesInItems.ContainsKey(mediaEntity.ExpandedUrl))
                                    {
                                        AppController.Current.AllImagesInItems.Add(mediaEntity.ExpandedUrl, mediaEntity.MediaUrl);
                                    }
                                }
                            }
                        }
                    }
                    if (item.Entities.HashTags != null)
                    {
                        foreach (TwitterHashTag hashEntity in item.Entities.HashTags)
                        {

                            if (hashEntity != null)
                            {
                                if (!AppController.Current.AllKnownHashtags.Contains("#" + hashEntity.Text))
                                {
                                    AppController.Current.AllKnownHashtags.Add("#" + hashEntity.Text);
                                }
                            }
                        }
                    }
                }
                item.Text = text;
                if (status.Place != null)
                {
                    Geo geo = new Geo();
                    geo.CityOrEqual = status.Place.FullName;
                    geo.StreetName = status.Place.Name;
                    item.Place = geo;
                }
                if (status.RetweetedStatus != null)
                {
                    item.RetweetedItem = getItemFromStatus(status.RetweetedStatus, account);
                    if (item.RetweetedItem != null)
                    {
                        if (AppController.Current.AllAccounts.Where(saccount => saccount.Id == status.User.Id).Count() > 0)
                        {
                            item.isRetweetedByMe = true;
                        }
                        else
                        {
                            item.isRetweetedToMe = true;
                        }
                    }
                }


                if (item.Author.ProfileBackgroundColorString != "")
                {
                    //  item.BackgroundColor = "#" + item.Author.ProfileBackgroundColorString;
                }
                return item;
            }
            catch
            {
                return null;
            }
        }

        public static TwitterItem getItemFromDirectMessage(TwitterDirectMessage directMessage, AccountTwitter account)
        {
            TwitterItem item = new TwitterItem();
            item.RetrievingAccount = account;
            string text = Functions.decodeHtml(directMessage.Text);
            item.Author = getPersonFromUser(directMessage.Sender, account);
            item.Id = directMessage.Id;
            item.Source.Name = "Direct message";
            item.CreatedAt = directMessage.CreatedDate.ToLocalTime();
            item.DMReceipient = getPersonFromUser(directMessage.Recipient, account);
           
            if (item.Author.ProfileBackgroundColorString != "")
            {
                //item.BackgroundColor = "#" + item.Author.ProfileBackgroundColorString;
            }


            try
            {
                if (AppController.Current.AllTwitterAccounts.Where(a => a.Login.Id == item.Author.Id).Count() > 0)
                {
                    Nymphicus.Model.AccountTwitter ownaccount = AppController.Current.AllTwitterAccounts.Where(a => a.Login.Id == item.Author.Id).First();
                    item.OwnAccountHavingWrittenThisTweet = ownaccount;
                }
            }
            catch { }

            item.isDirectMessage = true;
            item.Entities = directMessage.Entities;
            if (item.Entities != null)
            {
                if (item.Entities.Urls != null)
                {
                    foreach (TwitterUrl urlEntity in item.Entities.Urls)
                    {
                        if (!string.IsNullOrEmpty(urlEntity.ExpandedValue))
                        {
                            text = text.Replace(urlEntity.Value, urlEntity.ExpandedValue);
                            if (!AppController.Current.AllShortenedLinksInItems.ContainsKey(urlEntity.Value))
                            {
                                try
                                {
                                    AppController.Current.AllShortenedLinksInItems.Add(urlEntity.Value, urlEntity.ExpandedValue);
                                }
                                catch
                                {
                                    // as we are asynchron there might be another thread having added it already...
                                }
                            }
                        }
                    }
                }

                if (item.Entities.Media != null)
                {
                    foreach (TwitterMedia mediaEntity in item.Entities.Media)
                    {
                        if (mediaEntity.MediaType == TwitterMediaType.Photo)
                        {
                            if (!string.IsNullOrEmpty(mediaEntity.ExpandedUrl))
                            {
                                text = text.Replace(mediaEntity.Url, mediaEntity.ExpandedUrl);
                                TwitterItem.embedded_image image = new TwitterItem.embedded_image();
                                image.url = mediaEntity.ExpandedUrl;
                                image.thumbnail_url = mediaEntity.MediaUrl;
                                item.imagesInPost.Add(image);
                                if (!AppController.Current.AllImagesInItems.ContainsKey(mediaEntity.ExpandedUrl))
                                {
                                    AppController.Current.AllImagesInItems.Add(mediaEntity.ExpandedUrl, mediaEntity.MediaUrl);
                                }
                            }
                        }
                    }
                }
                if (item.Entities.HashTags != null)
                {
                    foreach (TwitterHashTag hashEntity in item.Entities.HashTags)
                    {

                        if (hashEntity != null)
                        {
                            if (!AppController.Current.AllKnownHashtags.Contains("#" + hashEntity.Text))
                            {
                                AppController.Current.AllKnownHashtags.Add("#" + hashEntity.Text);
                            }
                        }
                    }
                }
            }
                
            item.Text = text;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            return item;
        }


        public static TwitterItem getItemFromTwitterSearchResult(TwitterStatus result, AccountTwitter account)
        {
            AppController.Current.Logger.writeToLogfile("getItemFromTwitterSearchResult started");
            TwitterItem item = getItemFromStatus(result,account);
            item.isSearchResult = true;
            AppController.Current.Logger.writeToLogfile("getItemFromTwitterSearchResult finihsed");
            return item;
        }

        public static Person getPersonFromLogin(string login, AccountTwitter account) {
            GetUserProfileForOptions options = new GetUserProfileForOptions();
            options.ScreenName = login;
            IAsyncResult result = account.twitterService.BeginGetUserProfileFor(options);
            Person person = new Person(account);
            TwitterUser user = account.twitterService.EndGetUserProfileFor(result);

            if (user != null)
            {
                person = getPersonFromUser(user, account);
            }

            return person;
        }

        public static TweetList getTweetListFromList(TwitterList list, AccountTwitter account)
        {
            TweetList tweetList = new TweetList(account);
            tweetList.name = list.Name;
            tweetList.person = getPersonFromUser(list.User, account);
            tweetList.Id = list.Id;
            tweetList.Slug = list.Slug;
            tweetList.FullName = list.FullName;
            if (list.User != null)
            {
                tweetList.NameAndCreator = list.Name + " (@" + list.User.ScreenName + ")";
                tweetList.IsOwnList = false;
            }
            else
            {
                tweetList.NameAndCreator = list.Name;
                tweetList.IsOwnList = true;
            }
            return tweetList;
        }

        public static Search getSearchFromTwitterSavedSearch(AccountTwitter account, TwitterSavedSearch twitterSearch) {
            Search search = new Search(account);
            search.CreatedAt = twitterSearch.CreatedDate.ToLocalTime();
            search.Id = twitterSearch.Id;
            search.name = twitterSearch.Name;
            search.query = twitterSearch.Query;
            search.Positon = twitterSearch.Position;
            
            return search;
        }
    }
}
