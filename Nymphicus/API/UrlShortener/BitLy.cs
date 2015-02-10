using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;


namespace Nymphicus.API.UrlShortener
{
    public class BitLy : ILinkShortener
    {
        public string ShortenLink(string Url)
        {
            string returnValue = CommonMethods.ShortenLinkSimple(Url, string.Format("http://api.bitly.com/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt",Login,Password, Url)).Trim();
            if (!returnValue.StartsWith("http"))
            {
                if (!string.IsNullOrWhiteSpace(returnValue))
                {
                    System.Windows.MessageBox.Show(returnValue);
                }
                return Url;
            }

            return returnValue;
        }

        public bool IsLinkOfThisShortener(string Url)
        {
            return (Url.ToLower().StartsWith("http://bit.ly/") || Url.ToLower().StartsWith("https://bit.ly/"));
        }

        public string ExpandLink(string Url)
        {
            return CommonMethods.ExpandLinkSimple(Url, string.Format("https://api.bitly.com/v3/expand?login={0}&apiKey={1}&shortUrl={2}&format=txt", Login,Password, Url));
        }

        public string ShortenAllLinksInText(string text)
        {
            return CommonMethods.ShortenAllLinksInText(text, this);
        }

        public string Name
        {
            get
            {
                return "bit.ly";
            }
        }

        public bool ValidateCredentials()
        {
            WebHelpers.Response respone =  WebHelpers.SendGetRequest(string.Format("https://api.bitly.com/v3/validate/?x_login=notLogin&x_apiKey=not_apikey&apiKey={1}&login={0}&format=txt",Login,Password));
            if (respone.Success)
            {
                if (respone.Content.Trim() == "0")
                {
                    return true;
                }
            }
            return false; ;
        }

        public bool CanHaveCredentials
        {
            get { return true; }
        }

        public bool NeedsCredentials
        {
            get { return true; }
        }

        public string Login
        {
            get
            {
                return Properties.Settings.Default.UrlShortenerLoginBitLy;
            }
            set
            {
                Properties.Settings.Default.UrlShortenerLoginBitLy = value;
            }
        }

        public string Password
        {
            get
            {
                return Properties.Settings.Default.UrlShortenerPasswordBitLy;
            }
            set
            {
                Properties.Settings.Default.UrlShortenerPasswordBitLy = value;
            }
        }


        public bool CanShorten
        {
            get { return true; }
        }


        public bool CanValidateCredentials
        {
            get { return true; }
        }
    }
}
