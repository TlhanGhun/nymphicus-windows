using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.API;

namespace Nymphicus.ExternalServices
{
    public class Pocket : IStore
    {
        private  string ApiKey = ConnectionData.pocketApiKey;

        private string lastError
        {
            get
            {
                return lastError;
            }
        }

        public string Name
        {
            get
            {
                return "Pocket";
            }

        }

        public string Description
        {
            get
            {
                return "Pocket is a service to remember websites. See http://getpocket.com/";
            }
        }

        public string Homepage
        {
            get
            {
                return "http://getpocket.com/";
            }
        }

        public string ServiceIcon 
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), "Images", "ExternalServices", "pocket.png").Substring(6);
            }
        }

        public string ServiceIconRelativePath
        {
            get
            {
                return "Images\\ExternalServices\\pocket.png";
            }
        }

        public bool UsesOAuth
        {
            get
            {
                return false;
            }
        }

        public string Username
        {
            get
            {
                return Properties.Settings.Default.ExtSrvRiLUsername;
            }

            set
            {
                Properties.Settings.Default.ExtSrvRiLUsername = value;
            }
        }

        public string Password
        {
            get
            {
                return Nymphicus.Crypto.ToInsecureString(Nymphicus.Crypto.DecryptString(Properties.Settings.Default.ExtSrvRiLPassword));
            }
            set
            {
                Properties.Settings.Default.ExtSrvRiLPassword = Crypto.EncryptString(Crypto.ToSecureString(value));
            }
        }

        public bool PasswordVerified { get; set; }

        public string LastError
        {
            get
            {
                return lastError;
            }
        }

        public bool CanVerifyCredentials { get { return true; } }

        public bool VerifyCredentials()
        {
            if (this.Username != "" && this.Password != "")
            {
                API.WebHelpers.Response result = API.WebHelpers.SendPostRequest(@"https://readitlaterlist.com/v2/auth", new
                {
                    username = this.Username,
                    password = this.Password,
                    apikey = ApiKey,

                }, false);
                result.Success = (!string.IsNullOrEmpty(result.Content) && result.Content.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Pocket failed");
                    PasswordVerified = false;
                    return false;
                }
                PasswordVerified = true;
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Login to Pocket failed");
                PasswordVerified = false;
                return false;
            }
        }

        public bool SendNow(Model.IItem item, string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) && item != null)
                {
                    url = "http://twitter.com/#!/" + AppController.Current.getAccountForId(item.accountId).Login.Username + "/statuses/" + item.Id.ToString();
                }
                if (this.Password != "" && this.Username != "")
                {
                    API.WebHelpers.Response result;
                    if (item != null)
                    {
                        result = API.WebHelpers.SendPostRequest(@"https://readitlaterlist.com/v2/add", new
                        {
                            username = this.Username,
                            password = this.Password,
                            apikey = ApiKey,
                            url = url,
                            title = item.Text,
                            ref_id = item.Id

                        }, false);
                    }
                    else
                    {
                        result = API.WebHelpers.SendPostRequest(@"https://readitlaterlist.com/v2/add", new
                        {
                            username = this.Username,
                            password = this.Password,
                            apikey = ApiKey,
                            url = url,

                        }, false);
                    }
                    result.Success = (!string.IsNullOrEmpty(result.Content) && result.Content.ToLowerInvariant() == "200 ok");
                    if (!result.Success)
                    {
                        System.Windows.Forms.MessageBox.Show("Error sending item to Pocket", result.Error);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Missing username or password", "Login to Pocket failed");
                    Nymphicus.UserInterface.Preferences preferences = new UserInterface.Preferences();
                    preferences.tabExternalServices.IsSelected = true;
                    preferences.Show();
                    PasswordVerified = false;
                    return false;
                }
            }

            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show("Error sending item to Pocket", exp.Message);
                AppController.Current.Logger.writeToLogfile("Sending to Pocket failed with exception: " + exp.Message);
                return false;
            }
        }
    }
}
