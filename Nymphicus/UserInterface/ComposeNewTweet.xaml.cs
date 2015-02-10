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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Threading;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for ComposeNewTweet.xaml
    /// </summary>
    public partial class ComposeNewTweet : Window
    {

        public static RoutedCommand Cancel = new RoutedCommand();
        //public static RoutedCommand ShortenLinks = new RoutedCommand();
        public static RoutedCommand UploadImage = new RoutedCommand();
        public static RoutedCommand SelectAccount = new RoutedCommand();

        private decimal inReplyToId;
        private string directMessageReceiver;
        private bool useTwitlonger = false;
        private TwitterItem toBeEditedItem;
        private string upload_image_url { get; set; }
        private int characters_left { get; set; }

        private BackgroundWorker backgroundWorkerUploadImage;

        

        public ComposeNewTweet()
        {
            InitializeComponent();

            Cancel.InputGestures.Add(new KeyGesture(Key.Escape));
           // ShortenLinks.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            UploadImage.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            SelectAccount.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            characters_left = 140;

            textBoxTweet.textBoxContent.TextChanged += new TextChangedEventHandler(textBoxTweet_TextChanged);
            textBoxTweet.textBoxContent.KeyDown += new KeyEventHandler(textBoxTweet_KeyDown);

            try
            {
                while (AppController.Current.AllKnownUsernames.Contains(null))
                {
                    AppController.Current.AllKnownUsernames.Remove(null);
                }

                while (AppController.Current.AllKnownHashtags.Contains(null))
                {
                    AppController.Current.AllKnownHashtags.Remove(null);
                }
            }
            catch { }

            textBoxTweet.Usernames = AppController.Current.AllKnownUsernames;
            textBoxTweet.Hashtags = AppController.Current.AllKnownHashtags;

            backgroundWorkerUploadImage = new BackgroundWorker();
            backgroundWorkerUploadImage.WorkerSupportsCancellation = true;
            backgroundWorkerUploadImage.WorkerReportsProgress = true;
            backgroundWorkerUploadImage.DoWork += new DoWorkEventHandler(backgroundWorkerUploadImage_DoWork);
            backgroundWorkerUploadImage.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerUploadImage_ProgressChanged);


            if (AppController.Current.AllAccounts.Count < 2)
            {
                comboBoxAccount.Visibility = System.Windows.Visibility.Collapsed;
            }
            try
            {
                if (AppController.Current.mainWindow.WindowState == System.Windows.WindowState.Normal)
                {
                    this.Top = AppController.Current.mainWindow.Top + 5;
                    this.Left = AppController.Current.mainWindow.Left + 5;
                }
            }
            catch
            {

            }
            textBoxTweet.textBoxContent.Focus();
        }

        #region keyboard shortcuts

        private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        /*private void ShortenLinksExecuted(object sender, ExecutedRoutedEventArgs e)
        {
           // buttonShortenUrls_Click(null, null);
        } */
        private void UploadImageExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonAddImage_Click(null, null);
        }
        private void SelectAccountExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            comboBoxAccount.comboBoxAccounts.Focus();
            comboBoxAccount.comboBoxAccounts.IsDropDownOpen = true;
        }


        #endregion

        #region Old upload code


        class UploadArguments
        {
            public string filepath;
            public AccountTwitter account;
        }

        void backgroundWorkerUploadImage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string link = e.UserState as string;
            if (!string.IsNullOrEmpty(link))
            {
                textBoxTweet.Text = textBoxTweet.Text.TrimEnd();
                if (textBoxTweet.Text.Length > 0)
                {
                    textBoxTweet.Text += " ";
                }
                textBoxTweet.Text += link;
            }
            this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.Visibility = Visibility.Collapsed));
            //this.Dispatcher.BeginInvoke(new Action(() => this.labelUploading.Visibility = Visibility.Collapsed));

        }

        void backgroundWorkerUploadImage_DoWork(object sender, DoWorkEventArgs e)
        {
            UploadArguments arguments = e.Argument as UploadArguments;

            API.ImageServices.ImageResponse uploadedUrl = null;
            uploadedUrl = AppController.Current.ActualImageService.Upload(arguments.filepath, arguments.account);
            if (uploadedUrl.Success)
            {
                backgroundWorkerUploadImage.ReportProgress(100, uploadedUrl.UrlFull);
            }
            else
            {
                MessageBox.Show(uploadedUrl.ErrorText, "Error in image upload");
                backgroundWorkerUploadImage.ReportProgress(100, "");
            }
        }

        #endregion

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonSendTweet_Click(object sender, RoutedEventArgs e)
        {
            if ((textBoxTweet.Text.Length > 0 && characters_left >= 0) || (characters_left < 0 && useTwitlonger))
            {
                AccountTwitter account;
                if (AppController.Current.AllAccounts.Count == 1)
                {
                    account = AppController.Current.AllAccounts[0] as AccountTwitter;
                }
                else
                {
                    account = comboBoxAccount.selectedAccount;
                }

                
                if (account != null)
                {
                    string text = textBoxTweet.Text;
                    TwitterItem sentItem;
                    ExternalServices.Twitlonger.TwitLongerResponse twitLongerResponse = new ExternalServices.Twitlonger.TwitLongerResponse();

                    this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.BusyContent = "Sending tweet..."));
                    this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.Visibility = Visibility.Visible));

                   /* if (Properties.Settings.Default.AutomaticUrlShortening)
                    {
                        text = AppController.Current.ActualLinkShortener.ShortenAllLinksInText(textBoxTweet.Text);
                    } */

                    if (useTwitlonger)
                    {
                        twitLongerResponse = ExternalServices.Twitlonger.Send(text, account);
                        if (!string.IsNullOrEmpty(twitLongerResponse.ErrorMessage))
                        {
                            MessageBox.Show(twitLongerResponse.ErrorMessage, "Sending to Twitlonger failed");
                            return;
                        }
                        else
                        {
                            text = twitLongerResponse.MessageText;
                            textBoxTweet.Text = text;
                        }
                    }

                    if (directMessageReceiver != null)
                    {
                        sentItem = AppController.Current.writeDirectMessage(account, text, directMessageReceiver);
                    }
                    else if (inReplyToId <= 0)
                    {
                        sentItem = AppController.Current.writeNewTweet(account, text, upload_image_url);
                    }
                    else
                    {
                        sentItem = AppController.Current.replyToTweet(account, text, inReplyToId, upload_image_url);
                    }

                    if (sentItem != null)
                    {
                        if (!string.IsNullOrEmpty(twitLongerResponse.MessageId))
                        {
                            ExternalServices.Twitlonger.IdCallback(sentItem.Id, twitLongerResponse.MessageId);
                        }
                        if (toBeEditedItem != null)
                        {
                            toBeEditedItem.DeleteThisTweet();
                        }
                        Close();
                    }
                    this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.Visibility = Visibility.Collapsed));
                }
            }
        }

        private void textBoxTweet_TextChanged(object sender, TextChangedEventArgs e)
        {
            update_characters_left();
            
            useTwitlonger = false;
            
    


        }

        private void update_characters_left()
        {
            characters_left = 140 - textBoxTweet.Text.Length;
            if (!string.IsNullOrEmpty(upload_image_url))
            {
                characters_left -= Nymphicus.Model.Twitter_Help_Configuration.characters_reserved_per_media;
            }

            GeneralFunctions.Links embedded_links = GeneralFunctions.find_links_in_text(textBoxTweet.Text);

            foreach (string http_link in embedded_links.http_links)
            {
                characters_left += http_link.Length;
                characters_left -= Twitter_Help_Configuration.short_url_length;
            }
            
            foreach (string https_link in embedded_links.https_links)
            {
                characters_left += https_link.Length;
                characters_left -= Twitter_Help_Configuration.short_url_length_https;
            }
            labelTextLength.Content = characters_left;

            if (characters_left == 0)
            {
                labelTextLength.Foreground = Brushes.YellowGreen;
                buttonSendTweet.IsEnabled = true;
            }
            else if (characters_left < 0 || characters_left >= 140)
            {

                if (characters_left < 0 && Properties.Settings.Default.ExtSrvUseTwitlonger && directMessageReceiver == null)
                {
                    labelTextLength.Foreground = Brushes.LightCoral;
                    buttonSendTweet.IsEnabled = true;
                    labelTextLength.Content = "Twitlonger (" + textBoxTweet.Text.Length.ToString() + ")";
                    useTwitlonger = true;
                }
                else
                {
                    labelTextLength.Foreground = Brushes.Red;
                    buttonSendTweet.IsEnabled = false;
                }
            }
            else
            {
                labelTextLength.Foreground = Brushes.Gray;
                buttonSendTweet.IsEnabled = true;
            }
        }

        public void isReplyToItem(TwitterItem item)
        {
            if (item == null)
            {
                return;
            }
            if (item.RetweetedItem != null)
            {
                item = item.RetweetedItem;
            }
            textBoxTweet.Text = "@" + item.Author.Username + " ";
            inReplyToId = item.Id;
            labelInReplyTo.Content = "In reply to @" + item.Author.Username;
            labelInReplyTo.ToolTip = item.Text;
            buttonSendTweet.Content = "Send reply";

            labelInReplyTo.Visibility = Visibility.Visible;
            List<string> alreadyShownUser = new List<string>();
            int positionOfCursor = textBoxTweet.Text.Length;
            int lengthOfSelection = 0;
            if (item.accountId != 0)
            {
                try
                {
                    alreadyShownUser.Add("@" + AppController.Current.getAccountForId(item.accountId).Login.Username.ToLower());
                }
                catch { }
            }
            alreadyShownUser.Add("@" + item.Author.Username.ToLower());
            if (item.ElementsInText != null)
            {
                foreach (TextSubTypes.ISubType element in item.ElementsInText)
                {
                    if (element.GetType() == typeof(TextSubTypes.User))
                    {
                        TextSubTypes.User userEnt = element as TextSubTypes.User;

                        if (!alreadyShownUser.Contains(userEnt.userName.ToLower()))
                        {
                            alreadyShownUser.Add(userEnt.userName.ToLower());
                            textBoxTweet.Text += userEnt.userName.TrimEnd(AppController.Current.TrimCharacters.ToArray()) + " ";
                            lengthOfSelection += userEnt.userName.Length + 2;
                        }
                    }
                }
            }
            textBoxTweet.textBoxContent.Select(positionOfCursor, lengthOfSelection);
            textBoxTweet.textBoxContent.Focus();
        }

        public void isDirectMessage(string receiver)
        {
            labelInReplyTo.Content = "Direct message to @" + receiver + " ";
            directMessageReceiver = receiver;
            buttonSendTweet.Content = "Send DM";
            labelInReplyTo.Visibility = Visibility.Visible;
            textBoxTweet.textBoxContent.Focus();
        }

        private void buttonAddImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "Images (*.png,*.jpg,*.gif,*.tif,*.bmp,*.pdf,*.xcf)|*.png;*.jpeg;*.jpg;*.gif;*.tif;*.tiff;*.bmp;*.pdf;*.xcf"; // Filter files by extension
            
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string file_fullpath = dlg.FileName;
                System.IO.FileInfo file_info = new System.IO.FileInfo(file_fullpath);
                if (file_info.Length > Model.Twitter_Help_Configuration.photo_size_limit)
                {
                    MessageBox.Show("Sorry, the image is to big. Twitter maximum upload limit is " + (Model.Twitter_Help_Configuration.photo_size_limit / 1024).ToString() + " KBytes.", "Image to big", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
   /*            this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.BusyContent = "Uploading image to " + AppController.Current.ActualImageService.Name + "..."));
                this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.Visibility = Visibility.Visible));
                //this.Dispatcher.BeginInvoke(new Action(() => this.labelUploading.Visibility = Visibility.Visible));
                AccountTwitter account;
                if (comboBoxAccount.selectedAccount != null)
                {
                    account = comboBoxAccount.selectedAccount as AccountTwitter;
                }
                else
                {
                    account = AppController.Current.AllAccounts[0] as AccountTwitter;
                }
              //  UploadArguments arguments = new UploadArguments();
              //  arguments.filepath = filename;
              //  arguments.account = account;
                */
                upload_image_url = file_fullpath;
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.UriSource = new Uri(upload_image_url);
                logo.EndInit();
                imageAddImageUploaded.Source = logo;
                imageAddImageUploaded.Visibility = Visibility.Visible;
                imageAddImage.Visibility = Visibility.Hidden;
                update_characters_left();
                // backgroundWorkerUploadImage.RunWorkerAsync(arguments);
            }
        }



        private void buttonShortenUrls_Click(object sender, RoutedEventArgs e)
        {
            // textBoxTweet.Text = AppController.Current.ActualLinkShortener.ShortenAllLinksInText(textBoxTweet.Text);
        }

        private void textBoxTweet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == (Key.Return & Key.LeftCtrl) || e.Key == (Key.Enter & Key.LeftCtrl) || e.Key == (Key.Return & Key.RightCtrl) || e.Key == (Key.Enter & Key.RightCtrl))
            {
                if (buttonSendTweet.IsEnabled)
                {
                    buttonSendTweet_Click(null, null);
                }
            }
        }

        public void selectAccount(AccountTwitter account)
        {
            if (account != null)
            {
                comboBoxAccount.comboBoxAccounts.SelectedItem = account;
            }
        }

        public void setToBeEditedItem(TwitterItem item)
        {
            toBeEditedItem = item;
            if (item != null)
            {
                textBoxTweet.textBoxContent.Text = item.Text;
                labelInReplyTo.Content = "Editing tweet - old version will be deleted";
                labelInReplyTo.Visibility = Visibility.Visible;
                comboBoxAccount.comboBoxAccounts.SelectedItem = item.OwnAccountHavingWrittenThisTweet;
            }
        }

        private void buttonAddImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            upload_image_url = null;
            imageAddImageUploaded.Visibility = Visibility.Hidden;
            imageAddImage.Visibility = Visibility.Visible;
        }
    }
}
