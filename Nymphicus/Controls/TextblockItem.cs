using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Nymphicus.Model;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;

namespace Nymphicus.Controls
{
    public class TextblockItem : TextBlock
    {

        public TextblockItem()
        {
        
        }

        class externalServiceParameter
        {
            public Nymphicus.ExternalServices.IStore storeService { get; set; }
            public string link { get; set; }
            public IItem item { get; set; }
        }

        void generateElementsTextblock(List<TextSubTypes.ISubType> inlines)
        {
            this.Inlines.Clear();

            try
            {

                IItem item = this.DataContext as IItem;



                foreach (object inline in inlines)
                {
                    if (inline.GetType() == typeof(TextSubTypes.Link))
                    {
                        TextSubTypes.Link element = (TextSubTypes.Link)inline;

                        Hyperlink link = new Hyperlink();
                        link.TextDecorations = null;
                        Uri target;
                        Uri.TryCreate(element.urlLong, UriKind.Absolute, out target);
                        link.NavigateUri = target;
                        link.ToolTip = element.urlLong;
                        if (Properties.Settings.Default.AutoExpandLinks || string.IsNullOrEmpty(element.urlShort))
                        {
                            link.Inlines.Add(element.urlLong);
                        }
                        else
                        {
                            link.Inlines.Add(element.urlShort);
                        }

                        link.Click += new RoutedEventHandler(link_Click);
                        ContextMenu contextMenuLink = new ContextMenu();

                        MenuItem menuItemCopyToClipboard = new MenuItem();
                        menuItemCopyToClipboard.Header = "Copy link";
                        menuItemCopyToClipboard.CommandParameter = element.urlLong;
                        menuItemCopyToClipboard.Click += new RoutedEventHandler(menuItemCopyToClipboard_Click);
                        contextMenuLink.Items.Add(menuItemCopyToClipboard);

                        MenuItem menuItemOpenInBrowser = new MenuItem();
                        menuItemOpenInBrowser.Header = "Open in webbrowser";
                        menuItemOpenInBrowser.CommandParameter = element.urlLong;
                        menuItemOpenInBrowser.Click += new RoutedEventHandler(menuItemOpenInBrowser_Click);
                        contextMenuLink.Items.Add(menuItemOpenInBrowser);

                        MenuItem contextExternalServices = new MenuItem();
                        contextExternalServices.Header = "Send to...";


                        foreach (Nymphicus.ExternalServices.IStore service in AppController.Current.AllIStores)
                        {
                            MenuItem menuItem = new MenuItem();
                            menuItem.Header = service.Name;
                            externalServiceParameter parameter = new externalServiceParameter();
                            parameter.storeService = service;
                            parameter.link = element.urlLong;
                            parameter.item = item;
                            menuItem.CommandParameter = parameter;
                            BitmapImage serviceImage = new BitmapImage(new Uri(service.ServiceIcon));
                            serviceImage.DecodePixelWidth = 16;
                            serviceImage.CacheOption = BitmapCacheOption.OnLoad;
                            menuItem.Icon = new System.Windows.Controls.Image { Source = serviceImage };

                            menuItem.Click += new RoutedEventHandler(menuItem_Click);
                            contextExternalServices.Items.Add(menuItem);
                        }

                        contextMenuLink.Items.Add(contextExternalServices);
                        link.ContextMenu = contextMenuLink;
                        this.Inlines.Add(link);
                    }
                    else if (inline.GetType() == typeof(TextSubTypes.ImageLink))
                    {
                        TextSubTypes.ImageLink element = (TextSubTypes.ImageLink)inline;

                        Hyperlink link = new Hyperlink();
                        link.TextDecorations = null;
                        Uri target;
                        Uri.TryCreate(element.urlLong, UriKind.Absolute, out target);
                        link.NavigateUri = target;

                        Grid tooltipGrid = new Grid();
                        Image tooltipImage = new Image();
                        tooltipImage.Source = GetImageFromURL(element.imageUrl);
                        tooltipGrid.Children.Add(tooltipImage);
                        link.ToolTip = tooltipGrid;

                        link.Inlines.Add(element.urlLong);

                        link.Click += new RoutedEventHandler(link_Click);
                        this.Inlines.Add(link);
                    }
                    else if (inline.GetType() == typeof(TextSubTypes.HashTag))
                    {
                        TextSubTypes.HashTag element = (TextSubTypes.HashTag)inline;
                        Hyperlink hashtag = new Hyperlink();
                        char[] trimChars = {',',
                                                 ';',
                                                 ':',
                                                 ' ',
                                                 '!',
                                             '?',
                                             '.'
                        };
                        hashtag.TargetName = element.text.Substring(1).Trim(trimChars);
                        hashtag.TextDecorations = null;
                        hashtag.DataContext = item;
                        hashtag.ToolTip = "Open search for " + element.text;
                        hashtag.Click += new RoutedEventHandler(hashtag_Click);
                        TextBlock hashTextBlock = new TextBlock();
                        hashTextBlock.Text = element.text;
                        hashTextBlock.TextDecorations = null;
                        hashTextBlock.Foreground = Brushes.Red;
                        hashtag.Inlines.Add(hashTextBlock);

                        ContextMenu contextHashtag = new ContextMenu();

                        MenuItem menuItemOpenInSearch = new MenuItem();
                        menuItemOpenInSearch.Header = "Open search for " + element.text;
                        menuItemOpenInSearch.CommandParameter = element.text;
                        menuItemOpenInSearch.Click += new RoutedEventHandler(menuItemOpenInSearch_Click);
                        contextHashtag.Items.Add(menuItemOpenInSearch);

                        MenuItem menuItemMuteHashtag = new MenuItem();
                        menuItemMuteHashtag.Header = "Mute " + element.text + " in this View (will create a filter)";
                        menuItemMuteHashtag.CommandParameter = element.text;
                        menuItemMuteHashtag.Click += new RoutedEventHandler(menuItemMuteHashtag_Click);
                        contextHashtag.Items.Add(menuItemMuteHashtag);

                        hashtag.ContextMenu = contextHashtag;

                        this.Inlines.Add(hashtag);
                    }
                    else if (inline.GetType() == typeof(TextSubTypes.User))
                    {
                        TextSubTypes.User element = (TextSubTypes.User)inline;
                        Hyperlink twitterUser = new Hyperlink();
                        twitterUser.TextDecorations = null;
                        twitterUser.DataContext = item;
                        twitterUser.ToolTip = "Open user info of " + element.userName;
                        twitterUser.TargetName = element.userName.Substring(1);
                        char[] trimChars = {',',
                                                 ';',
                                                 ':',
                                                 ' ',
                                                 '!',
                                             '?',
                                             '.'
                        };
                        twitterUser.TargetName = twitterUser.TargetName.Trim(trimChars);
                        twitterUser.Click += new RoutedEventHandler(twitterUser_Click);
                        TextBlock twitterUserBlock = new TextBlock();
                        twitterUserBlock.TextWrapping = TextWrapping.Wrap;
                        twitterUserBlock.TextDecorations = null;
                        twitterUserBlock.Text = element.userName;
                        twitterUserBlock.Foreground = Brushes.DarkGreen;
                        twitterUser.Inlines.Add(twitterUserBlock);
                        this.Inlines.Add(twitterUser);
                    }
                    else if (inline.GetType() == typeof(TextSubTypes.Text))
                    {
                        TextSubTypes.Text element = (TextSubTypes.Text)inline;
                        this.Inlines.Add(element.text);
                    }
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Tweet text parsing failed");
                AppController.Current.Logger.writeToLogfile(exp);
                try
                {
                    TwitterItem item = this.DataContext as TwitterItem;
                    AppController.Current.Logger.writeToLogfile("Tweet text: " + item.Text);
                }
                catch (Exception exp2)
                {
                    AppController.Current.Logger.writeToLogfile("Tweet text cannot be casted to string!");
                    AppController.Current.Logger.writeToLogfile(exp2);
                }
            }
        }

        void menuItemMuteHashtag_Click(object sender, RoutedEventArgs e)
        {
               MenuItem menuItem = sender as MenuItem;
               if (menuItem != null)
               {
                   if (AppController.Current.CurrentView != null)
                   {
                       if (AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == menuItem.CommandParameter.ToString().ToLower()).Count() > 0)
                       {
                           Filter existingFilter = AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == menuItem.CommandParameter.ToString().ToLower()).First();
                           if (existingFilter != null)
                           {
                               if (!AppController.Current.CurrentView.subscribedFilter.Contains(existingFilter))
                               {
                                   AppController.Current.CurrentView.subscribeToFilter(existingFilter.Id);
                               }
                           }
                       }
                       else
                       {
                           Filter filter = new Filter();
                           filter.Name = menuItem.CommandParameter.ToString();
                           filter.FilterString = menuItem.CommandParameter.ToString();
                           filter.FilterAuthor = false;
                           filter.FilterClient = false;
                           filter.FilterText = true;
                           filter.IsExcludeFilter = true;
                           AppController.Current.AllFilters.Add(filter);
                           AppController.Current.CurrentView.subscribeToFilter(filter.Id);
                       }
                   }
               }
        }

