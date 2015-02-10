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
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for ComposeNewTweet.xaml
    /// </summary>
    public partial class ComposeNewApnPost : Window
    {

        public static RoutedCommand Cancel = new RoutedCommand();
        public static RoutedCommand ShortenLinks = new RoutedCommand();
        public static RoutedCommand UploadImage = new RoutedCommand();
        public static RoutedCommand SelectAccount = new RoutedCommand();

        private decimal inReplyToId;
        private ApnItem toBeEditedItem;
        private string privateMessageReceiver;
        private AppNetDotNet.Model.File toBeEmbeddedFile = null;
        public string path_to_be_uploaded_image { get; set; }

        private BackgroundWorker backgroundWorkerUploadImage;

        public ComposeNewApnPost()
        {
            InitializeComponent();

            Cancel.InputGestures.Add(new KeyGesture(Key.Escape));
            ShortenLinks.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            UploadImage.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            SelectAccount.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));

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

            autoCompeteTextbox_post.textBoxContent.TextChanged += textBoxContent_TextChanged;
            autoCompeteTextbox_post.textBoxContent.KeyDown += textBoxContent_KeyDown;

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
            autoCompeteTextbox_post.textBoxContent.Focus();
        }

        #region keyboard shortcuts

        private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        private void ShortenLinksExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonShortenUrls_Click(null, null);
        }
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

        void backgroundWorkerUploadImage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string link = e.UserState as string;
            AppNetDotNet.Model.File file = e.UserState as AppNetDotNet.Model.File;
            if (file != null && e.ProgressPercentage == 100)
            {
                autoCompeteTextbox_post.Text = autoCompeteTextbox_post.Text.TrimEnd();

                autoCompeteTextbox_post.Text += " photos.app.net/{post_id}/1";
                
                autoCompeteTextbox_post.Text += link;
                buttonAddImage.IsEnabled = false;
                toBeEmbeddedFile = file;
            }
            else if (!string.IsNullOrEmpty(link) && e.ProgressPercentage == 0)
            {
                MessageBox.Show(link, "Error in upload");
            }
            else
            {
                MessageBox.Show("Unknown Error in upload");
            }
            this.Dispatcher.BeginInvoke(new Action(() => this.progressBarUploading.Visibility = Visibility.Collapsed));
            //this.Dispatcher.BeginInvoke(new Action(() => this.labelUploading.Visibility = Visibility.Collapsed));

        }

        void backgroundWorkerUploadImage_DoWork(object sender, DoWorkEventArgs e)
        {
            UploadArguments arguments = e.Argument as UploadArguments;
            if (System.IO.File.Exists(arguments.filepath))
            {
                Tuple<File,ApiCallResponse> response = Files.create(arguments.account.accessToken, arguments.filepath, type:"com.nymphicusapp.image");
                if (response.Item2.success)
                {
                    backgroundWorkerUploadImage.ReportProgress(100, response.Item1);
                }
                else
                {
                    backgroundWorkerUploadImage.ReportProgress(0, response.Item2.errorMessage);
                }
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonSendPost_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(autoCompeteTextbox_post.textBoxContent.Text))
            {
                AccountAppDotNet account = comboBoxAccount.comboBoxAccounts.SelectedItem  as AccountAppDotNet;
                if (account != null)
                {
                    AppNetDotNet.Model.Entities entities = null;
                    string toBePostedText = autoCompeteTextbox_post.textBoxContent.Text;
                    if (autoCompeteTextbox_post.MarkdownLinksInText.Count() > 0)
                    {
                        entities = new AppNetDotNet.Model.Entities();
                        entities.links = new List<AppNetDotNet.Model.Entities.Link>();
                        entities.hashtags = null;
                        entities.mentions = null;
                        foreach (KeyValuePair<string, string> link in autoCompeteTextbox_post.MarkdownLinksInText)
                        {
                            AppNetDotNet.Model.Entities.Link linkEntity = new AppNetDotNet.Model.Entities.Link();
                            linkEntity.text = link.Value;
                            linkEntity.url = link.Key;
                            int startPosition = toBePostedText.IndexOf(string.Format("[{0}]({1})", linkEntity.text, linkEntity.url));
                            linkEntity.pos = startPosition;
                            linkEntity.len = linkEntity.text.Length;
                            toBePostedText = toBePostedText.Replace(string.Format("[{0}]({1})", linkEntity.text, linkEntity.url), linkEntity.text);
                            entities.links.Add(linkEntity);
                        }
                    }

                    List<AppNetDotNet.Model.File> toBeAddedFiles = null;
                    if (!string.IsNullOrEmpty(path_to_be_uploaded_image))
                    {
                        if (System.IO.File.Exists(path_to_be_uploaded_image))
                        {
                            Tuple<AppNetDotNet.Model.File, ApiCallResponse> uploadedFile = AppNetDotNet.ApiCalls.Files.create(account.accessToken, local_file_path: path_to_be_uploaded_image, type: "de.li-ghun.nymphicus.image");
                            if (uploadedFile.Item2.success)
                            {
                                toBeAddedFiles = new List<File>();
                                toBeAddedFiles.Add(uploadedFile.Item1);
                            }
                        }
                    }

                    Tuple<Post, ApiCallResponse> response;

                    if (inReplyToId == 0)
                    {
                        response = Posts.create(account.accessToken, toBePostedText, entities: entities, parse_links: true, toBeEmbeddedFiles: toBeAddedFiles);
                    }
                    else
                    {
                        response = Posts.create(account.accessToken, toBePostedText, inReplyToId.ToString(), entities: entities, parse_links: true, toBeEmbeddedFiles: toBeAddedFiles);
                    }

                    if (response.Item2.success)
                    {
                        Close();
                    }

                }
            }
        }

  
        public void isReplyToItem(ApnItem item)
        {
            if (item == null)
            {
                return;
            }
            autoCompeteTextbox_post.Text = "@" + item.apnItem.user.username + " ";
            inReplyToId = item.Id;
            labelInReplyTo.Content = "In reply to @" + item.AuthorName;
            labelInReplyTo.ToolTip = item.Text;
            buttonSendPost.Content = "Send reply";

            labelInReplyTo.Visibility = Visibility.Visible;
            List<string> alreadyShownUser = new List<string>();
            int positionOfCursor = autoCompeteTextbox_post.Text.Length;
            int lengthOfSelection = 0;
            if (item.accountId != 0)
            {
                try
                {
                    alreadyShownUser.Add("@" + item.receivingAccount.username);
                }
                catch { }
            }
            alreadyShownUser.Add("@" + item.apnItem.user.username.ToLower());

            comboBoxAccount.comboBoxAccounts.SelectedItem = item.receivingAccount;
           
            autoCompeteTextbox_post.textBoxContent.Select(positionOfCursor, lengthOfSelection);
            autoCompeteTextbox_post.textBoxContent.Focus();
        }

        private void buttonAddImage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(path_to_be_uploaded_image))
            {
                return;
            }
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "Images (*.png,*.jpg,*.gif,*.tif)|*.png;*.jpeg;*.jpg;*.gif;*.tif;*.tiff"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                if (System.IO.File.Exists(filename))
                {
                    path_to_be_uploaded_image = filename;
                    imageAddImage.Opacity = 1.0;
                    autoCompeteTextbox_post.textBoxContent.Text = autoCompeteTextbox_post.textBoxContent.Text.Insert(autoCompeteTextbox_post.textBoxContent.CaretIndex, "[" + System.IO.Path.GetFileNameWithoutExtension(path_to_be_uploaded_image) + "](photos.app.net/{post_id}/1)");
                    buttonAddImage.ToolTip = "Right click to remove the image";
                }
            }
        }

        class UploadArguments
        {
            public string filepath;
            public AccountAppDotNet account;
        }

        private void buttonShortenUrls_Click(object sender, RoutedEventArgs e)
        {
            autoCompeteTextbox_post.Text = AppController.Current.ActualLinkShortener.ShortenAllLinksInText(autoCompeteTextbox_post.Text);
        }

        private void textBoxContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == (Key.Return & Key.LeftCtrl) || e.Key == (Key.Enter & Key.LeftCtrl) || e.Key == (Key.Return & Key.RightCtrl) || e.Key == (Key.Enter & Key.RightCtrl))
            {
                if (buttonSendPost.IsEnabled)
                {
                    buttonSendPost_Click(null, null);
                }
            }
        }

        void textBoxContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (buttonSendPost == null)
            {
                return;
            }
            buttonSendPost.IsEnabled = true;
            int remaining_chars = 255;

            if (labelTextLength != null)
            {
                remaining_chars -= autoCompeteTextbox_post.NumberOfChars;
                labelTextLength.Content = remaining_chars.ToString();
            }
            if (remaining_chars < 0)
            {
                labelTextLength.Foreground = Brushes.Red;
                buttonSendPost.IsEnabled = false;
            }
            else if (remaining_chars < 3)
            {
                labelTextLength.Foreground = Brushes.Orange;
            }
            else
            {
                labelTextLength.Foreground = Brushes.Black;
            }
        }

        public void isPrivateMessage(string receiver)
        {
            labelInReplyTo.Content = "Private message to @" + receiver + " ";
            privateMessageReceiver = receiver;
            buttonSendPost.Content = "Send PM";
            labelInReplyTo.Visibility = Visibility.Visible;
            autoCompeteTextbox_post.textBoxContent.Focus();
        }

        public void selectAccount(AccountTwitter account)
        {
            if (account != null)
            {
                comboBoxAccount.comboBoxAccounts.SelectedItem = account;
            }
        }

        public void setToBeEditedItem(ApnItem item)
        {
            toBeEditedItem = item;
            if (item != null)
            {
                autoCompeteTextbox_post.textBoxContent.Text = item.Text;
                labelInReplyTo.Content = "Editing post - old version will be deleted";
                labelInReplyTo.Visibility = Visibility.Visible;
            }
        }

        private void buttonShortenUrls_Click_1(object sender, RoutedEventArgs e)
        {
            UserInterface.Apn.AddMarkdownLink add_link_window = new Apn.AddMarkdownLink();
            add_link_window.Left = this.Left;
            add_link_window.Top = this.Top;
            add_link_window.Width = this.Width;
            add_link_window.Height = this.Height;
            add_link_window.InsertLink += add_link_window_InsertLink;
            add_link_window.Show();
        }

        void add_link_window_InsertLink(object sender, UserInterface.Apn.AddMarkdownLink.InsertLinkEventArgs e)
        {
            if (e != null)
            {
                if (e.success)
                {
                    autoCompeteTextbox_post.textBoxContent.Text = autoCompeteTextbox_post.textBoxContent.Text.Insert(autoCompeteTextbox_post.textBoxContent.CaretIndex, e.insert_string);
                }
            }
        }

        private void buttonAddImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            imageAddImage.Opacity = 0.7;
            if (autoCompeteTextbox_post.textBoxContent.Text.Contains("[" + System.IO.Path.GetFileNameWithoutExtension(path_to_be_uploaded_image) + "](photos.app.net/{post_id}/1)"))
            {
                autoCompeteTextbox_post.textBoxContent.Text = autoCompeteTextbox_post.textBoxContent.Text.Replace("[" + System.IO.Path.GetFileNameWithoutExtension(path_to_be_uploaded_image) + "](photos.app.net/{post_id}/1)", "");
            }
            path_to_be_uploaded_image = "";
            buttonAddImage.ToolTip = "Upload an image";
        }
    }
}
