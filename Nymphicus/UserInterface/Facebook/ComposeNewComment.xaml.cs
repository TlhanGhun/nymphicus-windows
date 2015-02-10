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

namespace Nymphicus.UserInterface.Facebook
{
    /// <summary>
    /// Interaction logic for ComposeNewTweet.xaml
    /// </summary>
    public partial class ComposeNewComment : Window
    {
        private FacebookItem item;

        #region AeroGlass stuff
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

        public ComposeNewComment(FacebookItem fbItem)
        {
            InitializeComponent();

            item = fbItem;
            if (fbItem != null)
            {
                comboBoxAccount.comboBoxAccounts.SelectedItem = fbItem.Account;   
            }
            try
            {
                textblockCommentedText.Text = "Comment on: " + fbItem.Text.Substring(0, Math.Min(200, fbItem.Text.Length)) + " ... by " + fbItem.AuthorName;
                if (AppController.Current.mainWindow.WindowState == System.Windows.WindowState.Normal)
                {
                    this.Top = AppController.Current.mainWindow.Top + 5;
                    this.Left = AppController.Current.mainWindow.Left + 5;
                }
            }
            catch
            {

            }
            textBoxComment.Focus();

        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonSendComment_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxComment.Text.Length > 0)
            {
                AccountFacebook account = comboBoxAccount.comboBoxAccounts.SelectedItem as AccountFacebook;
                if (account != null)
                {
                    account.CommentItem(item, textBoxComment.Text);
                }
                else
                {
                    item.Account.CommentItem(item, textBoxComment.Text);
                }
                Close();
            }
        }

        private void textBoxComment_TextChanged(object sender, TextChangedEventArgs e)
        {
          
            
        }

        private void textBoxComment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == (Key.Return & Key.LeftCtrl) || e.Key == (Key.Enter & Key.LeftCtrl))
            {
                if (buttonSendComment.IsEnabled)
                {
                    buttonSendComment_Click(null, null);
                }
            }
        }


    }
}
