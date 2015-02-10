//-----------------------------------------------------------------------
// <copyright file="IImageService.cs" company="lI' Ghun">
// 
//  Copyright (c) 2011, Sven Walther (sven@li-ghun.de)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Nymphicus nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Sven Walther</author>
// <summary>The interface for image upload services</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.Model;

namespace Nymphicus.API.ImageServices
{
    /// <summary>
    /// Interface for image services
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Indicates if this service has implemented the upload feature
        /// If not only GetMini and IsUrlFromThisService might be implemented
        /// </summary>
        bool CanUpload { get; }

        /// <summary>
        /// Name of the service
        /// </summary>
        string Name { get; }

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

        /// <summary>
        /// Upload an image with comment (some services need a comment)
        /// </summary>
        /// <param name="Filepath"></param>
        /// <param name="Description"></param>
        /// <param name="account"></param>
        /// <returns>All metadata of the upload image</returns>
        ImageResponse Upload(string Filepath, string Description, IAccount account);

        /// <summary>
        /// Upload an image
        /// </summary>
        /// <param name="Filepath"></param>
        /// <param name="account"></param>
        /// <returns>All metadata of the upload image</returns>
        ImageResponse Upload(string Filepath, IAccount account);        

        /// <summary>
        /// Checks if a given URL is from this service
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        Boolean IsUrlFromThisService(string Url);

        /// <summary>
        /// Gets the full image data of an image identifier
        /// </summary>
        /// <param name="Identifier"></param>
        /// <returns>All image metadata</returns>
        ImageResponse GetImage(string Identifier);

        /// <summary>
        /// Get a mini/thumbnail version of a given image
        /// </summary>
        /// <param name="Url"></param>
        /// <returns>Url to thumbnail image</returns>
        string GetMini(string Url);

        /// <summary>
        /// List of serives which can be used to authenticate
        /// </summary>
        List<CompatibleService> CompatibleServices { get; }
    }

    public enum CompatibleService
    {
        All,
        Twitter,
        AppNet,
        Facebook,
        GoogleReader,
        QUOTEfm
    }

    public class oAuthDelegateToken
    {
        public string delegate_token { get; set; }
    }
}
