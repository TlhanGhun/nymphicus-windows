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

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for DebugStatistics.xaml
    /// </summary>
    public partial class DebugStatistics : Window
    {
        private int NumberOfFacebookItems {get;set;}
        private int NumberOfTwitterItems {get;set;}
        private int NumberOfItems {
            get
                {
                return (NumberOfFacebookItems + NumberOfTwitterItems);
            }
        }


        public DebugStatistics()
        {
            InitializeComponent();

            CreateStatistics();
   

        }

        private void CreateStatistics()
        {
            textBoxStats.Text = "";

            textBoxStats.Text += AppController.Current.AllAccounts.Count().ToString() + " accounts in general";
            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "\r\n";

            textBoxStats.Text += "***** Twitter *****\r\n";
            textBoxStats.Text += AppController.Current.AllTwitterAccounts.Count().ToString() + " Twitter accounts";
            textBoxStats.Text += "\r\n";
            textBoxStats.Text += AppController.Current.AllPersons.Count().ToString() + " persons in general";
            textBoxStats.Text += "\r\n";
            foreach (AccountTwitter account in AppController.Current.AllTwitterAccounts)
            {
                textBoxStats.Text += "\r\n";
                textBoxStats.Text += " - @" + account.username + "\r\n";
                textBoxStats.Text += "   " + account.Timeline.Count().ToString() + " items in timeline\r\n";
                textBoxStats.Text += "     " + account.Timeline.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Mentions.Count().ToString() + " items in mentions\r\n";
                textBoxStats.Text += "     " + account.Mentions.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.DirectMessages.Count().ToString() + " items in direct messages\r\n";
                textBoxStats.Text += "     " + account.DirectMessages.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Retweets.Count().ToString() + " items in retweets\r\n";
                textBoxStats.Text += "     " + account.Retweets.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Retweeted.Count().ToString() + " items in retweeted\r\n";
                textBoxStats.Text += "     " + account.Retweeted.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";

                foreach (Search search in account.Searches)
                {
                    textBoxStats.Text += "    - " + search.Items.Count().ToString() + " items in search " + search.name + "\r\n";
                    textBoxStats.Text += "      busy since " + search.BusyTime.TotalSeconds.ToString() + " seconds\r\n";
                }
                foreach (TweetList list in account.Lists)
                {
                    textBoxStats.Text += "    o " + list.Items.Count().ToString() + " items in list " + list.name + "\r\n";
                    textBoxStats.Text += "      busy since " + list.BusyTime.TotalSeconds.ToString() + " seconds\r\n";
                }


            }

            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "***** Facebook *****\r\n";
            textBoxStats.Text += AppController.Current.AllFacebookAccounts.Count().ToString() + " Facebook accounts";
            textBoxStats.Text += "\r\n";
            foreach (AccountFacebook account in AppController.Current.AllFacebookAccounts)
            {
                textBoxStats.Text += "\r\n";
                textBoxStats.Text += " -- @" + account.FullName + "\r\n";
                textBoxStats.Text += "   " + account.StatusMessages.Count().ToString() + " items in status messages\r\n";
                textBoxStats.Text += "     " + account.StatusMessages.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Links.Count().ToString() + " items in status links\r\n";
                textBoxStats.Text += "     " + account.Links.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Videos.Count().ToString() + " items in status videos\r\n";
                textBoxStats.Text += "     " + account.Videos.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Photos.Count().ToString() + " items in status photos\r\n";
                textBoxStats.Text += "     " + account.Photos.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Events.Count().ToString() + " items in status events\r\n";
                textBoxStats.Text += "     " + account.Events.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.Notes.Count().ToString() + " items in status notes\r\n";
                textBoxStats.Text += "     " + account.Notes.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";
                textBoxStats.Text += "   " + account.CheckIns.Count().ToString() + " items in status checkins\r\n";
                textBoxStats.Text += "     " + account.CheckIns.Where(i => i.HumanReadableUpdateInProgress == true).Count().ToString() + " human readable thread active\r\n";

            }

            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "***** Views *****\r\n";
            textBoxStats.Text += AppController.Current.AllViews.Count().ToString() + " Views accounts";
            textBoxStats.Text += "\r\n";
            foreach (View view in AppController.Current.AllViews)
            {
                textBoxStats.Text += "\r\n";
                textBoxStats.Text += " -- @" + view.Name + "\r\n";
                textBoxStats.Text += view.Items.Count().ToString() + " items in Items\r\n";
                textBoxStats.Text += view.FilteredItems .Count().ToString() + " items in Filtered Items\r\n";
                textBoxStats.Text += view.OverflowItems.Count().ToString() + " items in Overflow items\r\n";
                textBoxStats.Text += view.ListResults.Count().ToString() + " items in List results\r\n";
            }

            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "\r\n";
            textBoxStats.Text += "****** AppController ******\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllFilters.Count().ToString() + " filters\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllImageServices.Count().ToString() + " image services\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllImagesInItems.Count().ToString() + " images in items\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllIStores.Count().ToString() + " data stores\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllKnownHashtags.Count().ToString() + " known hashtags\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllKnownUsernames.Count().ToString() + " known usernames\r\n";
            textBoxStats.Text += "  " + AppController.Current.AlllinkShortenerServices.Count().ToString() + " link shortener services\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllLists.Count().ToString() + " lists\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllSearches.Count().ToString() + " searches\r\n";
            textBoxStats.Text += "  " + AppController.Current.AllShortenedLinksInItems.Count().ToString() + " shoretened links in items\r\n";
            textBoxStats.Text += "  " + AppController.Current.accountColors.Count().ToString() + " account colores\r\n";
            textBoxStats.Text += "  " + AppController.Current.NotWorkingAccounts.Count().ToString() + " not working accounts (should always be zero!)\r\n";


        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            CreateStatistics();
        }
    }
}
