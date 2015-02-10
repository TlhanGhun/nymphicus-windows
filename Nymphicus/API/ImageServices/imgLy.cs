//-----------------------------------------------------------------------
// <copyright file="imgly.cs" company="lI' Ghun">
// 
//  Copyright (c) 2011, Sven Walther (sven@li-ghun.de)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Nymphicus nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Sven Walther</author>
// <summary>Implementation of IImageService for img.ly</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using Newtonsoft.Json;

namespace Nymphicus.API.ImageServices
{
    public class imgLy : IImageService
    {
        private const string apiEndpoint = "http://img.ly/api/2/upload.json";

        private List<CompatibleService> services;

        public imgLy()
        {
            services = new List<CompatibleService>();
            services.Add(CompatibleService.Twitter);
            services.Add(CompatibleService.AppNet);
        }

        public bool CanUpload
        {
            get { return false; }
        }

        public ImageResponse Upload(string Filepath, string Description, Model.AccountTwitter account)
        {
            return Upload(Filepath, account);
        }

        public ImageResponse Upload(string Filepath, Model.IAccount account)
        {
            if (account == null)
            {
                return null;
            }
            if(account.GetType() == typeof(Model.AccountTwitter)) {
                return UploadToTwitter(Filepath,account as Model.AccountTwitter);
            }
            else if (account.GetType() == typeof(Model.AccountAppDotNet))
            {
                return UploadToApn(Filepath, account as Model.AccountAppDotNet);
            }
            else
            {
                return null;
            }
        }

        private ImageResponse UploadToTwitter(string Filepath, Model.AccountTwitter account)
        {/* hhh
            if (File.Exists(Filepath))
            {
                // Read file data
                FileStream fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                Twitterizer.WebRequestBuilder oauthRequest = new Twitterizer.WebRequestBuilder(new Uri("https://api.twitter.com/1/account/verify_credentials.json"), Twitterizer.HTTPVerb.GET, account.Tokens);
                oauthRequest.PrepareRequest();
               

                // Generate post objects
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();
                string CredentialAuthorization = oauthRequest.GenerateAuthorizationHeader();

                additionalHeaders.Add("X-Verify-Credentials-Authorization", CredentialAuthorization);
                additionalHeaders.Add("X-Auth-Service-Provider", "https://api.twitter.com/1/account/verify_credentials.json");

                postParameters.Add("media", data);



                // Create request and receive response
                string postURL = apiEndpoint;
                HttpWebResponse webResponse = WebHelpers.MultipartFormDataPost(postURL, postParameters, account, additionalHeaders);

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
            return null;
        }

        private ImageResponse UploadToApn(string Filepath, Model.AccountAppDotNet account)
        {
            string delegateClientIdOfImgLy = "XaBgfNdXRL6msTWhewgVaMB3BumT22YQ";

            if (File.Exists(Filepath))
            {
                // Read file data
                FileStream fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                Dictionary<string,string> oauthParams = new Dictionary<string,string>();
                oauthParams.Add("grant_type","delegate");
                oauthParams.Add("delegate_client_id",delegateClientIdOfImgLy);

                    Dictionary<string, string> oAuthHeaders = new Dictionary<string, string>();
                    oAuthHeaders.Add("Authorization", "Bearer " + account.accessToken);

                    WebHelpers.Response oAuthResponse = WebHelpers.SendPostRequest("https://alpha.app.net/oauth/access_token",oauthParams, additionalHeaders: oAuthHeaders);

                    oAuthDelegateToken oAuthToken = JsonConvert.DeserializeObject<oAuthDelegateToken>(oAuthResponse.Content);
                    if (oAuthToken == null)
                    {
                        return null;
                    }



                // Generate post objects
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

                additionalHeaders.Add("Identity-Delegate-Token", oAuthToken.delegate_token);
                additionalHeaders.Add("Identity-Delegate-Endpoint", "https://alpha-api.app.net/stream/0/token");

                postParameters.Add("media", data);



                // Create request and receive response
                HttpWebResponse webResponse = WebHelpers.MultipartFormDataPost("http://img.ly/api/2/upload.json", postParameters, null, additionalHeaders);

                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                string fullResponse = responseReader.ReadToEnd();
                webResponse.Close();
                return parseResponse(fullResponse);
            }
            else
            {
                return null;
            }
        }

        public bool IsUrlFromThisService(string Url)
        {
            return Url.ToLower().StartsWith("http://img.ly/");
        }

        public ImageResponse GetImage(string Identifier)
        {
            throw new NotImplementedException();
        }

        private static ImageResponse parseResponse(string response)
        {

            ImageResponse imageResponse = new ImageResponse();
            imglyImage json;
            try
            {
                json = JsonConvert.DeserializeObject<imglyImage>(response);
            }
            catch (Exception exp)
            {
                imageResponse.ErrorText = exp.Message;
                return imageResponse;
            }
            imageResponse.Success = true;
            imageResponse.Id = json.id;
            imageResponse.UrlFull = json.url;
            imageResponse.UrlThumbnail = "http://img.ly/show/" + json.id;

            return imageResponse;
        }

        private class imglyImage
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
                return "img.ly";
            }
        }


        public string GetMini(string Url)
        {
            if (Url.Length > 14)
            {
                return string.Format("http://img.ly/show/mini/{0}", Url.Substring(14));
            }
            else
            {
                return string.Empty;
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


        public List<CompatibleService> CompatibleServices
        {
            get { return services; }
        }


        public ImageResponse Upload(string Filepath, string Description, Model.IAccount account)
        {
            return Upload(Filepath, account);
        }
    }
}
