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
using Nymphicus.Model;
using Nymphicus.Controls;
using Nymphicus.UserInterface;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;


namespace Nymphicus
{
  
    public class GeneralFunctions
    {
     
        public class treeViewForViews
        {
            public static void setAllCheckboxesInTreeview(TreeView treeview, bool isCheckedValue)
            {
                ItemCollection itemCollection = treeview.Items;
                foreach (TreeViewItem item in itemCollection)
                {
                    if (item.HasItems)
                    {
                        foreach (object item2 in item.Items)
                        {
                            if (IsType(item2, "EditableCheckbox"))
                            {
                                EditableCheckbox editableCheckbox = item2 as EditableCheckbox;
                                CheckBox checkbox = editableCheckbox.checkboxContent;
                                checkbox.IsChecked = isCheckedValue;
                            }
                            else if (IsType(item2, "CheckBox"))
                            {
                                CheckBox checkbox = item2 as CheckBox;
                                checkbox.IsChecked = isCheckedValue;


                            }
                            else if (IsType(item2, "TreeViewItem"))
                            {
                                TreeViewItem lastLevel = item2 as TreeViewItem;
                                foreach (object item3 in lastLevel.Items)
                                {
                                    if (IsType(item3, "CheckBox"))
                                    {
                                        CheckBox checkbox = item3 as CheckBox;
                                        checkbox.IsChecked = isCheckedValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static void invertAllCheckboxesInTreeview(TreeView treeview)
            {
                ItemCollection itemCollection = treeview.Items;
                foreach (TreeViewItem item in itemCollection)
                {
                    if (item.HasItems)
                    {
                        foreach (object item2 in item.Items)
                        {
                            if (IsType(item2, "EditableCheckbox"))
                            {
                                EditableCheckbox editableCheckbox = item2 as EditableCheckbox;
                                CheckBox checkbox = editableCheckbox.checkboxContent;
                                checkbox.IsChecked = !checkbox.IsChecked;
                            }
                            else if (IsType(item2, "CheckBox"))
                            {
                                CheckBox checkbox = item2 as CheckBox;
                                checkbox.IsChecked = !checkbox.IsChecked;


                            }
                            else if (IsType(item2, "TreeViewItem"))
                            {
                                TreeViewItem lastLevel = item2 as TreeViewItem;
                                foreach (object item3 in lastLevel.Items)
                                {
                                    if (IsType(item3, "CheckBox"))
                                    {
                                        CheckBox checkbox = item3 as CheckBox;
                                        checkbox.IsChecked = !checkbox.IsChecked;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static void setAllCheckboxesOfOneKindInTreeview(TreeView treeview, string viewType, bool isCheckedValue)
            {
                viewType = viewType + "_";
                ItemCollection itemCollection = treeview.Items;
                foreach (TreeViewItem item in itemCollection)
                {
                    if (item.HasItems)
                    {
                        foreach (object item2 in item.Items)
                        {
                            if (IsType(item2, "EditableCheckbox"))
                            {
                                EditableCheckbox editableCheckbox = item2 as EditableCheckbox;
                                CheckBox checkbox = editableCheckbox.checkboxContent;
                                if (checkbox.Name.StartsWith(viewType))
                                {
                                    checkbox.IsChecked = isCheckedValue;
                                }

                            }
                            else if (IsType(item2, "CheckBox"))
                            {
                                CheckBox checkbox = item2 as CheckBox;
                                if (checkbox.Name.StartsWith(viewType))
                                {
                                    checkbox.IsChecked = isCheckedValue;
                                }
                            }
                            else if (IsType(item2, "TreeViewItem"))
                            {
                                TreeViewItem lastLevel = item2 as TreeViewItem;
                                foreach (object item3 in lastLevel.Items)
                                {
                                    if (IsType(item3, "CheckBox"))
                                    {
                                        CheckBox checkbox = item3 as CheckBox;
                                        if (checkbox.Name.StartsWith(viewType))
                                        {
                                            checkbox.IsChecked = isCheckedValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            public static void applyViewSettings(View view, ItemCollection itemCollection)
            {
                if (view == null || itemCollection == null)
                {
                    AppController.Current.Logger.writeToLogfile("applyViewSettings with null parameter");
                    return;
                }

                foreach (TreeViewItem item in itemCollection)
                {
                    if (item.HasItems)
                    {
                        foreach (object item2 in item.Items)
                        {
                            if (IsType(item2, "EditableCheckbox"))
                            {
                                EditableCheckbox editableCheckbox = item2 as EditableCheckbox;
                                CheckBox checkbox = editableCheckbox.checkboxContent;
                                decimal id = getIdOfCheckboxName(checkbox.Name);
                                if (checkbox.Name.StartsWith("filter_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFilter(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFilter(id);
                                    }
                                }
                            }
                            else if (IsType(item2, "CheckBox"))
                            {
                                CheckBox checkbox = item2 as CheckBox;
                                decimal id = getIdOfCheckboxName(checkbox.Name);
                                #region Twitter
                                if (checkbox.Name.StartsWith("timeline_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToTimeline(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromTimeline(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("mentions_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToMentions(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromMentions(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("directMessages_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToDirectMessages(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromDirectMessages(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("retweets_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToRetweets(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromRetweets(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("favorites_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFavorites(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFavorites(id);
                                    }
                                }
                                #endregion
                                // App.net start
                                else if (checkbox.Name.StartsWith("apnPersonalStream_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToApnPersonalStream(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromApnPersonalStream(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("apnMentions_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToApnMentions(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromApnMentions(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("apnPrivateMessages_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToApnPrivateMessages(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromApnPrivateMessages(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("filter_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFilter(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFilter(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("statusMessages_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbStatusMessages(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbStatusMessages(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("links_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbLinks(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbLinks(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("photos_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbPhotos(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbPhotos(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("videos_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbVideos(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbVideos(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("events_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbEvents(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbEvents(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("checkIns_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbCheckIns(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbCheckIns(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("notes_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToFbNotes(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromFbNotes(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("qfmRecos_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToQuoteFmRecommendations(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromQuoteFmRecommendations(id);
                                    }
                                }
                                else if (checkbox.Name.StartsWith("qfmCat_"))
                                {
                                    if (checkbox.IsChecked.Value)
                                    {
                                        view.subscribeToQuoteFmCategory(id);
                                    }
                                    else
                                    {
                                        view.unsubsribeFromQuoteFmCategory(id);
                                    }
                                }


                            }
                            else if (IsType(item2, "TreeViewItem"))
                            {
                                TreeViewItem lastLevel = item2 as TreeViewItem;
                                foreach (object item3 in lastLevel.Items)
                                {
                                    if (IsType(item3, "CheckBox"))
                                    {
                                        CheckBox checkbox = item3 as CheckBox;
                                        decimal id = getIdOfCheckboxName(checkbox.Name);
                                        if (checkbox.Name.StartsWith("search_"))
                                        {
                                            if (checkbox.IsChecked.Value)
                                            {
                                                view.subscribeToSearch(id);
                                            }
                                            else
                                            {
                                                view.unsubsribeFromSearch(id);
                                            }
                                        }
                                        else if (checkbox.Name.StartsWith("list_"))
                                        {
                                            if (checkbox.IsChecked.Value)
                                            {
                                                view.subscribeToList(id);
                                            }
                                            else
                                            {
                                                view.unsubsribeFromList(id);
                                            }
                                        }
                                        else if (checkbox.Name.StartsWith("greasy"))
                                        {
                                            if (checkbox.IsChecked.Value)
                                            {
                                                view.subscribeToList(id);
                                            }
                                            else
                                            {
                                                view.unsubsribeFromList(id);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static decimal getIdOfCheckboxName(string checkboxName)
            {
                decimal id = 0;
                string[] split = checkboxName.Split('_');
                if (split.Length == 2)
                {
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

            public static bool IsType(object obj, string type)
            {
                return (obj.GetType().ToString().ToLower().EndsWith(type.ToLower()));
            }

            public static TreeView CreateViewTreeView(View view, TreeView treeview)
            {
                return CreateViewTreeView(view, treeview, true);
            }
            public static TreeView CreateViewTreeView(View view, TreeView treeview, bool IncludeFilter)
            {

                treeview.Items.Clear();
                if (view == null)
                {
                    return null;
                }

                foreach (IAccount iaccount in AppController.Current.AllAccounts)
                {
                    if (view.isTwitterOnlyView)
                    {
                        if (iaccount.GetType() == typeof(AccountTwitter))
                        {
                            #region Twitter
                            AccountTwitter account = iaccount as AccountTwitter;
                            string accountIdString = account.Login.Id.ToString();

                            TreeViewItem accountItem = new TreeViewItem();
                            accountItem.Name = "account_" + accountIdString;
                            accountItem.ToolTip = account.Login.NameAndLogin;
                            accountItem.Header = "@" + account.Login.Username;
                            accountItem.IsExpanded = true;

                            CheckBox timeline = new CheckBox();
                            timeline.Name = "timeline_" + accountIdString;
                            timeline.Content = "Timeline";
                            if (view.subscribedTimelines.Contains(account.Login.Id))
                            {
                                timeline.IsChecked = true;
                            }
                            accountItem.Items.Add(timeline);

                            CheckBox mentions = new CheckBox();
                            mentions.Name = "mentions_" + accountIdString;
                            mentions.Content = "Mentions";
                            if (view.subscribedMentions.Contains(account.Login.Id))
                            {
                                mentions.IsChecked = true;
                            }
                            accountItem.Items.Add(mentions);

                            CheckBox directMessages = new CheckBox();
                            directMessages.Name = "directMessages_" + accountIdString;
                            directMessages.Content = "Direct messages";
                            if (view.subscribedDirectMessages.Contains(account.Login.Id))
                            {
                                directMessages.IsChecked = true;
                            }
                            accountItem.Items.Add(directMessages);

                            CheckBox retweets = new CheckBox();
                            retweets.Name = "retweets_" + accountIdString;
                            retweets.Content = "Retweets";
                            if (view.subscribedRetweets.Contains(account.Login.Id))
                            {
                                retweets.IsChecked = true;
                            }
                            accountItem.Items.Add(retweets);

                            CheckBox favorites = new CheckBox();
                            favorites.Name = "favorites_" + accountIdString;
                            favorites.Content = "Favorites";
                            if (view.subscribedFavorites.Contains(account.Login.Id))
                            {
                                favorites.IsChecked = true;
                            }
                            accountItem.Items.Add(favorites);

                            if (account.Lists.Count > 0)
                            {
                                TreeViewItem lists = new TreeViewItem();
                                lists.Name = "lists_" + accountIdString;
                                lists.Header = "Lists";
                                lists.IsExpanded = true;

                                foreach (TweetList list in account.Lists)
                                {
                                    CheckBox listCheckbox = new CheckBox();
                                    listCheckbox.Name = "list_" + list.Id.ToString();
                                    listCheckbox.Content = list.NameAndCreator;
                                    listCheckbox.ToolTip = list.Description;
                                    if (view.subscribedLists.Contains(list.Id))
                                    {
                                        listCheckbox.IsChecked = true;
                                    }
                                    lists.Items.Add(listCheckbox);
                                }
                                accountItem.Items.Add(lists);
                            }

                            if (account.Searches.Count > 0)
                            {
                                TreeViewItem searches = new TreeViewItem();
                                searches.Name = "searches_" + accountIdString;
                                searches.Header = "Searches";
                                searches.IsExpanded = true;
                                foreach (Search search in account.Searches)
                                {
                                    CheckBox searchCheckbox = new CheckBox();
                                    searchCheckbox.Name = "search_" + search.Id.ToString();
                                    searchCheckbox.Content = search.name;
                                    if (view.subscribedSearches.Contains(search.Id))
                                    {
                                        searchCheckbox.IsChecked = true;
                                    }
                                    searches.Items.Add(searchCheckbox);
                                }
                                accountItem.Items.Add(searches);
                            }

                            treeview.Items.Add(accountItem);
                            #endregion
                        }
                    }
                    else
                    {
                        if (iaccount.GetType() == typeof(AccountFacebook))
                        {
                            #region Facebook
                            AccountFacebook account = iaccount as AccountFacebook;
                            string accountIdString = account.Id.ToString();

                            TreeViewItem accountItem = new TreeViewItem();
                            accountItem.Name = "fbAccount_" + accountIdString;
                            accountItem.ToolTip = account.username;
                            accountItem.Header = "Facebook " + account.username;
                            accountItem.IsExpanded = true;

                            CheckBox statusMessages = new CheckBox();
                            statusMessages.Name = "statusMessages_" + accountIdString;
                            statusMessages.Content = "Status Messages";
                            if (view.subscribedFbStatusMessages.Contains(account.Id))
                            {
                                statusMessages.IsChecked = true;
                            }
                            accountItem.Items.Add(statusMessages);

                            CheckBox links = new CheckBox();
                            links.Name = "links_" + accountIdString;
                            links.Content = "Links";
                            if (view.subscribedFbLinks.Contains(account.Id))
                            {
                                links.IsChecked = true;
                            }
                            accountItem.Items.Add(links);

                            CheckBox videos = new CheckBox();
                            videos.Name = "videos_" + accountIdString;
                            videos.Content = "Videos";
                            if (view.subscribedFbVideos.Contains(account.Id))
                            {
                                videos.IsChecked = true;
                            }
                            accountItem.Items.Add(videos);

                            CheckBox photos = new CheckBox();
                            photos.Name = "photos_" + accountIdString;
                            photos.Content = "Photos";
                            if (view.subscribedFbPhotos.Contains(account.Id))
                            {
                                photos.IsChecked = true;
                            }
                            accountItem.Items.Add(photos);

                            CheckBox events = new CheckBox();
                            events.Name = "events_" + accountIdString;
                            events.Content = "Events";
                            if (view.subscribedFbEvents.Contains(account.Id))
                            {
                                events.IsChecked = true;
                            }
                            accountItem.Items.Add(events);

                            CheckBox checkIns = new CheckBox();
                            checkIns.Name = "checkIns_" + accountIdString;
                            checkIns.Content = "CheckIns";
                            if (view.subscribedFbCheckIns.Contains(account.Id))
                            {
                                checkIns.IsChecked = true;
                            }
                            accountItem.Items.Add(checkIns);

                            CheckBox notes = new CheckBox();
                            notes.Name = "notes_" + accountIdString;
                            notes.Content = "Notes";
                            if (view.subscribedFbNotes.Contains(account.Id))
                            {
                                notes.IsChecked = true;
                            }
                            accountItem.Items.Add(notes);

                            treeview.Items.Add(accountItem);
                            #endregion
                        }
                        else if (iaccount.GetType() == typeof(AccountAppDotNet))
                        {
                            #region App.net
                            AccountAppDotNet account = iaccount as AccountAppDotNet;
                            string accountIdString = account.Id.ToString();

                            TreeViewItem accountItem = new TreeViewItem();
                            accountItem.Name = "apnPersonalStream_" + accountIdString;
                            accountItem.ToolTip = account.username;
                            accountItem.Header = "App.net @" + account.username;
                            accountItem.IsExpanded = true;

                            CheckBox personalStream = new CheckBox();
                            personalStream.Name = "apnPersonalStream_" + accountIdString;
                            personalStream.Content = "My stream";
                            if (view.subscribedApnPersonalStreams.Contains(account.Id))
                            {
                                personalStream.IsChecked = true;
                            }
                            accountItem.Items.Add(personalStream);

                            CheckBox privateMessages = new CheckBox();
                            privateMessages.Name = "apnPrivateMessages_" + accountIdString;
                            privateMessages.Content = "Private Messages";
                            if (view.subscribedApnPrivateMessages.Contains(account.Id))
                            {
                                privateMessages.IsChecked = true;
                            }
                            accountItem.Items.Add(privateMessages);

                            CheckBox mentions = new CheckBox();
                            mentions.Name = "apnMentions_" + accountIdString;
                            mentions.Content = "Mentions";
                            if (view.subscribedApnMentions.Contains(account.Id))
                            {
                                mentions.IsChecked = true;
                            }
                            accountItem.Items.Add(mentions);

                            CheckBox reposts = new CheckBox();
                            reposts.Name = "apnReposts_" + accountIdString;
                            reposts.Content = "Reposts";
                            reposts.IsEnabled = false;
                            if (false && view.subscribedApnPersonalStreams.Contains(account.Id))
                            {
                                personalStream.IsChecked = true;
                            }
                            accountItem.Items.Add(reposts);

                            treeview.Items.Add(accountItem);
                            #endregion
                        }
                        
                        else if (iaccount.GetType() == typeof(AccountQuoteFM))
                        {
                            # region QUOTE.fm
                            AccountQuoteFM account = iaccount as AccountQuoteFM;
                            string accountIdString = account.User.Id.ToString();

                            TreeViewItem accountItem = new TreeViewItem();
                            accountItem.Name = "account_" + accountIdString;
                            accountItem.ToolTip = account.User.Fullname;
                            accountItem.Header = "QUOTE.fm " + account.User.username;
                            accountItem.IsExpanded = true;

                            CheckBox recommendations = new CheckBox();
                            recommendations.Name = "qfmRecos_" + accountIdString;
                            recommendations.Content = "Recommendations";
                            if (view.subscribedQuoteFmRecommendations.Contains(account.User.Id))
                            {
                                recommendations.IsChecked = true;
                            }
                            accountItem.Items.Add(recommendations);

                            treeview.Items.Add(accountItem);
                        }
                    }

                    if (AppController.Current.HasQuoteFmAccounts && QuoteFmCategories.Categories != null && !view.isTwitterOnlyView)
                    {
                        TreeViewItem categorytItem = new TreeViewItem();
                        categorytItem.Name = "qfmCategories";
                        categorytItem.ToolTip = "QUOTE.fm categories";
                        categorytItem.Header = "QUOTE.fm categories";
                        categorytItem.IsExpanded = true;

                        foreach (QuoteSharp.Category category in QuoteFmCategories.Categories.catagories.entities)
                        {
                            CheckBox checkboxCategory = new CheckBox();
                            checkboxCategory.Name = "qfmCat_" + category.id.ToString();
                            checkboxCategory.Content = category.name;
                            checkboxCategory.ToolTip = category.name;
                            if (view.subscribedQuoteFmCategories.Contains(category.id))
                            {
                                checkboxCategory.IsChecked = true;
                            }
                            categorytItem.Items.Add(checkboxCategory);
                        }

                        treeview.Items.Add(categorytItem);
                            #endregion
                    }
                }

                TreeViewItem filters = new TreeViewItem();
                filters.Name = "filters";
                filters.Header = "Filter";
                filters.IsExpanded = true;
                filters.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
                foreach (Filter filter in AppController.Current.AllFilters)
                {
                    EditableCheckbox filterCheckbox = new EditableCheckbox();
                    filterCheckbox.checkboxContent.Name = "filter_" + filter.Id.ToString();
                    filterCheckbox.checkboxContent.Content = filter.Name;
                    if (view.subscribedFilter.Contains(filter))
                    {
                        filterCheckbox.checkboxContent.IsChecked = true;
                    }
                    filterCheckbox.buttonEdit.CommandParameter = filter;
                    filterCheckbox.buttonEdit.Click += new RoutedEventHandler(buttonEdit_Click);


                    filters.Items.Add(filterCheckbox);
                }
                Button createNewFilter = new Button();
                createNewFilter.Content = "Create new filter";
                createNewFilter.Click += new RoutedEventHandler(createNewFilter_Click);
                filters.Items.Add(createNewFilter);

                treeview.Items.Add(filters);

                return treeview;
            }

            static void buttonEdit_Click(object sender, RoutedEventArgs e)
            {
                Button button = sender as Button;
                if (button != null)
                {
                    Filter filter = button.CommandParameter as Filter;
                    if (filter != null)
                    {
                        EditFilter editFilter = new EditFilter();
                        editFilter.LoadFilter(filter);
                        editFilter.Show();
                    }
                }
            }



            static void createNewFilter_Click(object sender, RoutedEventArgs e)
            {
                EditFilter newFilter = new EditFilter();
                newFilter.Show();
            }


        }

        public static Links find_links_in_text(string text)
        {
            Links links = new Links();
            Regex regex_http = new Regex(@"((http):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase);
            Regex regex_https = new Regex(@"((https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase);
            Regex regex_ftp = new Regex(@"((ftp):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase);

            MatchCollection http_matches = regex_http.Matches(text);
            foreach (Match http_match in http_matches)
            {
                links.http_links.Add(http_match.Value);
            }

            MatchCollection https_matches = regex_https.Matches(text);
            foreach (Match https_match in https_matches)
            {
                links.https_links.Add(https_match.Value);
            }

            MatchCollection ftp_matches = regex_ftp.Matches(text);
            foreach (Match ftp_match in ftp_matches)
            {
                links.ftp_links.Add(ftp_match.Value);
            }

            return links;
        }

        public class Links
        {
            public Links()
            {
                http_links = new List<string>();
                https_links = new List<string>();
                ftp_links = new List<string>();
            }

            public List<string> http_links { get; set; }
            public List<string> https_links { get; set; }
            public List<string> ftp_links { get; set; }
        }


        public class ReadStates
        {
            public static bool GetCurrentReadStateByTweetId(decimal id)
            {
                return AppController.Current.ReadTweets.Contains(id);
            }

            public static void AddTweetAsBeingRead(decimal id)
        {
            if (!AppController.Current.ReadTweets.Contains(id))
            {
                AppController.Current.ReadTweets.Add(id);
            }
        }

            public static void RemoveTweetAsBeingRead(decimal id)
            {
            if (AppController.Current.ReadTweets.Contains(id))
            {
                try
                {
                    AppController.Current.ReadTweets.Remove(id);
                }
                catch
                {
                    AppController.Current.Logger.writeToLogfile("Unable to save unread state");
                }
            }
            }

            public static void StoreTweetReadState(decimal id, bool isRead)
            {
                if(id > 0) {
                    if (isRead)
                    {
                        AddTweetAsBeingRead(id);
                    }
                    else
                    {
                        RemoveTweetAsBeingRead(id);
                    }
                }
            }
        }

    }
    public static class DateTimeUtils
    {
        public static DateTime ConvertFromUnixTimestamp(this long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1).ToUniversalTime();
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }
    }
}
