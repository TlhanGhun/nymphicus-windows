using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.Model;
using Nymphicus.API;
using System.Xml;
using System.Text.RegularExpressions;

namespace Nymphicus.ExternalServices
{
    public class Twitlonger
    {
        private static string appName = ConnectionData.twitLongerAppName;
        private static string apiKey = ConnectionData.twitLongerApiKey;
        private static string apiEndpointSend = "http://www.twitlonger.com/api_post";
        private static string apiEndpointIdCallback = "http://www.twitlonger.com/api_set_id";

        public class TwitLongerResponse
        {
            public string MessageId { get; set; }
            public string MessageText { get; set; }
            public string Link { get; set; }
            public string ShortLink { get; set; }
            public string ErrorMessage { get; set; }
            public string User { get; set; }
        }

        public static TwitLongerResponse Send(string text, AccountTwitter account) {
            TwitLongerResponse twitLongerResponse = new TwitLongerResponse();
            twitLongerResponse.MessageText = text;
            API.WebHelpers.Response result = API.WebHelpers.SendPostRequest(apiEndpointSend, new
            {
                application = appName,
                api_key = apiKey,
                username = account.Login.Username,
                message = text
            }, false);
            if (string.IsNullOrEmpty(result.Content))
            {
                twitLongerResponse.ErrorMessage = "Empty reply message";
            }
            else
            {
                XmlDocument xmlDocument = new XmlDocument();

                try
                {
                    xmlDocument.LoadXml(result.Content);
                }
                catch
                {
                    twitLongerResponse.ErrorMessage = "Malformed XML received";
                    return twitLongerResponse;
                }
                XmlNodeList rootElements = xmlDocument.GetElementsByTagName("twitlonger");
                if (rootElements.Count != 1)
                {
                    twitLongerResponse.ErrorMessage = "Invalid XML-Data received from service - wrong root element";
                    return twitLongerResponse;
                }
                else
                {
                    if (rootElements[0].HasChildNodes)
                    {
                        foreach (XmlNode child in rootElements[0].ChildNodes)
                        {
                            if (child.Name.ToLower() == "post")
                            {
                                foreach (XmlNode node in child.ChildNodes)
                                {
                                    switch (node.Name.ToLower())
                                    {
                                        case "error":
                                            twitLongerResponse.ErrorMessage = node.InnerText;
                                            twitLongerResponse.MessageText = text;
                                            return twitLongerResponse;

                                        case "id":
                                            twitLongerResponse.MessageId = node.InnerText;
                                            break;
                                        case "link":
                                            twitLongerResponse.Link = node.InnerText;
                                            break;
                                        case "short":
                                            twitLongerResponse.ShortLink = node.InnerText;
                                            break;
                                        case "content":
                                            twitLongerResponse.MessageText = node.InnerText;
                                            break;
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(twitLongerResponse.MessageId) || string.IsNullOrEmpty(twitLongerResponse.MessageText))
                        {
                            twitLongerResponse.MessageText = text;
                            twitLongerResponse.ErrorMessage = "Incomplete answer";
                            return twitLongerResponse;
                        }
                             
                    }
                    else
                    {
                        twitLongerResponse.ErrorMessage = "Invalid XML-Data received from service - missing child note";
                        return twitLongerResponse;
                    }
                }
            }

            return twitLongerResponse;
        }

        public static bool IsTwitLongerText(string text)
        {
            string[] separator = { " " };
            string[] parts = text.Split(separator,StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return false;
            }
            else
            {
                if (parts[parts.Length - 2] == "(cont)" && (parts[parts.Length - 1].StartsWith("http://tl.gd/") || parts[parts.Length - 1].StartsWith("http://www.twitlonger.com/show/")))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void IdCallback(decimal twitterId, string twitLongerId)
        {
            API.WebHelpers.Response result = API.WebHelpers.SendPostRequest(apiEndpointIdCallback, new
            {
                application = appName,
                api_key = apiKey,
                message_id = twitLongerId,
                twitter_id = twitterId
            }, false);
        }

        public static TwitLongerResponse GetLongText(string text)
        {
            try
            {
                TwitLongerResponse twitLongerResponse = new TwitLongerResponse();
                twitLongerResponse.MessageText = text;

                string[] separator = { " " };
                string[] parts = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    return twitLongerResponse;
                }
                else
                {
                    if (parts[parts.Length - 2] == "(cont)" && (parts[parts.Length - 1].StartsWith("http://tl.gd/") || parts[parts.Length - 1].StartsWith("http://www.twitlonger.com/show/")))
                    {
                        string messageId = parts[parts.Length - 1].Substring(13);
                        if (messageId.Contains("/"))
                        {
                            int positionOfSlash = messageId.LastIndexOf("/");
                            if (text.Length > positionOfSlash + 1)
                            {
                                messageId = messageId.Substring(positionOfSlash + 1);
                            }
                        }
                        API.WebHelpers.Response result = API.WebHelpers.SendPostRequest("http://www.twitlonger.com/api_read/" + messageId, new
                        {
                            application = appName,
                            api_key = apiKey,
                        }, false);
                        if (string.IsNullOrEmpty(result.Content))
                        {
                            twitLongerResponse.ErrorMessage = "Empty reply message";
                        }
                        else
                        {
                            XmlDocument xmlDocument = new XmlDocument();

                            try
                            {
                                xmlDocument.LoadXml(result.Content);
                            }
                            catch
                            {
                                twitLongerResponse.ErrorMessage = "Malformed XML received";
                                return twitLongerResponse;
                            }
                            XmlNodeList rootElements = xmlDocument.GetElementsByTagName("twitlonger");
                            if (rootElements.Count != 1)
                            {
                                twitLongerResponse.ErrorMessage = "Invalid XML-Data received from service - wrong root element";
                                return twitLongerResponse;
                            }
                            else
                            {
                                if (rootElements[0].HasChildNodes)
                                {
                                    foreach (XmlNode child in rootElements[0].ChildNodes)
                                    {
                                        if (child.Name.ToLower() == "post")
                                        {
                                            foreach (XmlNode node in child.ChildNodes)
                                            {
                                                switch (node.Name.ToLower())
                                                {
                                                    case "error":
                                                        twitLongerResponse.ErrorMessage = node.InnerText;
                                                        twitLongerResponse.MessageText = text;
                                                        return twitLongerResponse;

                                                    case "id":
                                                        twitLongerResponse.MessageId = node.InnerText;
                                                        break;
                                                    case "link":
                                                        twitLongerResponse.Link = node.InnerText;
                                                        break;
                                                    case "user":
                                                        twitLongerResponse.User = node.InnerText;
                                                        break;
                                                    case "content":
                                                        twitLongerResponse.MessageText = node.InnerText;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    if (string.IsNullOrEmpty(twitLongerResponse.MessageId) || string.IsNullOrEmpty(twitLongerResponse.MessageText))
                                    {
                                        twitLongerResponse.MessageText = text;
                                        twitLongerResponse.ErrorMessage = "Incomplete answer";
                                        return twitLongerResponse;
                                    }

                                }
                                else
                                {
                                    twitLongerResponse.ErrorMessage = "Invalid XML-Data received from service - missing child note";
                                    return twitLongerResponse;
                                }
                            }
                        }

                        return twitLongerResponse;


                    }
                    else
                    {
                        return twitLongerResponse;
                    }
                }
            }
            catch (Exception exp)
            {
                ExternalServices.Twitlonger.TwitLongerResponse response = new TwitLongerResponse();
                response.MessageText = text;
                response.ErrorMessage = exp.Message;

                return response;
            }
        }
    }
}
