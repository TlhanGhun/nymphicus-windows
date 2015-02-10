using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.API.UrlShortener
{
    class googl : ILinkShortener
    {
        public string ShortenLink(string Url)
        {
            throw new NotImplementedException();
        }

        public bool IsLinkOfThisShortener(string Url)
        {
            throw new NotImplementedException();
        }

        public string ExpandLink(string Url)
        {
            throw new NotImplementedException();
        }

        public string ShortenAllLinksInText(string text)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get
            {
                return "goo.gl";
            }

        }

        private class googlImage
        {
            public string kind { get; set; }
            public string id { get; set; }
            public string longUrl { get; set; }
            public string status { get; set; }

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


        public bool CanShorten
        {
            get { return false; }
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
