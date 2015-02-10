using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.Model;
using System.Net;
using System.IO;
using TweetSharp;
using Hammock;

namespace Nymphicus.API
{
    public static class Tweetmarker
    {
        public static bool storeTweetMark(AccountTwitter account, string type, decimal id)
        {
            return false;
            Hammock.RestRequest oauthRequest = account.twitterService.PrepareEchoRequest();
            oauthRequest.Path = string.Format("/v1/lastread?collection={0}&username={1}&api_key={2}", type, account.Login.Username.ToLower(), "");
            oauthRequest.UserAgent = "Nymphicus for Windows";
            oauthRequest.Method = Hammock.Web.WebMethod.Post;

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            Dictionary<string, string> additonalHeaders = new Dictionary<string, string>();
            
            // Create request and receive response
            string postURL = string.Format("https://api.tweetmarker.net/");
            RestClient client = new RestClient { Authority = postURL };
            client.Method = Hammock.Web.WebMethod.Post;
            client.AddPostContent(WebHelpers.getBytesFromString(id.ToString()));

            RestResponse response = client.Request(oauthRequest);

            return (response.StatusCode == HttpStatusCode.OK);
            
        }


        public static decimal getTweetMark(AccountTwitter account, string type)
        {
            return 0;
           // Twitterizer.WebRequestBuilder oauthRequest = new Twitterizer.WebRequestBuilder(new Uri("https://api.twitter.com/1/account/verify_credentials.json"), Twitterizer.HTTPVerb.GET, account.Tokens);
           // oauthRequest.SetupOAuth();
           
            Dictionary<string, string> additonalHeaders = new Dictionary<string, string>();
           // string CredentialAuthorization = oauthRequest.GenerateAuthorizationHeader();

           // additonalHeaders.Add("X-Verify-Credentials-Authorization", CredentialAuthorization);
           // additonalHeaders.Add("X-Auth-Service-Provider", "https://api.twitter.com/1/account/verify_credentials.json");

            // Create request and receive response
            string postURL = string.Format("https://api.tweetmarker.net/v1/lastread?collection={0}&username={1}&api_key={2}", type, account.Login.Username.ToLower(), ConnectionData.tweetMarkerApiKey);
            WebHelpers.Response webResponse = WebHelpers.SendGetRequest(postURL, additonalHeaders);

            decimal lastReadTweet = 0;
            decimal.TryParse(webResponse.Content, out lastReadTweet);
            return lastReadTweet;
        }
    }
}
