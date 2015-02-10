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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace Nymphicus.UserInterface.Facebook
{
    /// <summary>
    /// Interaction logic for ShowUser.xaml
    /// </summary>
    public partial class ShowUser : Window
    {
        #region Aero Glass
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [StructLayout(LayoutKind.Sequential)]
        public class MARGINS
        {
            public int cxLeftWidth, cxRightWidth,
                cyTopHeight, cyBottomHeight;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
            {
                // Get the current window handle
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Colors.Transparent;

                this.Background = Brushes.Transparent;

                // Set the proper margins for the extended glass part
                MARGINS margins = new MARGINS();
                margins.cxLeftWidth = -1;
                margins.cxRightWidth = -1;
                margins.cyTopHeight = -1;
                margins.cyBottomHeight = -1;

                int result = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);

                if (result < 0)
                {
                    MessageBox.Show("An error occured while extending the glass unit.");
                }

            }
        }

        #endregion

        public FacebookUser MyPerson { get; set; }
        public ObservableCollection<IItem> Items { get; set; }

        private BackgroundWorker backgroundWorkerRecentStatuses;

        public void UpdateRecentStatuses()
        {
            Items.Clear();

            if (!backgroundWorkerRecentStatuses.IsBusy)
            {
                backgroundWorkerRecentStatuses.RunWorkerAsync();
            }
            else
            {
                backgroundWorkerRecentStatuses.CancelAsync();
            }
        }


        public ShowUser()
        {
            InitializeComponent();


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

        public void setUser(FacebookUser user)
        {

            MyPerson = user;
            userInfoBox.DataContext = MyPerson;
            Items = new ObservableCollection<IItem>();
            listBoxItems.listView_Tweets.ItemsSource = Items;
            backgroundWorkerRecentStatuses = new BackgroundWorker();
            backgroundWorkerRecentStatuses.WorkerReportsProgress = true;
            backgroundWorkerRecentStatuses.DoWork +=new DoWorkEventHandler(backgroundWorkerRecentStatuses_DoWork);
            backgroundWorkerRecentStatuses.ProgressChanged +=new ProgressChangedEventHandler(backgroundWorkerRecentStatuses_ProgressChanged);
            

            if (user != null)
            {
                if (user.FullName != null)
                {
                    buttonPostToWall.Content = "Post to " + user.FullName + "'s wall"; 
                }
                UpdateRecentStatuses();
          
                this.Title = user.FullName;

            }
        }

        void image_DownloadCompleted(object sender, EventArgs e)
        {
            try
            {
                BitmapImage bitmapImage = (BitmapImage)sender;
                bitmapImage.Freeze();
                bitmapImage.DecodePixelHeight = 32;
                
                if (bitmapImage != null)
                {
                    
                    this.Icon = bitmapImage;
                }
            }
            catch
            {
                // Dann halt nicht...
            }
        }

        void backgroundWorkerRecentStatuses_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FacebookItem item = (FacebookItem)e.UserState;
            if (item != null)
            {
                if (Items.Where(i => i.Id == item.Id).Count() == 0)
                {
                    Items.Add(item);
                }
            }
        }

        private void backgroundWorkerRecentStatuses_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this == null)
            {
                AppController.Current.Logger.writeToLogfile("Null this at recent status messages");
                return;
            }

            ObservableCollection<FacebookItem> items = new ObservableCollection<FacebookItem>();
            try
            {

                      if (MyPerson.ReceivingAccount != null)
                {
                    dynamic parameters = new System.Dynamic.ExpandoObject();
                    dynamic result = MyPerson.ReceivingAccount.facebookClient.Get(MyPerson.Id + "/feed", parameters); // User feed

                    if (result != null)
                    {
                        foreach (dynamic post in result.data)
                        {
                            FacebookItem item = FacebookItem.ConvertResponseToItem(post, MyPerson.ReceivingAccount);
                             backgroundWorkerRecentStatuses.ReportProgress(100, item);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                AppController.Current.Logger.writeToLogfile("Retrieving recent tweets failed for " + MyPerson.FullName);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void buttonPostToWall_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxPostToWall.Text))
            {
                MyPerson.ReceivingAccount.PostTextStatusToUserWall(textBoxPostToWall.Text, MyPerson);
                UpdateRecentStatuses();
            }
        }

        private void textBoxPostToWall_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBoxPostToWall.Text.Length > 0)
            {
                buttonPostToWall.IsEnabled = true;
            }
            else
            {
                buttonPostToWall.IsEnabled = false;
            }
        }
    }
}
