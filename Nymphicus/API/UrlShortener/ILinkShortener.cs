using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.API.UrlShortener
{
    public interface ILinkShortener
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        string Name {get;}

        bool CanShorten { get; }

        /// <summary>
        /// Defines that this service has the option to login
        /// </summary>
        bool CanHaveCredentials { get; }

        /// <summary>
        /// Defines if this service needs Credentials
        /// </summary>
        bool NeedsCredentials { get; }

        /// <summary>
        /// Login username
        /// </summary>
        string Login { get; set; }

        /// <summary>
        /// Login password
        /// </summary>
        string Password { get; set; }

        bool CanValidateCredentials { get; }

        /// <summary>
        /// Checks if the given username and password are vaild
        /// </summary>
        /// <returns>true/false</returns>
        bool ValidateCredentials();

        /// <summary>
        /// shorten the given long link
        /// </summary>
        /// <param name="Url">the long link to be shortened</param>
        /// <returns>The shortened link (if shorter than original one)</returns>
        string ShortenLink(string Url);

        /// <summary>
        /// Returns true if given URL is from this shortener
        /// </summary>
        /// <param name="Url">the to be checked url</param>
        /// <returns></returns>
        bool IsLinkOfThisShortener(string Url);

        /// <summary>
        /// Expand a given short link
        /// </summary>
        /// <param name="Url">the short link</param>
        /// <returns>the long link (short link on fail)</returns>
        string ExpandLink(string Url);

        /// <summary>
        /// Searches all links in a text an shortens them using this service
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string ShortenAllLinksInText(string text);
    }
}
