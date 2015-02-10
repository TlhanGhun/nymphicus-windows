using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Nymphicus.API.UrlShortener
{
    class UnshortMe : ILinkShortener
    {
        public string Name
        {
            get { return "UnshortMe"; }
        }

        public bool CanShorten
        {
            get { return false; }
        }

        public bool CanHaveCredentials
        {
            get { return false; }
        }

        public bool NeedsCredentials
        {
            get { return false; }
        }

        public string Login
        {
            get
            {
                return "";
            }
            set
            {
                
            }
        }

        public string Password
        {
            get
            {
                return "";
            }
            set
            {
                
            }
        }

        public string ShortenLink(string Url)
        {
            return Url;
        }

        public bool IsLinkOfThisShortener(string Url)
        {
            return true;
        }

        public string ExpandLink(string Url)
        {
           // AppController.Current.Logger.writeToLogfile("unshort.me called for " + Url);
            // return empty strings if not valid
            if (string.IsNullOrEmpty(Url))
            {
                AppController.Current.Logger.writeToLogfile("Empty link");
                return "";
            }

            string unshortMeUrl = "http://api.unshort.me/?r=" + Url + "&t=json";

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(unshortMeUrl);
            request.UserAgent = "Nymphicus for Windows " + Converter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) + " (http://www.li-ghun.de/Nymphicus)";
            request.Timeout = 2000;
            request.Method = "POST";
            string strResult = null;
            try
            {
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    strResult = reader.ReadToEnd();
                }
            }
            catch 
            {
                AppController.Current.Logger.writeToLogfile("Unshort.me failed with exception for " + Url);
                return Url; // eat it and return original url
            }

            UnshortMeResponse jsonRepsone;
            try
            {
                jsonRepsone = JsonConvert.DeserializeObject<UnshortMeResponse>(strResult);
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Unshort.me failed with exception in JSON parser for " + Url);
                AppController.Current.Logger.writeToLogfile(exp);
                return Url; // eat it and return original url
            }

            if (!jsonRepsone.success || string.IsNullOrEmpty(jsonRepsone.resolvedURL))
            {
               // AppController.Current.Logger.writeToLogfile("Unshort.me failed with empty resolvedUrl or success = false for " + Url);
                return Url;
            }
            else
            {
                //AppController.Current.Logger.writeToLogfile("Found expanded link for " + Url + ": " + jsonRepsone.resolvedURL);
                try
                {
                    AppController.Current.AllShortenedLinksInItems.Add(Url, jsonRepsone.resolvedURL);
                }
                catch
                {
                    // as we are asynchron there might be another thread having added it already...
                }
                return jsonRepsone.resolvedURL;
            }
        }

        public string ShortenAllLinksInText(string text)
        {
            return text;
        }

        private class UnshortMeResponse
        {
            public string requestedURL { get; set; }
            public bool success { get; set; }
            public string resolvedURL { get; set; }
        }


        public bool CanValidateCredentials
        {
            get { return false; }
        }

        public bool ValidateCredentials()
        {
            throw new NotImplementedException();
        }
    }
}
