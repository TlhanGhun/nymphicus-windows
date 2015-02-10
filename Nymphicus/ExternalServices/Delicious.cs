using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.ExternalServices
{
    public class Delicious : IStore
    {
        public string Name
        {
            get { return "del.icio.us"; }
        }

        public string Description
        {
            get { return "Delicious is a Social Bookmarking service, which means you can save all your bookmarks online, share them with other people, and see what other people are bookmarking."; }
        }

        public string Homepage
        {
            get { return "http://del.icio.us/"; }
        }

        public bool UsesOAuth
        {
            get { return false; }
        }

        public string Username
        {
            get
            {
                return Properties.Settings.Default.ExtSrvDeliciousUsername;
            }

            set
            {
                Properties.Settings.Default.ExtSrvDeliciousUsername = value;
            }
        }

        public string Password
        {
            get
            {
                return Nymphicus.Crypto.ToInsecureString(Nymphicus.Crypto.DecryptString(Properties.Settings.Default.ExtSrvDeliciousPassword));
            }
            set
            {
                Properties.Settings.Default.ExtSrvDeliciousPassword = Crypto.EncryptString(Crypto.ToSecureString(value));
            }
        }

        public bool PasswordVerified
        {
            get;
            set;
        }

        public string LastError
        {
            get { return ""; }
        }

        public string ServiceIcon
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), "Images", "ExternalServices", "delicious.png").Substring(6);
            }
        }

        public string ServiceIconRelativePath
        {
            get
            {
                return "Images\\ExternalServices\\delicious.png";
            }
        }

        public bool CanVerifyCredentials { get { return false; } }

        public bool VerifyCredentials()
        {
            throw new NotImplementedException();
        }

        public bool SendNow(Model.IItem item, string url)
        {
            if (string.IsNullOrEmpty(url) && item != null)
            {
                url = "http://twitter.com/#!/" + AppController.Current.getAccountForId(item.accountId).Login.Username + "/statuses/" + item.Id.ToString();
            }

            if (!string.IsNullOrEmpty(this.Username) && !string.IsNullOrEmpty(this.Password))
            {
                API.WebHelpers.Response result;
                if (item == null)
                {
                    result = API.WebHelpers.SendPostRequestWithBasicAuth(@"https://api.del.icio.us/v1/posts/add", new
                    {
                        url = url,
                        description = url,
                        title = url

                    },
                    this.Username,
                    this.Password,
                    false);
                }
                else
                {
                    result = API.WebHelpers.SendPostRequestWithBasicAuth(@"https://api.del.icio.us/v1/posts/add", new
                    {
                        url = url,
                        description = url,
                        title = item.Text
                    },
                    this.Username,
                    this.Password,
                    false);
                }

                bool success = (!string.IsNullOrEmpty(result.Content) && result.Content.ToLowerInvariant().Contains("<result code=\"done\" />"));
                if (!success)
                {
                    System.Windows.Forms.MessageBox.Show("Error sending item to del.icio.us", "del.icio.us error");
                }
                return success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Login to del.icio.us failed");
                Nymphicus.UserInterface.Preferences preferences = new UserInterface.Preferences();
                preferences.tabExternalServices.IsSelected = true;
                preferences.Show();
                PasswordVerified = false;
                return false;
            }
        }
    }
}
