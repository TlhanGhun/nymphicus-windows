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
    class yfrog : IImageService
    {
        const string imageEndpoint = "https://yfrog.com/api/xauth_upload";
        const string videoEndpoint = "http://render.imageshack.us/upload_api.php";

        private List<CompatibleService> services;

        public yfrog()
        {
            services = new List<CompatibleService>();
            services.Add(CompatibleService.Twitter);
        }

        public bool CanUpload
        {
            get { return false; }
        }

        public ImageResponse Upload(string Filepath, string Description, AccountTwitter account)
        {
            return Upload(Filepath, account);
        }

        public ImageResponse Upload(string Filepath, AccountTwitter account)
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

                postParameters.Add("key", API.ConnectionData.yfrogApiKey1);
                postParameters.Add("media", data);



                // Create request and receive response
                string postURL = imageEndpoint;
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
            return Url.ToLower().StartsWith("http://yfrog.com/");
        }

        public ImageResponse GetImage(string Identifier)
        {
            throw new NotImplementedException();
        }

        private static ImageResponse parseResponse(string response)
        {
            
            ImageResponse imageResponse = new ImageResponse();
            
            XmlDocument xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.LoadXml(response);
            }
            catch (Exception exp)
            {
                imageResponse.ErrorText = exp.Message;
                return imageResponse;
            }
            XmlNodeList rootElements = xmlDocument.GetElementsByTagName("rsp");
            if (rootElements.Count != 1)
            {
                imageResponse.ErrorText = "Invalid XML-Data received from service";
                return imageResponse;
            }
            else
            {
                string succcesCode;
                try
                {
                    succcesCode = rootElements[0].Attributes["stat"].InnerText;
                }
                catch (Exception exp)
                {
                    imageResponse.ErrorText = exp.Message;
                    return imageResponse;
                }
                if (succcesCode.ToLower() == "ok" || !rootElements[0].HasChildNodes)
                {
                    foreach (XmlNode node in rootElements[0].ChildNodes)
                    {
                        switch (node.Name.ToLower())
                        {
                            case "mediaid":
                                imageResponse.Id = node.InnerText;
                                imageResponse.UrlThumbnail = "http://yfrog.com/" + node.InnerText + ":medium";
                                break;
                            case "mediaurl":
                                imageResponse.UrlFull = node.InnerText;
                                break;
                        }
                        if (imageResponse.UrlFull != null && imageResponse.Id != null)
                        {
                            imageResponse.Success = true;
                        }
                        else
                        {
                            imageResponse.ErrorText = "Incomplete XML answer";
                        }
                    }
                }
                else
                {
                    imageResponse.ErrorText = "Service returned error";
                    try
                    {
                        XmlNodeList errorElements = xmlDocument.GetElementsByTagName("err");
                        imageResponse.ErrorText = errorElements[0].Attributes["msg"].InnerText;
                    }
                    catch
                    {
                        // error anyway...
                    }
                    return imageResponse;
                }
            }
            return imageResponse;
            /*
              Sample response:
                <?xml version="1.0" encoding="UTF-8"?>
                <rsp stat="ok">
                 <mediaid>abc123</mediaid>
                 <mediaurl>http://twitpic.com/abc123</mediaurl>
                </rsp>


                Sample error response:
                <?xml version="1.0" encoding="UTF-8"?>
                <rsp stat="fail">
                    <err code="1001" msg="Invalid twitter username or password" />
            */
        }


        public string Name
        {
            get
            {
                return "yfrog";
            }
        }


        public string GetMini(string Url)
        {
            return Url + ":medium";
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
            return Upload(Filepath, account as AccountTwitter);
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
