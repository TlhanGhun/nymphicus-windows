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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Nymphicus.Model;
using TweetSharp;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for FirstStartWizard.xaml
    /// </summary>
    public partial class FirstStartWizard : Window
    {

        private OAuthRequestToken oAuthRequestToken;

        enum WizardStep
        {
            Welcome,
            InitiateOAuth,
            CreateView,
            Finish
        }

        private WizardStep CurrentStep { get; set; }

        private Button buttonAuthorize;
        private Button buttonCreateView;

        public FirstStartWizard()
        {
            InitializeComponent();

            CurrentStep = WizardStep.Welcome;

            AppController.Current.AllAccounts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllAccounts_CollectionChanged);
        }

        void AllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AppController.Current.AllAccounts.Count > 0)
            {
                buttonNext.IsEnabled = true;                
            }
            else
            {
                buttonNext.IsEnabled = false;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://twitter.com/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void checkBoxShowProxy_Checked(object sender, RoutedEventArgs e)
        {
            proxySettings1.Visibility = Visibility.Visible;
        }

        private void checkBoxShowProxy_Unchecked(object sender, RoutedEventArgs e)
        {
            proxySettings1.Visibility = Visibility.Collapsed;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("Next button pressed", CurrentStep.ToString(), type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            switch (CurrentStep) {
                case WizardStep.Welcome:
                    CurrentStep = WizardStep.InitiateOAuth;
                    AppController.Current.ApplyProxySettings();
                    generateOAuthSubpage();
                    break;

                case WizardStep.InitiateOAuth:
                    CurrentStep = WizardStep.CreateView;
                    generateCreateViewSubpage();
                    break;

                case WizardStep.CreateView:
                    CurrentStep = WizardStep.Finish;
                    generateFinishSubpage();
                    break;

                case WizardStep.Finish:
                    AppController.Current.openMainWindow();
                    Close();
                    break;
            }
        }

        private void generateOAuthSubpage()
        {
            AppController.Current.Logger.addDebugMessage("Generating OAuth subpage", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            buttonNext.IsEnabled = false;
            labelTitle.Content = "Authorize accounts";
            textBlockFulltext.Text = "";
            textBlockFulltext.Inlines.Add("We will start this wizard by authorizing an account with App.net, Twitter, Facebook or QUOTE.fm.");
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add("Nymphicus uses a technique called ");
            Hyperlink linkToOAuth = new Hyperlink();
            linkToOAuth.Inlines.Add("OAuth");
            linkToOAuth.Click +=new RoutedEventHandler(linkToOAuth_Click);
            textBlockFulltext.Inlines.Add(linkToOAuth);
            textBlockFulltext.Inlines.Add(" to authorize with the services. This means when you press the \"+ Account\" button below a new window will open.");
             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add("Within this window Nymphicus will open a webpage of the service which will ask for your username and password. Those credentials will never been seen by Nymphicus itself but only for Twitter.");
             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add("When you are done with logging into Twitter it will show you a one time valid PIN code which you then please enter in the shown textbox of Nymphicus on bottom of the authorization window. The other services will be authenticated automatically without manually entering a PIN or similar.");
             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add("So let's start...");
             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add(new LineBreak());

             Button buttonApnAuthorize = new Button();
             buttonApnAuthorize.Content = "+ App.net";
             buttonApnAuthorize.Click += buttonApnAuthorize_Click;
             textBlockFulltext.Inlines.Add(buttonApnAuthorize);
             textBlockFulltext.Inlines.Add(", ");

             buttonAuthorize = new Button();
             buttonAuthorize.Content = "+ Twitter";
             buttonAuthorize.Click += new RoutedEventHandler(buttonAuthorize_Click);
             textBlockFulltext.Inlines.Add(buttonAuthorize);
             textBlockFulltext.Inlines.Add(", ");

        
             Button buttonFacebookAuthorize = new Button();
             buttonFacebookAuthorize.Content = "+ Facebook";
             buttonFacebookAuthorize.Click += new RoutedEventHandler(buttonFacebookAuthorize_Click);
             textBlockFulltext.Inlines.Add(buttonFacebookAuthorize);

             textBlockFulltext.Inlines.Add(" or ");
             Button buttonQuoteFmAuthorize = new Button();
             buttonQuoteFmAuthorize.Content = "+ QUOTE.fm";
             buttonQuoteFmAuthorize.Click += new RoutedEventHandler(buttonQuoteFmAuthorize_Click);
             textBlockFulltext.Inlines.Add(buttonQuoteFmAuthorize);

             textBlockFulltext.Inlines.Add(new LineBreak());
             textBlockFulltext.Inlines.Add(new LineBreak());
             Nymphicus.Controls.ListBoxAccounts listBoxAccounts = new Controls.ListBoxAccounts();
             listBoxAccounts.Width = 300;
             textBlockFulltext.Inlines.Add(listBoxAccounts);
        }

        void buttonApnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("App.net button clicked", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            AccountAppDotNet.authorizeNewAccount();
        }

        void buttonQuoteFmAuthorize_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("QUOTE.fm button clicked", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            UserInterface.QuoteFM.QuoteFM_AuthEasy authWindow = new QuoteFM.QuoteFM_AuthEasy();
            authWindow.Show();
        }

        void buttonFacebookAuthorize_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("Facebook button clicked", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            API.FacebookAPI.AuthorizeNewFacebookAccount newAccountWindow = new API.FacebookAPI.AuthorizeNewFacebookAccount();
            newAccountWindow.Show();
            
        }



        void buttonAuthorize_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("Twitter button clicked", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            UserInterface.GetOAuthorization authWindow = null;
            try
            {
                authWindow = new UserInterface.GetOAuthorization();
                AppController.Current.Logger.addDebugMessage("Twitter OAuth window opened", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, type: DebugMessage.DebugMessageTypes.FirstStartWizard);
                MessageBox.Show("Unable to connect to Twitter. Please check your internet connectivity e. g. by setting your proxy server.\n\nThe reported reason in detail is: " + exp.Message,"Connection failed",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            if (authWindow != null)
            {
                authWindow.AuthSuccess += new GetOAuthorization.AuthSuccessEventHandler(authWindow_AuthSuccess);

                authWindow.Show();
            }
        }

        void authWindow_AuthSuccess(object sender, GetOAuthorization.AuthSuccessEventArgs e)
        {
            GetOAuthorization authWindow = sender as GetOAuthorization;

            AppController.Current.sendNotification("General info", "Authorization in progress", "", "", null);
            AppController.Current.Logger.addDebugMessage("Start Twitter auth", "");
            AppController.Current.Logger.addDebugMessage("Proxy enabled", Properties.Settings.Default.ProxyEnabled.ToString(), type: DebugMessage.DebugMessageTypes.LogMessage);
            AppController.Current.Logger.addDebugMessage("Proxy server", Properties.Settings.Default.ProxyServer, type: DebugMessage.DebugMessageTypes.LogMessage);
            AppController.Current.Logger.addDebugMessage("Proxy port", Properties.Settings.Default.ProxyPort.ToString(), type: DebugMessage.DebugMessageTypes.LogMessage);

            OAuthAccessToken accessToken = null;

            try
            {
                accessToken = authWindow.account.twitterService.GetAccessToken(e.requestToken, e.verifyKey);
                
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace,type:DebugMessage.DebugMessageTypes.Twitter);
                AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace, "", null);
            }

            try
            {
                authWindow.account.Token = accessToken.Token;
                authWindow.account.TokenSecret = accessToken.TokenSecret;
     
                authWindow.account.verifyCredentials();
                Model.AccountTwitter newAccount = authWindow.account;
                newAccount.IsStreamingAccount = true;
                newAccount.getLists();
                newAccount.getSearches();
                newAccount.UpdateItems();
                AppController.Current.Logger.addDebugMessage("New Twitter account authoried", "", account: newAccount, type: DebugMessage.DebugMessageTypes.FirstStartWizard);
                AppController.Current.AllAccounts.Add(newAccount);
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.addDebugMessage(exp.Message, exp.StackTrace, type: DebugMessage.DebugMessageTypes.Twitter);
                AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace, "", null);
            }
        }

        void linkToOAuth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://en.wikipedia.org/wiki/OAuth");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void generateCreateViewSubpage()
        {
            AppController.Current.Logger.addDebugMessage("Generating create View subpage", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            buttonNext.IsEnabled = false;
            labelTitle.Content = "Create a view";
            textBlockFulltext.Text = "";
            textBlockFulltext.Inlines.Add("Now that we have created an account we will create a first view which defines what kind of tweets you want to get displayed in your main window");
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add("A view is a collection of different tweet types (timeline, direct messages, search results, ...) and different accounts which are displayed in one single list of items. So you can define views for certain types, certain accounts and any mixture.");
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add("When you press the \"Create view\" button a new window will open showing you all possible tweet types. Please enter a name for your view there and select at least one checkbox.");
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            buttonCreateView = new Button();
            buttonCreateView.Content = "Create view";
            buttonCreateView.Click += new RoutedEventHandler(buttonCreateView_Click);
            textBlockFulltext.Inlines.Add(buttonCreateView);

        }

        void buttonCreateView_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("Create View clicked", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            AppController.Current.AllViews.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllViews_CollectionChanged);
            Nymphicus.UserInterface.CreateOrEditView newViewWindow = new CreateOrEditView();
            newViewWindow.infoBoxViewName.textBoxContent.Text = "My first view";
            try
            {
                newViewWindow.buttonSelectAll_Click(null, null);
                newViewWindow.infoBoxViewName.InfoContent = "Example view";
                newViewWindow.buttonSave.IsEnabled = true;
            }
            catch
            {
                // just in case...
            }
            newViewWindow.Show();
        }

        void AllViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AppController.Current.Logger.addDebugMessage("View added", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            if (AppController.Current.AllViews.Count > 0)
            {
                buttonNext.IsEnabled = true;
                buttonCreateView.Content = "Create another view";
            }
        }

        private void generateFinishSubpage()
        {
            AppController.Current.Logger.addDebugMessage("Generating Finish subpage", "-", type: DebugMessage.DebugMessageTypes.FirstStartWizard);
            buttonNext.IsEnabled = true;
            labelTitle.Content = "Initial setup completed";
            textBlockFulltext.Text = "";
            textBlockFulltext.Inlines.Add("OK, you now have an account and a view so you can now start using Nymphicus by pressing the next button.");
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add("You can always add (and remove) accounts and views in the preferences of Nymphicus.");
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add("The full documentation of Nymphicus is available at ");
            Hyperlink linkToDocs = new Hyperlink();
            linkToDocs.Inlines.Add("http://www.nymphicusapp.com/windows/documentation/");
            linkToDocs.Click += new RoutedEventHandler(linkToDocs_Click);
            textBlockFulltext.Inlines.Add(linkToDocs);
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add(new LineBreak());
            textBlockFulltext.Inlines.Add("And now: Have fun!");

            AppController.Current.saveAccountsAndViews();
        }

        void linkToDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.nymphicusapp.com/windows/documentation/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }
    
    }
}
