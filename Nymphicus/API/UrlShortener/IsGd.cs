using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.API.UrlShortener
{
    class IsGd : ILinkShortener
    {
        public string ShortenLink(string Url)
        {
            return CommonMethods.ShortenLinkSimple(Url, string.Format("http://is.gd/api.php?longurl={0}", Url));
        }

        public bool IsLinkOfThisShortener(string Url)
        {
            return Url.ToLower().StartsWith("http:/is.gd/");
        }

        public string ExpandLink(string Url)
        {
            return CommonMethods.ExpandLinkSimple(Url, string.Format("http://is.gd/forward.php?shorturl={0}", Url));
        }

        public string ShortenAllLinksInText(string text)
        {
            return CommonMethods.ShortenAllLinksInText(text, this);
        }

        public string Name
        {
            get
            {
                return "is.gd";
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
