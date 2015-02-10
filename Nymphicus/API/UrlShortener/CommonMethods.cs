using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.IO;

namespace Nymphicus.API.UrlShortener
{
    public class CommonMethods
    {
        public static string ShortenAllLinksInText(string text, ILinkShortener shortener)
        {
            
            string[] words = Regex.Split(text, @"([\r\n \(\)\{\}\[\]])");

            //xxx das geht doch besser...
            foreach (string word in words)
            {
                if (word.ToLower().StartsWith("http://") || word.ToLower().StartsWith("https://"))
                {
                    //xxx check for images
                    text = text.Replace(word, shortener.ShortenLink(word));
                }
            }
            return text;
        }

        public static string ShortenLinkSimple(string Url, string RequestUrl)
        {
            // return empty strings if not valid
            if (!IsValidURL(Url))
            {
                return "";
            }

            WebRequest request = HttpWebRequest.Create(RequestUrl);
            request.Proxy = null;
            string strResult = null;
            try
            {
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.ASCII);
                    strResult = reader.ReadToEnd();
                    if (!IsValidURL(strResult))
                    {
                        WebException w = new WebException(strResult);

                        throw w;
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                return Url; // eat it and return original url
            }

            // if converted is longer than original, return original
            if (strResult.Length > Url.Length)
            {
                strResult = Url.Trim();
            }

            return strResult;
        }

        public static string ExpandLinkSimple(string Url, string RequestUrl)
        {
            // return empty strings if not valid
            if (!IsValidURL(Url))
            {
                return "";
            }

            WebRequest request = HttpWebRequest.Create(RequestUrl);
            request.Proxy = null;
            string strResult = null;
            try
            {
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.ASCII);
                    strResult = reader.ReadToEnd();
                    if (!IsValidURL(strResult))
                    {
                        WebException w = new WebException(strResult);

                        throw w;
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                return Url; // eat it and return original url
            }

            return strResult.Trim();
        }

        public static bool IsValidURL(string strurl)
        {
            // Validate the URL
            if (true == strurl.ToLower().StartsWith("http://") || true == strurl.StartsWith("https://"))
            {
                return true;
            }
            return false;
        }
    }
}
