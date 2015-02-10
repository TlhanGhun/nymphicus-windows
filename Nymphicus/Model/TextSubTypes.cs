using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Nymphicus.Model
{
    public class TextSubTypes
    {
        public interface ISubType
        {
            
        }

        public class Link : ISubType
        {
            public string urlLong { get; set; }
            public string urlShort { get; set; }

            public Link(string UrlLong, string UrlShort)
            {
                urlLong = UrlLong;
                urlShort = UrlShort;
            }
        }

        public class ImageLink : ISubType
        {
            public string urlLong { get; set; }
            public string urlShort { get; set; }
            public string imageUrl { get; set; }

            public ImageLink(string UrlLong, string UrlShort, string ImageUrl)
            {
                urlLong = UrlLong;
                urlShort = UrlShort;
                imageUrl = ImageUrl;
            }
        }

        public class Text : ISubType
        {
            public string text { get; set; }

            public Text(string Text)
            {
                text = Text;
            }
        }

        public class HashTag :ISubType
        {
            public string text { get; set; }

            public HashTag(string Text)
            {
                text = Text;
            }
        }

        public class User :ISubType
        {
            public string userName { get; set; }

            public User(string UserName)
            {
                userName = UserName;
            }
        }

    }
}
