using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using System.Xml;
using Nymphicus.Model;
using Newtonsoft.Json;
using TweetSharp;
using Hammock;

namespace Nymphicus.API.ImageServices
{
    class TwitPic : IImageService
    {
        const string imageEndpoint = "http://api.twitpic.com/2/upload.json";

           private List<CompatibleService> services;

           public TwitPic()
        {
            services = new List<CompatibleService>();
            services.Add(CompatibleService.Twitter);
        }

        public bool CanUpload
        {
            get { return false; }
        }

        public ImageResponse Upload(string Filepath, string Description, AccountTwitter account, string text = "")
        {
            return Upload(Filepath, null, account, text);
        }

        public ImageResponse Upload(string Filepath, AccountTwitter account, string text = "")
        {
            if (File.Exists(Filepath))
            {
                TwitterService service = new TwitterService(API.ConnectionData.twitterConsumerKey, API.ConnectionData.twitterConsumerSecret);
                service.AuthenticateWith(account.Token, account.TokenSecret);

                // Prepare an OAuth Echo request to TwitPic
                RestRequest request = service.PrepareEchoRequest();
                request.Path = "upload.json";
                request.AddFile("media",System.IO.Path.GetFileName(Filepath),Filepath);
                request.AddField("key", API.ConnectionData.twitpicApiKey);
                request.AddField("message", text);

                // Post photo to TwitPic with Hammock
                RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "2" };
                RestResponse response = client.Request(request);
                return parseResponse(response.Content);
            }
            else
            {
                return null;
            }
             
        }

        public bool IsUrlFromThisService(string Url)
        {
            return Url.ToLower().StartsWith("http://twitpic.com/");
        }

        public ImageResponse GetImage(string Identifier)
        {
            throw new NotImplementedException();
        }

        private static ImageResponse parseResponse(string response)
        {
            
            ImageResponse imageResponse = new ImageResponse();
            TwitPicImage json;
            try
            {
                json = JsonConvert.DeserializeObject<TwitPicImage>(response);
            }
            catch (Exception exp)
            {
                imageResponse.ErrorText = exp.Message;
                return imageResponse;
            }
            imageResponse.Success = true;
            imageResponse.Id = json.id;
            imageResponse.UrlFull = json.url;
            imageResponse.UrlThumbnail = "http://twitpic.com/show/thumb/" + json.id;

            return imageResponse;
        }

        private class TwitPicImage
        {
            public string id { get; set; }
            public string text { get; set; }
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int size { get; set; }
            public string type { get; set; }
            public string timestamp { get; set; }
            public User user { get; set; }

            public class User
            {
                public string id { get; set; }
                public string screen_name { get; set; }
            }
        }


        public string Name
        {
            get
            {
                return "TwitPic";
            }
        }


        public string GetMini(string Url)
        {
            string ID = "";
            if (Url.Length > 19)
            {
                ID = Url.Substring(19);
                return string.Format("http://twitpic.com/show/thumb/{0}", ID);
            }
            else
            {
                return Url;
            }
        }


        public bool CanHaveCredentials
        {
            get { throw new NotImplementedException(); }
        }

        public bool NeedsCredentials
        {
            get { throw new NotImplementedException(); }
        }

        public string Login
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Password
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public ImageResponse Upload(string Filepath, string Description, IAccount account)
        {
            AccountTwitter twitterAccount = account as AccountTwitter;
            if (File.Exists(Filepath) && twitterAccount != null)
            {
                TwitterService service = new TwitterService(API.ConnectionData.twitterConsumerKey, API.ConnectionData.twitterConsumerSecret);
                service.AuthenticateWith(twitterAccount.Token, twitterAccount.TokenSecret);

                // Prepare an OAuth Echo request to TwitPic
                RestRequest request = service.PrepareEchoRequest();
                request.Path = "upload.json";
                request.AddFile("media", System.IO.Path.GetFileName(Filepath), Filepath);
                request.AddField("key", API.ConnectionData.twitpicApiKey);
                request.AddField("message", Description);

                // Post photo to TwitPic with Hammock
                RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "2" };
                RestResponse response = client.Request(request);
                return parseResponse(response.Content);
            }
            else
            {
                return null;
            }
        }

        public List<CompatibleService> CompatibleServices
        {
            get { return services; }
        }


        public ImageResponse Upload(string Filepath, Model.IAccount account)
        {
            return Upload(Filepath, "", account);
        }
    }
}
