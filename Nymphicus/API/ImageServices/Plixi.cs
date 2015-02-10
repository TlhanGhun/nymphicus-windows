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

namespace Nymphicus.API.ImageServices
{
    class Plixi : IImageService
    {
        // private string apiEndpoint = "http://api.plixi.com/";
        private string apiKey = API.ConnectionData.plixiApiKey;

        private List<CompatibleService> services;

        public Plixi()
        {
            services = new List<CompatibleService>();
            services.Add(CompatibleService.All);
        }

        public List<CompatibleService> CompatibleServices
        {
            get { return services; }
        }

        public bool CanUpload
        {
            get { return false; }
        }

        public ImageResponse Upload(string Filepath, string Description, Model.AccountTwitter account)
        {
            return Upload(Filepath, account);
        }

        public ImageResponse Upload(string Filepath, Model.AccountTwitter account)
        {
            return null;
            /* hhh
            if (File.Exists(Filepath))
            {
                // Read file data
                FileStream fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                Twitterizer.WebRequestBuilder oauthRequest = new Twitterizer.WebRequestBuilder(new Uri("https://api.twitter.com/1/account/verify_credentials.xml"), Twitterizer.HTTPVerb.GET, account.Tokens);
                oauthRequest.PrepareRequest();

                // Generate post objects
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                Dictionary<string, string> additonalHeaders = new Dictionary<string, string>();
                string CredentialAuthorization = oauthRequest.GenerateAuthorizationHeader();

                additonalHeaders.Add("X-Verify-Credentials-Authorization", CredentialAuthorization);
                additonalHeaders.Add("X-Auth-Service-Provider", "https://api.twitter.com/1/account/verify_credentials.xml");

                postParameters.Add("api_key", apiKey);
                postParameters.Add("response_format", "json");
                postParameters.Add("isoauth", "true");
                postParameters.Add("media ", data);

                // Create request and receive response
                string postURL = "http://tweetphotoapi.com/api/upload.aspx";
                HttpWebResponse webResponse = WebHelpers.MultipartFormDataPost(postURL, postParameters, account, additonalHeaders);

                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                string fullResponse = responseReader.ReadToEnd();
                webResponse.Close();
                return parseResponse(fullResponse);
            }
            else
            {
                return null;
            }
             * */
        }

        public bool IsUrlFromThisService(string Url)
        {
            return (Url.ToLower().StartsWith("http://plixi.com/p/") || Url.ToLower().StartsWith("http://lockerz.com/s/"));
        }

        public ImageResponse GetImage(string Identifier)
        {
            throw new NotImplementedException();
        }

        public string GetMini(string Url)
        {
            return string.Format("http://api.plixi.com/api/tpapi.svc/imagefromurl?url={0}&size=medium", System.Web.HttpUtility.UrlEncode(Url));
        }

        public string Name
        {
            get
            {
                return "Plixi";
            }
        }

        private static ImageResponse parseResponse(string response)
        {

            ImageResponse imageResponse = new ImageResponse();
            plixiImage json;
            try
            {
                json = JsonConvert.DeserializeObject<plixiImage>(response);
            }
            catch (Exception exp)
            {
                imageResponse.ErrorText = exp.Message;
                return imageResponse;
            }
            imageResponse.Success = true;
            imageResponse.Id = json.MediaId;
            imageResponse.UrlFull = json.MediaUrl;
            imageResponse.UrlThumbnail = json.MediaUrl;

            return imageResponse;
        }

        private class plixiImage
        {
            public string MediaId { get; set; }
            public string MediaUrl { get; set; }
            public string Status { get; set; }
            public int UserId { get; set; }

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
            throw new NotImplementedException();
        }


        public ImageResponse Upload(string Filepath, Model.IAccount account)
        {
            return Upload(Filepath, "", account);
        }
    }
}