        void menuItemOpenInSearch_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                TwitterItem item = this.DataContext as TwitterItem;
                if (item != null)
                {
                    AppController.Current.openSearchWindow(menuItem.CommandParameter.ToString(), item.RetrievingAccount);
                }

                AppController.Current.openSearchWindow(menuItem.CommandParameter.ToString(), AppController.Current.AllTwitterAccounts[0]);
            }
        }

        void menuItemOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string url = menuItem.CommandParameter as string;
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(url);
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
            }
        }

        void menuItemCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                if (menuItem != null)
                {
                    string url = menuItem.CommandParameter as string;
                    if (!string.IsNullOrEmpty(url))
                    {
                        Clipboard.SetText(url);
                    }
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Copying to clipboard failed");
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                externalServiceParameter parameter = menuItem.CommandParameter as externalServiceParameter;
                parameter.storeService.SendNow(parameter.item, parameter.link);
            }
        }
        

       

        public List<TextSubTypes.ISubType> ItemText
        {
            get { return (List<TextSubTypes.ISubType>)GetValue(ItemTextProperty); }
            set { SetValue(ItemTextProperty, value); }
        }

        public static readonly DependencyProperty ItemTextProperty =
                DependencyProperty.Register(
                "ItemText",
                typeof(List<TextSubTypes.ISubType>),
                typeof(TextblockItem),
                new FrameworkPropertyMetadata(new List<TextSubTypes.ISubType>(), new PropertyChangedCallback(OnItemTextChanged)));

     

        private static void OnItemTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            List<TextSubTypes.ISubType> text = args.NewValue as List<TextSubTypes.ISubType>;
            if (text == null)
            {
                text = new List<TextSubTypes.ISubType>();
            }


            if (text.Count > 0)
            {
                DateTime start = DateTime.Now;
                TextblockItem textblock = (TextblockItem)obj;
                textblock.Inlines.Clear();
                textblock.generateElementsTextblock(text);
                return;
            }   
        
        }


        public TwitterItem.Client ItemSource
        {
            get { return (TwitterItem.Client)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemSourceProperty =
                DependencyProperty.Register(
                "ItemSource",
                typeof(TwitterItem.Client),
                typeof(TextblockItem),
                new FrameworkPropertyMetadata(new TwitterItem.Client(), new PropertyChangedCallback(OnItemSourceChanged)));

        private static void OnItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TwitterItem.Client client = args.NewValue as TwitterItem.Client;
            if (client == null)
            {
                return;
            }


            if (!string.IsNullOrEmpty(client.Name))
            {
                TextblockItem textblock = (TextblockItem)obj;
                textblock.Inlines.Clear();

                TextBlock additonalInfo = new TextBlock();
                    additonalInfo.Name = "SourceClient";
                    additonalInfo.TextWrapping = TextWrapping.Wrap;
                    additonalInfo.FontSize = 9;
                    additonalInfo.FontStyle = FontStyles.Italic;

                    if (client.Name != "")
                    {
                        additonalInfo.Inlines.Add(" via ");
                        ContextMenu contextClient = new ContextMenu();

                        MenuItem menuItemMuteClient = new MenuItem();
                        menuItemMuteClient.Header = "Mute all tweets written with " + client.Name + " (will create a filter for current View)"; ;
                        menuItemMuteClient.CommandParameter = client.Name;
                        menuItemMuteClient.Click += new RoutedEventHandler(menuItemMuteClient_Click);
                        contextClient.Items.Add(menuItemMuteClient);

                        TextBlock sourceNameTextblock = new TextBlock();
                        sourceNameTextblock.TextWrapping = TextWrapping.Wrap;
                        sourceNameTextblock.Text = client.Name;
                        sourceNameTextblock.Foreground = Brushes.DarkKhaki;
                        if (!string.IsNullOrEmpty(client.Url))
                        {
                            Hyperlink sourceClient = new Hyperlink();
                            sourceClient.TextDecorations = null;
                            sourceClient.TargetName = client.Name;
                            try
                            {
                                sourceClient.NavigateUri = new Uri(client.Url);
                            }
                            catch
                            {
                                sourceClient.NavigateUri = new Uri("http://twitter.com/");
                            }
                            sourceClient.Click += new RoutedEventHandler(sourceClient_Click);

                            MenuItem menuItemOpenClientHomepage = new MenuItem();
                            menuItemOpenClientHomepage.Header = "Open homepage of " + client.Name;
                            menuItemOpenClientHomepage.CommandParameter = client.Url;
                            menuItemOpenClientHomepage.Click += new RoutedEventHandler(menuItemOpenClientHomepage_Click);
                            contextClient.Items.Add(menuItemOpenClientHomepage);

                            sourceClient.Inlines.Add(sourceNameTextblock);
                            additonalInfo.Inlines.Add(sourceClient);
                        }
                        else
                        {
                            additonalInfo.Inlines.Add(sourceNameTextblock);
                        }
                        sourceNameTextblock.ContextMenu = contextClient;
                    }

                    textblock.Inlines.Add(additonalInfo);
                         
            }
        }

        static void menuItemOpenClientHomepage_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(menuItem.CommandParameter.ToString());
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
        }

        static void menuItemMuteClient_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                if (AppController.Current.CurrentView != null)
                {
                    if (AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == menuItem.CommandParameter.ToString().ToLower() && f.FilterClient).Count() > 0)
                    {
                        Filter existingFilter = AppController.Current.AllFilters.Where(f => f.FilterString.ToLower() == menuItem.CommandParameter.ToString().ToLower() && f.FilterClient).First();
                        if (existingFilter != null)
                        {
                            if (!AppController.Current.CurrentView.subscribedFilter.Contains(existingFilter))
                            {
                                AppController.Current.CurrentView.subscribeToFilter(existingFilter.Id);
                            }
                        }
                    }
                    else
                    {
                        Filter filter = new Filter();
                        filter.Name = "Mute client " + menuItem.CommandParameter.ToString();
                        filter.FilterString = menuItem.CommandParameter.ToString();
                        filter.FilterAuthor = false;
                        filter.FilterClient = true;
                        filter.FilterText = false;
                        filter.IsExcludeFilter = true;
                        filter.FilterRetweeter = false;
                        AppController.Current.AllFilters.Add(filter);
                        AppController.Current.CurrentView.subscribeToFilter(filter.Id);
                    }
                }
            }
        }

        static void sourceClient_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.Source as Hyperlink;
            if (link != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
                }
                catch (Exception exp)
                {
                    AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                    AppController.Current.Logger.writeToLogfile(exp);
                }
            }
        }

        static void twitterUser_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            IItem iitem = link.DataContext as IItem;
            if (iitem.GetType() == typeof(TwitterItem))
            {
                try
                {
                    Person person = API.TweetSharpConverter.getPersonFromLogin(link.TargetName, link.DataContext as AccountTwitter);
                    UserInterface.ShowUser showUser = new UserInterface.ShowUser();
                    showUser.setPerson(person, link.DataContext as AccountTwitter);
                    showUser.Show();
                }
                catch
                {
                    try
                    {
                        System.Diagnostics.Process.Start("http://www.twitter.com/" + link.TargetName);
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
            }
            else if (iitem.GetType() == typeof(ApnItem))
            {
                try
                {
                    ApnItem item = iitem as ApnItem;
                    UserInterface.Apn.UserInfo userInfo = new UserInterface.Apn.UserInfo(item.receivingAccount, link.TargetName);
                    userInfo.Show();
                }
                catch
                {
                    try
                    {
                        System.Diagnostics.Process.Start("http://alpha.app.net/" + link.TargetName);
                    }
                    catch (Exception exp)
                    {
                        AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                        AppController.Current.Logger.writeToLogfile(exp);
                    }
                }
            }

        }

        static void hashtag_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            IItem iitem = link.DataContext as IItem;
            if (iitem.GetType() == typeof(TwitterItem))
            {
                TwitterItem item = iitem as TwitterItem;
                if (item != null)
                {
                    AppController.Current.openSearchWindow("#" + link.TargetName, item.RetrievingAccount);
                }

                AppController.Current.openSearchWindow("#" + link.TargetName, AppController.Current.AllTwitterAccounts[0]);
            }
            else if (iitem.GetType() == typeof(ApnItem))
            {
                System.Diagnostics.Process.Start("https://alpha.app.net/hashtags/" + link.TargetName);
            }
        }

        static void link_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            try
            {
                System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private static BitmapImage GetImageFromURL(string Url)
        {
            BitmapImage bImage = new BitmapImage();
            bImage.BeginInit();
            bImage.UriSource = new Uri(Url);
            bImage.CacheOption = BitmapCacheOption.OnLoad;
            bImage.EndInit();
            
            
            return bImage;

        }
    }
}
