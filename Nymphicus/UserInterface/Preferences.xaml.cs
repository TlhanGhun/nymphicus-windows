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
using TweetSharp;
using Nymphicus.Model;
using Nymphicus.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        //private bool _isDragging = false;
        //private ListView _dragSource;

        private ThreadSaveObservableCollection<View> viewsUsingSelectedFilter;

        public Preferences()
        {
            InitializeComponent();
            borderAccountInfo.Visibility = Visibility.Hidden;
            colorPicker.SelectedColorChanged += new Action<Color>(colorPicker_SelectedColorChanged);
            listViewAccounts.listViewAccounts.SelectionChanged += new SelectionChangedEventHandler(listViewAccounts_SelectionChanged);
            listViewViews.ItemsSource = AppController.Current.AllViews;
            listBoxFilters.ItemsSource = AppController.Current.AllFilters;
            listboxIStores.ItemsSource = AppController.Current.AllIStores;
            viewsUsingSelectedFilter = new ThreadSaveObservableCollection<View>();
            listViewViews.SelectionChanged += new SelectionChangedEventHandler(listViewViews_SelectionChanged);
            listBoxViewsUsingThisFilter.ItemsSource = viewsUsingSelectedFilter;
            if (listBoxFilters.Items.Count > 0)
            {
                listBoxFilters.SelectedIndex = 0;
            }

            #region Themes

            // colorPickerGeneralBackgroundColor.SelectedColor = 

            #endregion


            if (AppController.Current.AllAccounts.Count > 0)
            {
                listViewAccounts.listViewAccounts.SelectedIndex = 0;
            }

            if (AppController.Current.AllViews.Count > 0)
            {
                listViewViews.SelectedIndex = 0;
            }

            AppController.Current.AllFilters.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllFilters_CollectionChanged);
            AppController.Current.AllAccounts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(AllAccounts_CollectionChanged);

            createRadiobuttonsForUrlShortener();
            createRadiobuttonsForImageServices();
            selectLocationService();



            if (AppController.Current.SnarlIsRunning)
            {
                textBlockSnarlRunningOrNot.Visibility = Visibility.Hidden;
            }

            if (AppController.Current.mainWindow != null)
            {
                this.Top = AppController.Current.mainWindow.Top;
                if (AppController.Current.mainWindow.Left > this.Width + 5)
                {
                    this.Left = AppController.Current.mainWindow.Left - this.Width - 5;
                }
                else
                {
                    this.Left = AppController.Current.mainWindow.Left + AppController.Current.mainWindow.Width + 5;

                }
            }
        }



        void AllAccounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            applyViewSettings();
            createViewTree(listViewViews.SelectedItem as View);
        }

        void AllFilters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Filter filter = listBoxFilters.SelectedItem as Filter;
            if (filter != null)
            {
                viewsUsingSelectedFilter = new ThreadSaveObservableCollection<View>();
                foreach (View view in getAllViewsUsingThisFilter(filter))
                {
                    viewsUsingSelectedFilter.Add(view);
                }
                listBoxViewsUsingThisFilter.ItemsSource = viewsUsingSelectedFilter;
            }
        }

        void listViewViews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            View view = listViewViews.SelectedItem as View;
            if(view != null) {
                createViewTree(view);
            }
            else
            {
                treeViewSelector.Items.Clear();
            }
        }


        private void createViewTree(View view)
        {
            treeViewSelector = GeneralFunctions.treeViewForViews.CreateViewTreeView(view, treeViewSelector);
            return;
        }

        void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Filter filter = button.CommandParameter as Filter;
                if (filter != null)
                {
                    EditFilter editFilter = new EditFilter();
                    editFilter.LoadFilter(filter);
                    editFilter.FilterChanged += new EditFilter.FilterChangedEventHandler(editFilter_FilterChanged);
                    editFilter.Show();
                }
            }
        }

        void editFilter_FilterChanged(object sender, EditFilter.FilterChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.changedFilter != null)
                {
                    listBoxFilters.SelectedItem = null;
                    listBoxFilters.SelectedItem = e.changedFilter;
                }
            }
        }

        void createNewFilter_Click(object sender, RoutedEventArgs e)
        {
            applyViewSettings();
            EditFilter newFilter = new EditFilter();
            newFilter.FilterCreated += new EditFilter.FilterCreatedEventHandler(newFilter_FilterCreated);
            newFilter.Show();
        }

        void newFilter_FilterCreated(object sender, EditFilter.FilterCreatedEventArgs e)
        {
            if (e != null)
            {
                if (e.newFilter != null)
                {
                    createViewTree(getCurrentSelectedView());
                }
            }
        }

        private void applyViewSettings()
        {
            View view = listViewViews.SelectedItem as View;
            if (treeViewSelector != null)
            {
                ItemCollection itemCollection = treeViewSelector.Items;

                GeneralFunctions.treeViewForViews.applyViewSettings(view, treeViewSelector.Items);
            }
        }

        private static decimal getIdOfCheckboxName(string checkboxName) {
            decimal id = 0;
            string[] split = checkboxName.Split('_');
            if(split.Length == 2) {
                try
                {
                    id = Convert.ToDecimal(split[1]);
                }
                catch
                {
                    id = 0;
                }
            }
            return id;
        }

        private static bool IsType(object obj, string type)
        {
            return (obj.GetType().ToString().ToLower().EndsWith(type.ToLower()));
        }

        void listViewAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            applyViewSettings();
            userInfoTwitter.Visibility = System.Windows.Visibility.Collapsed;
            userInfoFacebook.Visibility = System.Windows.Visibility.Collapsed;
            userInfoQuoteFm.Visibility = System.Windows.Visibility.Collapsed;
            buttonEditLists.Visibility = System.Windows.Visibility.Collapsed;
            ListView listView = sender as ListView;
            if(listView != null) {
                IAccount iaccount = listView.SelectedItem as IAccount;
                if(iaccount != null) {
                    buttonRemoveAccount.IsEnabled = true;
                    borderAccountInfo.Visibility = Visibility.Visible;
                    colorPicker.SelectedColor = iaccount.accountColor;
                    if (iaccount.GetType() == typeof(AccountTwitter))
                    {
                        AccountTwitter twAccount = iaccount as AccountTwitter;
                        buttonEditLists.Visibility = System.Windows.Visibility.Visible;
                        userInfoTwitter.Visibility = System.Windows.Visibility.Visible;

                        userInfoTwitter.DataContext = twAccount.Login;
                    }
                    else if (iaccount.GetType() == typeof(AccountFacebook))
                    {
                        AccountFacebook fbAccount = iaccount as AccountFacebook;

                        userInfoFacebook.Visibility = System.Windows.Visibility.Visible;
                      
                        userInfoFacebook.DataContext = fbAccount.User;
                    }
                    else if (iaccount.GetType() == typeof(AccountQuoteFM))
                    {
                        AccountQuoteFM qfmAccount = iaccount as AccountQuoteFM;
                        userInfoQuoteFm.Visibility = System.Windows.Visibility.Visible;
                        userInfoQuoteFm.DataContext = qfmAccount.User;
                    }

                    if (iaccount.accountColor != null)
                    {
                        colorPicker.SelectedColor = iaccount.accountColor;
                        colorPicker.UpdateLayout();
                    }
                }
                else
                {
                    buttonRemoveAccount.IsEnabled = false;
                    borderAccountInfo.Visibility = Visibility.Hidden;
                }
            }
        }


        void colorPicker_SelectedColorChanged(Color color)
        {
            if (listViewAccounts.listViewAccounts.SelectedItem != null)
            {
                var selectAccount = listViewAccounts.listViewAccounts.SelectedItem as IAccount;
                selectAccount.accountColor = color;
                SolidColorBrush brush = new SolidColorBrush(color);

                AppController.Current.accountColors[selectAccount.Id] = brush;
                listViewAccounts.listViewAccounts.Items.Refresh();
                
            }
        }

        private void buttonAddNewAccount_Click(object sender, RoutedEventArgs e)
        {
           

            UserInterface.GetOAuthorization authWindow;
            try
            {                
                authWindow = new UserInterface.GetOAuthorization();
            }
            catch
            {

                MessageBox.Show("Unable to connect to Twitter. Please check your internet connectivity e. g. by setting your proxy server","Connection failed",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            authWindow.AuthSuccess += new GetOAuthorization.AuthSuccessEventHandler(authWindow_AuthSuccess);

            authWindow.Show();
        }


        private void buttonAddNewAccountIdentica_Click(object sender, RoutedEventArgs e)
        {
            /*
            tokens = new Twitterizer.OAuthTokens();
            tokens.AccessToken = API.ConnectionData.twitterOAuthToken;
            tokens.AccessTokenSecret = API.ConnectionData.twitterOAuthTokenSecret;

            UserInterface.GetOAuthorization authWindowIdentica;
            try
            {
                //oAuthRequestToken = OAuthUtility.GetRequestToken(API.ConnectionData.twitterConsumerKey, API.ConnectionData.twitterConsumerSecret, "oob");

                oAuthRequestToken = OpenOAuthUtility.GetRequestToken(API.ConnectionData.identicaRequestTokenUrl, API.ConnectionData.identiaConsumerKey, API.ConnectionData.identicaConsumerSecret, "oob");
                //authWindow = new UserInterface.GetOAuthorization(Twitterizer.OAuthUtility.BuildAuthorizationUri(oAuthRequestToken.Token).AbsoluteUri.ToString());
                authWindowIdentica = new UserInterface.GetOAuthorization(Twitterizer.OpenOAuthUtility.BuildAuthorizationUri(API.ConnectionData.identicaOauthUrl, oAuthRequestToken.Token).AbsoluteUri.ToString());
            }
            catch (Exception exp)
            {

                MessageBox.Show(exp.Message, "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            authWindowIdentica.AuthSuccess += new GetOAuthorization.AuthSuccessEventHandler(authWindowIdentica_AuthSuccess);

            authWindowIdentica.Show();
             * */
        }


        private void showStacktrace(Exception exp)
        {
            MessageBox.Show(exp.StackTrace, exp.Message, MessageBoxButton.OK, MessageBoxImage.Information);
            if (exp.InnerException != null)
            {
                showStacktrace(exp.InnerException);
            }
        }

        void authWindow_AuthSuccess(object sender, GetOAuthorization.AuthSuccessEventArgs e)
        {
            AppController.Current.sendNotification("General info", "Authorization in progress", "", "",null);
            GetOAuthorization authWindow = sender as GetOAuthorization;
            OAuthAccessToken accessToken = null;
            
            try
            {
                accessToken = authWindow.account.twitterService.GetAccessToken(e.requestToken, e.verifyKey);
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace,"",null);
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
                AppController.Current.AllAccounts.Add(newAccount);
                AppController.Current.saveAccountsAndViews();
                if (AppController.Current.AllViews.Count == 0)
                {
                    tabViews.IsSelected = true;
                    buttonViewAdd_Click(null, null);
                }
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace, "",null);
            }

        }

        void authWindowIdentica_AuthSuccess(object sender, GetOAuthorization.AuthSuccessEventArgs e)
        {
            /*
            AppController.Current.sendNotification("General info", "Authorization in progress", "", "", null);
            OAuthTokenResponse accessToken = new OAuthTokenResponse();
            try
            {
                //accessToken = OpenOAuthUtility.GetAccessToken(API.ConnectionData.identicaAccessTokenUrl, API.ConnectionData.twitterConsumerKey, API.ConnectionData.twitterConsumerSecret, oAuthRequestToken.Token, e.verifyKey);
                accessToken = OpenOAuthUtility.GetAccessToken(API.ConnectionData.identicaAccessTokenUrl, "Identi.ca API", API.ConnectionData.identiaConsumerKey, API.ConnectionData.identicaConsumerSecret, oAuthRequestToken.Token, e.verifyKey);
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace, "", null);
            }

            try
            {
                DataTypes.AccountTwitter newAccount = AppController.Current.addAccountFromTwitterTokens(accessToken);
                newAccount.UpdateItems();
                AppController.Current.openMainWindow();
                if (AppController.Current.AllViews.Count == 0)
                {
                    tabViews.IsSelected = true;
                    buttonViewAdd_Click(null, null);
                }
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", exp.Message, exp.StackTrace, "", null);
            }
             * */
        }


        private void buttonRemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            IAccount account = listViewAccounts.listViewAccounts.SelectedItem as IAccount;
            if (account != null)
            {
                AppController.Current.removeAccount(account);
            }

        }

        private void buttonEditLists_Click(object sender, RoutedEventArgs e)
        {
            if (listViewAccounts.listViewAccounts.SelectedItem != null)
            {
                AccountTwitter account = listViewAccounts.listViewAccounts.SelectedItem as AccountTwitter;
                ManageLists manageLists = new ManageLists();
                manageLists.selectAccount(account);
                manageLists.Show();
            }
        }

        private void buttonApplyView_Click(object sender, RoutedEventArgs e)
        {
            applyViewSettings();
        }

        private void buttonViewAdd_Click(object sender, RoutedEventArgs e)
        {
            CreateOrEditView newViewWindow = new CreateOrEditView();
            //CreateNewView newViewWindow = new CreateNewView();
            newViewWindow.Show();
        }

        private void buttonRemoveView_Click(object sender, RoutedEventArgs e)
        {
            if (listViewViews.SelectedItem != null)
            {
                var selectedView = listViewViews.SelectedItem as View;
                if (selectedView != null)
                {
                    AppController.Current.AllViews.Remove(selectedView);
                    selectedView = null;
                }
            }
        }

        #region Network

     

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            applyViewSettings();
            
            Properties.Settings.Default.Save();
            AppController.Current.ApplyProxySettings();
            this.Close();
        }

        #endregion

        private void comboBoxDefaultUrlShortener_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            if (combobox != null)
            {
                if (combobox.SelectedItem != null)
                {
                    Properties.Settings.Default.DefaultUrlShortener = combobox.SelectedItem.ToString();
                }
            }
        }

        private void checkBoxStreaming_Checked(object sender, RoutedEventArgs e)
        {
            if (listViewAccounts.listViewAccounts.SelectedItem != null)
            {
                AccountTwitter account = listViewAccounts.listViewAccounts.SelectedItem as AccountTwitter;
                if (account != null)
                {
                    account.IsStreamingAccount = true;
                }
            }
        }

        private void checkBoxStreaming_Unchecked(object sender, RoutedEventArgs e)
        {
            if (listViewAccounts.listViewAccounts.SelectedItem != null)
            {
                AccountTwitter account = listViewAccounts.listViewAccounts.SelectedItem as AccountTwitter;
                if (account != null)
                {
                    account.IsStreamingAccount = false;
                    account.UpdateItems();
                }
            }
        }

        private void buttonViewUp_Click(object sender, RoutedEventArgs e)
        {
            View selectedView = getCurrentSelectedView();
            if (selectedView != null)
            {
                if (listViewViews.SelectedIndex > 0)
                {
                   // View tempView = listViewViews.SelectedIndex as View
                }
            }
        }

        private View getCurrentSelectedView()
        {
                return listViewViews.SelectedItem as View;

        }

        #region Listbox Drag and Drop
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            return;
            // cast sender as list box
            /*
            ListView box = sender as ListView;

            if (box != null)
            {
                // ensure there is a list box item under the mouse
                if (Preferences.GetObjectDataFromPoint(box, e.GetPosition(box)) != null)
                {
                    // set the dragging flag
                    _isDragging = true;

                    // set drag source
                    _dragSource = box;
                }
            }

            // set handled to true, to override the listboxitem from handling mouse down and selecting the item
            e.Handled = true;
             * */
        }

        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            return;
            // cast sender as list box
            /*
            ListView box = sender as ListView;

            // if a list box item was selected, perform select / unselect here, rather than allowing
            // wpf to handle it on mouse down of the list box item.  This way, the user can click and
            // drag a bunch of selected items, without deselecting the item they clicked.
            if (box != null)
            {
                ListViewItem item = Preferences.GetObjectDataFromPoint(box, e.GetPosition(box)) as ListViewItem;

                if (item != null)
                {
                    item.IsSelected = !item.IsSelected;
                }
            }

            // clear dragging flag and drag source
            _isDragging = false;
            _dragSource = null;
             */
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            return;

            // cast sender as list box
            /*
            ListView box = sender as ListView;


            // if dragging, do drag drop
            if (box != null && _isDragging && _dragSource != null)
            {

                // if drag started with a listbox item, but it wasn't selected, select it now
                ListViewItem item = Preferences.GetObjectDataFromPoint(box, e.GetPosition(box)) as ListViewItem;

                if (item != null && !item.IsSelected)
                    item.IsSelected = true;

                // set the data to be dragged
                DataObject data = new DataObject();
                data.SetData(typeof(View), _dragSource.SelectedItems);

                // do drag drop
                DragDrop.DoDragDrop(_dragSource, data, DragDropEffects.Move);

                // clear dragging flag and drag source
                _isDragging = false;
                _dragSource = null;
            }
             * */
        }

        private static object GetObjectDataFromPoint(ListView source, Point point)
        {
            return null;
            // get the element under the mouse
            /*
            UIElement element = source.InputHitTest(point) as UIElement;

            if (element != null)
            {
                // if listbox item right off the bat, return it
                if (element is ListViewItem)
                    return element;

                // otheriwse, loop up the  visual tree until a listboxitem is found
                // if we get to the listbox, no need to go on, no listbox item was found
                while (element != source)
                {
                    // get the element's parent
                    element = VisualTreeHelper.GetParent(element) as UIElement;

                    // if now a listboxitem, return it
                    if (element is ListViewItem)
                        return element;
                }
            }

            return null; */
        }


        #endregion

        private void buttonAddFilter_Click(object sender, RoutedEventArgs e)
        {
            EditFilter newFilter = new EditFilter();
            newFilter.FilterCreated += new EditFilter.FilterCreatedEventHandler(newFilter_FilterCreated);
            newFilter.Show();
        }

        private void buttonRemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            Filter selectedFilter = listBoxFilters.SelectedItem as Filter;
            if (selectedFilter != null)
            {
                foreach (View view in getAllViewsUsingThisFilter(selectedFilter))
                {
                    view.unsubsribeFromFilter(selectedFilter.Id);
                }
                AppController.Current.AllFilters.Remove(selectedFilter);
            }
        }

        private IEnumerable<View> getAllViewsUsingThisFilter(Filter filter)
        {
            return AppController.Current.AllViews.Where(v => v.subscribedFilter.Contains(filter));
        }

        

        private void listBoxFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter selectedFilter = listBoxFilters.SelectedItem as Filter;
            if (selectedFilter != null)
            {
                viewsUsingSelectedFilter = new ThreadSaveObservableCollection<View>();
                buttonEditFilter.IsEnabled = true;
                foreach (View view in getAllViewsUsingThisFilter(selectedFilter))
                {
                    viewsUsingSelectedFilter.Add(view);
                }
                listBoxViewsUsingThisFilter.ItemsSource = viewsUsingSelectedFilter;
            }
            else
            {
                buttonEditFilter.IsEnabled = false;
            }
        }

        private void tabControlPreferences_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tabItem = e.OriginalSource as TabItem;
        }

        private void selectLocationService()
        {
            if (Properties.Settings.Default.LocationMapService == "OpenStreetMap")
            {
                radioButtonOpenStreetMap.IsChecked = true;
            }
            else
            {
                radioButtonGoogleMaps.IsChecked = true;
            }
        }

        private void createRadiobuttonsForUrlShortener()
        {
            foreach(API.UrlShortener.ILinkShortener shortener in AppController.Current.AlllinkShortenerServices) {
                if (!shortener.CanShorten)
                {
                    continue;
                }
                RadioButton radioButton = new RadioButton();
                radioButton.GroupName = "UrlShortener";
                radioButton.Content = shortener.Name;
                if (shortener == AppController.Current.ActualLinkShortener)
                {
                    radioButton.IsChecked = true;
                }
                radioButton.Checked += new RoutedEventHandler(radioButtonUrlShortenerService_Checked);
                radioButton.Margin = new Thickness(5,0,0,0);
                stackpanelForUrlShortenerServices.Children.Add(radioButton);
                if (shortener.CanHaveCredentials)
                {
                    StackPanel loginPanel = new StackPanel();
                    loginPanel.Orientation = Orientation.Vertical;


                    StackPanel usernamePanel = new StackPanel();
                    usernamePanel.Orientation = Orientation.Horizontal;
                    Label loginLabel = new Label();
                    loginLabel.Margin = new Thickness(8, 0, 0, 0);
                    loginLabel.Content = "Login";
                    loginLabel.Width = 120;
                    TextBox  loginTextbox = new TextBox();
                    loginTextbox.Width = 120;
                    loginTextbox.Text = shortener.Login;
                    loginTextbox.DataContext = shortener;
                    loginTextbox.TextChanged +=new TextChangedEventHandler(loginTextbox_TextChanged);
                    usernamePanel.Children.Add(loginLabel);
                    usernamePanel.Children.Add(loginTextbox);

                    loginPanel.Children.Add(usernamePanel);

                    StackPanel passwordPanel = new StackPanel();
                    passwordPanel.Orientation = Orientation.Horizontal;
                    Label passwordLabel = new Label();
                    passwordLabel.Width = 120;
                    passwordLabel.Content = "Password / Key";      
                    passwordLabel.Margin = new Thickness(8, 0, 0, 0);
                    PasswordBox passwordTextbox = new PasswordBox();
                    passwordTextbox.Width = 120;
                    passwordTextbox.Password = shortener.Password;
                    passwordTextbox.DataContext = shortener;
                    passwordTextbox.PasswordChanged += new RoutedEventHandler(passwordTextbox_PasswordChanged);

                    passwordPanel.Children.Add(passwordLabel);
                    passwordPanel.Children.Add(passwordTextbox);

                    loginPanel.Children.Add(passwordPanel);

                    stackpanelForUrlShortenerServices.Children.Add(loginPanel);   
                }
            }
        }

        void passwordTextbox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                API.UrlShortener.ILinkShortener shortener = passwordBox.DataContext as API.UrlShortener.ILinkShortener;
                if (shortener != null)
                {
                    shortener.Password = passwordBox.Password;
                }
            }
        }

        void loginTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                API.UrlShortener.ILinkShortener shortener = textBox.DataContext as API.UrlShortener.ILinkShortener;
                if (shortener != null)
                {
                    shortener.Login = textBox.Text;
                }
            }
        }

        private void createRadiobuttonsForImageServices()
        {
            foreach (API.ImageServices.IImageService imageService in AppController.Current.AllImageServices)
            {
                if (!imageService.CanUpload)
                {
                    continue;
                }
                RadioButton radioButton = new RadioButton();
                radioButton.GroupName = "ImageServices";
                radioButton.Content = imageService.Name;
                if (imageService == AppController.Current.ActualImageService)
                {
                    radioButton.IsChecked = true;
                }
                radioButton.Checked += new RoutedEventHandler(radioButtonImageService_Checked);
                radioButton.Margin = new Thickness(5, 0, 0, 0);
                stackpanelForImageServices.Children.Add(radioButton);
            }
        }

        private void radioButtonImageService_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = sender as RadioButton;
            if (radiobutton != null)
            {
                if (!string.IsNullOrEmpty(radiobutton.Content.ToString()))
                {
                    Properties.Settings.Default.DefaultImageService = radiobutton.Content.ToString();
                    AppController.Current.selectImageService();
                }
            }
        }

        private void radioButtonUrlShortenerService_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = sender as RadioButton;
            if (radiobutton != null)
            {
                if (!string.IsNullOrEmpty(radiobutton.Content.ToString()))
                {
                    Properties.Settings.Default.DefaultUrlShortener = radiobutton.Content.ToString();
                    AppController.Current.selectUrlShortener();
                }
            }
        }

        private void radioButtonLocationService_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = sender as RadioButton;
            if (radiobutton != null)
            {
                if (!string.IsNullOrEmpty(radiobutton.Content.ToString()))
                {
                    Properties.Settings.Default.LocationMapService = radiobutton.Content.ToString();
                    AppController.Current.selectUrlShortener();
                }
                if (radiobutton.Content.ToString() == "OpenStreetMap")
                {
                    radioButtonGoogleMaps.IsChecked = false;
                }
                else
                {
                    radioButtonOpenStreetMap.IsChecked = false;
                }
            }
        }


        private void buttonEditFilter_Click(object sender, RoutedEventArgs e)
        {

            Filter filter = listBoxFilters.SelectedItem as Filter;
            if (filter != null)
            {
                EditFilter editFilter = new EditFilter();
                editFilter.LoadFilter(filter);
                editFilter.Show();
            }
    }

        #region Hyperlinks in credits

        private void hyperlinkToTwitterizer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/danielcrenna/tweetsharp");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToTwitterizerLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/danielcrenna/tweetsharp/blob/master/LICENSE.md");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToIconic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://somerandomdude.com/projects/iconic/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToCcShareAlike_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://creativecommons.org/licenses/by-sa/3.0/us/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkTo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToToke_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://noer.it/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToJsonNet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://json.codeplex.com/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToJsonLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://json.codeplex.com/license");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToColorPicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://wpfcolorpicker.codeplex.com/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

    #endregion

       

        private void buttonAddNewAccountFacebook_Click(object sender, RoutedEventArgs e)
        {
            API.FacebookAPI.AuthorizeNewFacebookAccount newAuthWindow = new API.FacebookAPI.AuthorizeNewFacebookAccount();
            newAuthWindow.Show();
        }


        private void hyperlinkToFacebookSdk_Click(object sender, RoutedEventArgs e)
        {
               try
            {
                System.Diagnostics.Process.Start("http://facebooknet.codeplex.com/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }

            
        }

        private void hyperlinkToFacebookLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://facebooknet.codeplex.com/license");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToWpfToolkit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://wpftoolkit.codeplex.com/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void hyperlinkToWpFToolkitLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://wpftoolkit.codeplex.com/license");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void buttonDebugStatistics_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.DebugStatistics stats = new DebugStatistics();
            stats.Show();

        }

        private void buttonSnarlHompeage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://snarl.fullphat.net/");
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void buttonDebugSettings_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.DebugSettings debugSettings = new DebugSettings();
            debugSettings.Show();
        }

        private void buttonAddNewQuoteFM_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.QuoteFM.QuoteFM_AuthEasy newQmAccount = new QuoteFM.QuoteFM_AuthEasy();
            newQmAccount.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppController.Current.saveAccountsAndViews();
        }

        private void buttonAddNewAccountApn_Click_1(object sender, RoutedEventArgs e)
        {
            AccountAppDotNet.authorizeNewAccount();
        }


        private void buttonEditFilterInExtraWindow_Click_1(object sender, RoutedEventArgs e)
        {
            EditFilter editFilter = new EditFilter();
            editFilter.LoadFilter(listBoxFilters.SelectedItem as Filter);
            editFilter.Show();
        }


    }
}
