using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Dynamic;
using Facebook;
using Nymphicus.Model;

namespace Nymphicus.API.FacebookAPI
{
    /// <summary>
    /// Interaction logic for AuthorizeNewFacebookAccount.xaml
    /// </summary>
    public partial class AuthorizeNewFacebookAccount : Window
    {
        
   private Uri loginUri { get; set; }
        private string appId = "210081815685753";
        private string appSecret = "1dd70762e9b2290b736fabed4349163f";

        protected FacebookClient _fb;

        public FacebookOAuthResult facebookOAuthResult { get; set; }

        #region Permissions
        private string[] _extendedPermissions = new[] { 
             "user_activities",
             "user_birthday",
             "user_checkins",
             "user_education_history",
             "user_events",
             "user_games_activity",		 
             "user_groups",
             "user_hometown",
             "user_interests",
             "user_likes",
             "user_location",
             "user_notes",
             "user_online_presence",
             "user_photo_video_tags",
             "user_photos",
             "user_questions",
             "user_relationship_details",
             "user_relationships",
             "user_religion_politics",
             "user_status",
             "user_subscriptions",
             "user_videos",
             "user_website",
             "user_work_history",

             
             "friends_about_me",
             "friends_activities",
             "friends_birthday",
             "friends_checkins",
             "friends_education_history",
             "friends_events",
             "friends_games_activity",
             "friends_groups",
             "friends_hometown",
             "friends_interests",
             "friends_likes",
             "friends_location",
             "friends_notes",
             "friends_online_presence",
             "friends_photo_video_tags",
             "friends_photos",
             "friends_questions",
             "friends_relationship_details",
             "friends_relationships",
             "friends_religion_politics",
             "friends_status",
             "friends_subscriptions",
             "friends_videos",
             "friends_website",
             "friends_work_history",

            "create_event",
            "create_note",
            "email",
            "export_stream",
            "manage_friendlists",
            "manage_notifications",
            "manage_pages",
            "offline_access",
            "photo_upload",
            "publish_actions",
            "publish_checkins",
            "publish_stream",
            "read_friendlists",
            "read_insights",
            "read_mailbox",
            "read_requests",
            "read_stream",
            "rsvp_event",
            "share_item",
            "status_update",
            "video_upload",
        };
#endregion


        public AuthorizeNewFacebookAccount()
        {
            InitializeComponent();
            this.Opacity = 0.00;
            this.WindowState = System.Windows.WindowState.Minimized;

            _fb = new FacebookClient();

            loginUri = GenerateLoginUrl(appId, string.Join(" ",_extendedPermissions));
           // webBrowserOAuth.Navigated += new NavigatedEventHandler(webBrowserOAuth_Navigated);

            // reset of cookies to be able to have multiple accounts
            
           
            webBrowserOAuth.Navigate("javascript:void((function(){var a,b,c,e,f;f=0;a=document.cookie.split('; ');for(e=0;e<a.length&&a[e];e++){f++;for(b='.'+location.host;b;b=b.replace(/^(?:%5C.|[^%5C.]+)/,'')){for(c=location.pathname;c;c=c.replace(/.$/,'')){document.cookie=(a[e]+'; domain='+b+'; path='+c+'; expires='+new Date((new Date()).getTime()-1e11).toGMTString());}}}})())");

            webBrowserOAuth.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(webBrowserOAuth_Navigated);
            System.Threading.Thread.Sleep(500);

            webBrowserOAuth.Navigate(loginUri);
        }

        
        void webBrowserOAuth_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            if (_fb.TryParseOAuthCallbackUrl(e.Url, out oauthResult))
            {
                if (oauthResult.IsSuccess)
                {
                    AccountFacebook fbAccount = new AccountFacebook();
                    fbAccount.AccessToken = oauthResult.AccessToken;
                    fbAccount.verifyCredentials();
                    if (AppController.Current.AllFacebookAccounts.Where(acc => acc.Id == fbAccount.Id).Count() > 0)
                    {
                        MessageBox.Show("This account already has been authorized with Nymphicus successfully. On some systems there is an auto log in active which prevents Nymphicus from asking about other credentials. Please open Internet Explorer and log off there from Facebook- then try again. This window will be closed now", "Account already authorized", MessageBoxButton.OK);
                        Close();
                        return;
                            
                    }
                    facebookOAuthResult = oauthResult;
                    fbAccount.TokenExpiresAt = oauthResult.Expires;
                    AppController.Current.AllAccounts.Add(fbAccount);
                    Close();
                    return;
                }
                this.WindowState = System.Windows.WindowState.Normal;
                Show();

            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
                Show();
                oauthResult = null;
                facebookOAuthResult = oauthResult;
            }
        }

        private Uri GenerateLoginUrl(string appId, string extendedPermissions)
        {
            dynamic parameters = new ExpandoObject();
            parameters.client_id = appId;
            parameters.redirect_uri = "https://www.facebook.com/connect/login_success.html";

            // The requested response: an access token (token), an authorization code (code), or both (code token).
            parameters.response_type = "token";

            // list of additional display modes can be found at http://developers.facebook.com/docs/reference/dialogs/#display
            parameters.display = "popup";

            // add the 'scope' parameter only if we have extendedPermissions.
            if (!string.IsNullOrWhiteSpace(extendedPermissions))
                parameters.scope = extendedPermissions;

            // when the Form is loaded navigate to the login url.
            return _fb.GetLoginUrl(parameters);
        }

    }
    
}
