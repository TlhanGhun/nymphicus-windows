using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;

namespace Nymphicus.API.UrlShortener
{
    class TinyUrl : ILinkShortener
    {
        public string ShortenLink(string Url)
        {
            return CommonMethods.ShortenLinkSimple(Url, string.Format("http://tinyurl.com/api-create.php?url={0}", Url));
        }

        public bool IsLinkOfThisShortener(string Url)
        {
            return Url.ToLower().StartsWith("http://tinyurl.com/");
        }

        public string ExpandLink(string Url)
        {
            WebRequest request = HttpWebRequest.Create(Url);
            request.Method = "HEAD";
            string strResult = Url;
            try
            {
                WebResponse response = request.GetResponse();


                if (response.ResponseUri != null)
                {
                    if (CommonMethods.IsValidURL(response.ResponseUri.AbsoluteUri))
                    {
                        strResult = response.ResponseUri.AbsoluteUri;
                    }
                }
                    
            }
            catch
            {
                // geben wir halt das Orignal zurück - basta
            }
            return strResult;
        }

        public string ShortenAllLinksInText(string text)
        {
            return CommonMethods.ShortenAllLinksInText(text, this);
        }

        public string Name
        {
            get
            {
                return "TinyUrl";
            }
        }


        public bool CanHaveCredentials
        {
            get { return false; }
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




        public bool CanShorten
        {
            get { return true; }
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
