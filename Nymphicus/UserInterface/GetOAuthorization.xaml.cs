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
using System.Reflection;
using TweetSharp;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for GetOAuthorization.xaml
    /// </summary>
    public partial class GetOAuthorization : Window
    {
        
        private string originalUri = "";
        public Model.AccountTwitter account;
        private OAuthRequestToken requestToken;

        public GetOAuthorization()
        {
            InitializeComponent();

            account = new Model.AccountTwitter();
            requestToken = account.twitterService.GetRequestToken("oob");
            string authUrl = account.twitterService.GetAuthorizationUri(requestToken).AbsoluteUri;

            originalUri = authUrl;
            webBrowser1.Navigated += new System.Windows.Navigation.NavigatedEventHandler(webBrowser1_Navigated);            
            webBrowser1.Navigate(authUrl);
        }

        void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            HideScriptErrors(webBrowser1, true);
            if (e.Uri.AbsoluteUri.ToString() != originalUri)
            {
                textBoxOauthPIN.Visibility = System.Windows.Visibility.Visible;
                buttonFinish.Visibility = System.Windows.Visibility.Visible;
                labelPleaseEnterPin.Visibility = System.Windows.Visibility.Visible;
                imageGoNextArrow.Visibility = System.Windows.Visibility.Visible;
                BorderPinEntry.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public event AuthSuccessEventHandler AuthSuccess;
        public delegate void AuthSuccessEventHandler(object sender, AuthSuccessEventArgs e);
        public class AuthSuccessEventArgs : EventArgs
        {
            public string verifyKey
            {
                get;
                set;
            }
            public OAuthRequestToken requestToken { get; set; }
        }

        public void HideScriptErrors(WebBrowser wb, bool Hide) {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic); 
            if (fiComWebBrowser == null) return; 
            object objComWebBrowser = fiComWebBrowser.GetValue(wb); 
            if (objComWebBrowser == null) return; 
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide }); 
        }  

        private void buttonFinish_Click(object sender, RoutedEventArgs e)
        {
            AuthSuccessEventArgs eventArgs = new AuthSuccessEventArgs();
            eventArgs.verifyKey = textBoxOauthPIN.Text.Trim();
            eventArgs.requestToken = requestToken;
            AuthSuccess(this, eventArgs);
            Close();
        }
    }
}
