using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuoteSharp;

namespace Nymphicus.Model
{
    public class QuoteFmUser
    {
        public string username { get; set; }
        public string Fullname { get; set; }
        public decimal Id { get; set; }
        public string Avatar { get; set; }
        public DateTime? Created { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        public string Twitter { get; set; }
        public string Url { get; set; }
        public decimal FollowingCount { get; set; }
        public decimal FollowerCount { get; set; }
        public ThreadSaveObservableCollection<QuoteFmUser> Followers { get; set; }
        public ThreadSaveObservableCollection<QuoteFmUser> Followings { get; set; }
        public ThreadSaveObservableCollection<QuoteFmItem> Recommendations { get; set; }

        public int NumberOfRecommendations
        {
            get
            {
                return Recommendations.Count();
            }
        }

        public QuoteFmUser()
        {
            Followers = new ThreadSaveObservableCollection<QuoteFmUser>();
            Followings = new ThreadSaveObservableCollection<QuoteFmUser>();
            Recommendations = new ThreadSaveObservableCollection<QuoteFmItem>();
        }

        public static QuoteFmUser createFromApi(User user, bool getFollowers = false, bool getFollowings = false, bool getRecommendations = false)
        {
            if (user == null)
            {
                return null;
            }
            QuoteFmUser qfmUser = new QuoteFmUser();
            qfmUser.username = user.username;
            qfmUser.Fullname = user.fullname;
            qfmUser.Id = user.id;
            qfmUser.Avatar = user.avatar;
            qfmUser.Created = user.created;
            qfmUser.Bio = user.bio;
            qfmUser.FollowingCount = user.followingCount;
            qfmUser.FollowerCount = user.followerCount;
            qfmUser.Location = user.location;
            qfmUser.Twitter = user.twitter;
            qfmUser.Url = user.url;

            if (getFollowings)
            {
                qfmUser.getFollowings();
            }

            return qfmUser;
        }

        public IEnumerable<QuoteFmItem> updateRecommendations()
        {
            decimal maxKnownId = 0;
            List<QuoteFmItem> newQuoteFmItems = new List<QuoteFmItem>();
            if (Recommendations.Count > 0)
            {
                maxKnownId = Recommendations.Max(r => r.Id);
            }
            ListOfRecommendations listOfRecommendations = QuoteSharp.API.getRecommendationsListByUser(this.username);
            if(listOfRecommendations != null) {
                if(listOfRecommendations.entities.Count() > 0) {
                    IEnumerable<Recommendation> newRecommendations = listOfRecommendations.entities.Where(r => r.id > maxKnownId);
                    foreach(Recommendation recommendation in newRecommendations) {
                        QuoteFmItem newItem = QuoteFmItem.createFromApi(recommendation);
                        if (newItem != null)
                        {
                            Recommendations.Add(newItem);
                            newQuoteFmItems.Add(newItem);
                        }
                    }
                    
                }
            }
            return newQuoteFmItems;            
        }

        public void  getFollowings() {
            ListOfUsers listOfFollwoings = QuoteSharp.API.getUsersListOfFollowings(this.username);
            Followings.Clear();
            foreach(User following in listOfFollwoings.entities) {
                Followings.Add(QuoteFmUser.createFromApi(following));
            }
        }

        public string DescriptiveText
        {
            get
            {
                string desc = "Account: " + this.Fullname + " (" + this.username + ")\n";
                desc += "Bio: " + this.Bio;

                return desc;
            }
        
        }
    }

    
}
