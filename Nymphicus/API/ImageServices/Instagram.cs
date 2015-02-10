using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.API.ImageServices
{
    class Instagram : IImageService {

        private List<CompatibleService> services;

        public Instagram()
        {
            services = new List<CompatibleService>();
            services.Add(CompatibleService.All);
        }


    
        public bool CanUpload
        {
            get
            {
                return false;
            }
        }

        public ImageResponse Upload(string Filepath, string Description, Model.AccountTwitter account)
        {
            throw new NotImplementedException();
        }

        public ImageResponse Upload(string Filepath, Model.AccountTwitter account)
        {
            throw new NotImplementedException();
        }

        public bool IsUrlFromThisService(string Url)
        {
            return Url.ToLower().StartsWith("http://instagr.am/p/");
        }

        public ImageResponse GetImage(string Identifier)
        {
            throw new NotImplementedException();
        }

        public string GetMini(string Url)
        {
            return Url + "media/?size=m";
        }

        public string Name
        {
            get
            {
                return "instagr.am";
            }
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


        public ImageResponse Upload(string Filepath, string Description, Model.IAccount account)
        {
            throw new NotImplementedException();
        }

        public List<CompatibleService> CompatibleServices
        {
            get { return services; }
        }


        public ImageResponse Upload(string Filepath, Model.IAccount account)
        {
            return Upload(Filepath, "", account);
        }
    }
}
