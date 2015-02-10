using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Nymphicus.Model
{
    public class FacebookUser
    {
        public AccountFacebook ReceivingAccount { get; set; }

        public enum Genders
        {
            Male,
            Female
        }

        public string Id { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public Genders Gender { get; set; }
        public string Category { get; set; }
        private string _resolvedAvatar { get; set; }
        public string Avatar
        {
            get
            {
                if (this.Id == null || this.FullName == null)
                {
                    return null;
                }
                AppController.Current.Logger.writeToLogfile("Get avatar icon for Facebook user " + Id.ToString() + " (" + FullName + ")");
                if (!string.IsNullOrEmpty(_resolvedAvatar))
                {
                    return _resolvedAvatar;
                }
                if (!string.IsNullOrEmpty(Id))
                {
                    try
                    {
                        AppController.Current.Logger.writeToLogfile("Resolving avatar icon for Facebook user " + Id.ToString() + " (" + FullName + ")");
                        WebRequest request = WebRequest.Create("https://graph.facebook.com/" + Id + "/picture");
                        WebResponse response = request.GetResponse();
                        if (response != null)
                        {
                            if (response.ResponseUri != null)
                            {
                                _resolvedAvatar = response.ResponseUri.AbsoluteUri;
                                response.Close();
                                return _resolvedAvatar;
                            }
                            response.Close();
                        }

                    }
                    catch (Exception exp)
                    {
                        AppController.Current.Logger.writeToLogfile("Resolving the avatar image of " + Id.ToString() + " failed with an exception");
                        AppController.Current.Logger.writeToLogfile(exp);
                        return "";
                    }
                    return "";

                }
                AppController.Current.Logger.writeToLogfile("Avatar resolving on user without Id");
                return "";
            }
        }

        public string OriginalAvatar
        {
            get
            {
                return "https://graph.facebook.com/" + Id + "/picture";
            }
        }

        public string Locale { get; set; }

        public List<Language> Languages { get; set; }
        public string ProfileLink { get; set; }
        /// <summary>
        /// Just to have the original api name af attribute in sync
        /// </summary>
        public string Link {
            get { return ProfileLink; } 
            set {ProfileLink = value;} 
        }
        public string ThirdPartyId { get; set; }
        public long Timezone { get; set; }
        public DateTime UpdatedTime { get; set; }
        public bool Verified { get; set; }
        public string Bio { get; set; }
        public DateTime Birthday { get; set; }
        public List<EducationItem> Education { get; set; }
        public string Email { get; set; }
        public string RelationshipStatus { get; set; }
        public LocationItem HomeTown { get; set; }
        public List<string> InterestedIn { get; set; }
        public LocationItem Location { get; set; }
        public string Political { get; set; }
        public string Quotes { get; set; }
        public string Religion { get; set; }
        public KeyValuePair<string,string> SignificantOther { get; set; }
        public string Website { get; set; }
        public List<WorkItem> Work { get; set; }

        public FacebookUser()
        {
            _resolvedAvatar = "";
            Gender = Genders.Male;
            Languages = new List<Language>();
            Education = new List<EducationItem>();
            InterestedIn = new List<string>();
            Work = new List<WorkItem>();
        }

        public static FacebookUser CreateFromDynamic(dynamic data, AccountFacebook ReceivingAccount) {
            if (data != null)
            {
                FacebookUser user = new FacebookUser();
                user.Id = data.id;
                user.FullName = data.name;
                if (data.username == null && data.first_name == null && ReceivingAccount != null)
                {
                    data = (IDictionary<string, object>)ReceivingAccount.facebookClient.Get(user.Id);
                }
                user.FirstName = data.first_name;
                user.LastName = data.last_name;
                user.MiddleName = data.middle_name;
                if (data.gender != null)
                {
                    if (data.gender == "female")
                    {
                        user.Gender = Genders.Female;
                    }
                }
                user.Locale = data.locale;
                user.ProfileLink = data.link;
                user.Username = data.username;
                user.ThirdPartyId = data.third_party_id;
                if (data.timezone != null)
                {
                    try
                    {
                        user.Timezone = data.timezone;
                    }
                    catch
                    {
                        user.Timezone = 0;
                    }
                }
                DateTime tempUpdated;
                DateTime.TryParse(data.updated_time, out tempUpdated);
                user.UpdatedTime = tempUpdated;
                bool tempVerified = false;
                try
                {
                    if (data.verified != null)
                    {
                        string verifyString = data.verified as string;
                        if (verifyString != null)
                        {
                            if (verifyString.ToLower() == "true")
                            {
                                user.Verified = true;
                            }
                        }
                    }
                    // Boolean.TryParse(data.verified, out tempVerified);
                    // much better code - but throws to many exception
                }
                catch { }
                user.Verified = tempVerified;
                user.Bio = data.bio;
                DateTime tempBirthday;
                DateTime.TryParse(data.birthday, out tempBirthday);
                user.Birthday = tempBirthday;
                user.Email = data.email;
                // this.HomeTown = data.hometown;
                user.Political = data.political;
                user.Quotes = data.quotes;
                user.RelationshipStatus = data.releationship_status;
                user.Religion = data.religion;
                user.Website = data.website;
                return user;
            }
            else
            {
                return null;
            }
        }

        public class Language
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class EducationItem
        {

        }

        public class WorkItem
        {

        }

        public class LocationItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
