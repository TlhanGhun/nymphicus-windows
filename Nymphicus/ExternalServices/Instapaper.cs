using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.ExternalServices
{
    public class Instapaper : IStore
    {
        public string Name
        {
            get { return "Instapaper"; }
        }

        public string Description
        {
            get { return "Instapaper saves webpages for reading them later"; }
        }

        public string Homepage
        {
            get { return "http://www.instapaper.com/"; }
        }

        public bool UsesOAuth
        {
            get { return false; }
        }

        public string Username
        {
            get
            {
                return Properties.Settings.Default.ExtSrvInstapaperUsername;
            }

            set
            {
                Properties.Settings.Default.ExtSrvInstapaperUsername = value;
            }
        }

        public string Password
        {
            get
            {
                return Nymphicus.Crypto.ToInsecureString(Nymphicus.Crypto.DecryptString(Properties.Settings.Default.ExtSrvInstapaperPassword));
            }
            set
            {
                Properties.Settings.Default.ExtSrvInstapaperPassword = Crypto.EncryptString(Crypto.ToSecureString(value));
            }
        }

        public bool PasswordVerified
        {
            get;set;
        }

        public string LastError
        {
            get { return ""; }
        }

        public string ServiceIcon
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), "Images", "ExternalServices", "instapaper.png").Substring(6);
            }
        }

        public string ServiceIconRelativePath
        {
            get
            {
                return "Images\\ExternalServices\\instapaer.png";
            }
        }

        public bool CanVerifyCredentials { get { return true; } }

        public bool VerifyCredentials()
        {
            if (!string.IsNullOrEmpty(this.Username) && !string.IsNullOrEmpty(this.Password))
            {
                API.WebHelpers.Response result = API.WebHelpers.SendPostRequest(@"https://www.instapaper.com/api/authenticate", new
                {
                    username = Username,
                    password = Password
                }, false);
                
                bool success = (!string.IsNullOrEmpty(result.Content) && result.Content.ToLowerInvariant().StartsWith("200"));
                if (!success)
                {
                    System.Windows.Forms.MessageBox.Show("Invalid username or password", "Login to Instapaper failed");
                }
                return success;
            }
            else
            {
                PasswordVerified = false;
                return false;
            }
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
                    result = API.WebHelpers.SendPostRequest(@"https://www.instapaper.com/api/add", new
                    {
                        username = this.Username,
                        password = Password,
                        url = url,
                        title = url

                    }, false);
                }
                else
                {
                    result = API.WebHelpers.SendPostRequest(@"https://www.instapaper.com/api/add", new
                    {
                        username = this.Username,
                        password = Password,
                        url = url,
                        title = item.Text

                    }, false);
                }
                bool success = (!string.IsNullOrEmpty(result.Content) && result.Content.ToLowerInvariant().StartsWith("201"));
                if (!success)
                {
                    System.Windows.Forms.MessageBox.Show("Error sending item to Instapaper", "Instapaper error");
                }
                return success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Login to Instapaper failed");
                Nymphicus.UserInterface.Preferences preferences = new UserInterface.Preferences();
                preferences.tabExternalServices.IsSelected = true;
                preferences.Show();
                PasswordVerified = false;
                return false;
            }
        }
    }
}
