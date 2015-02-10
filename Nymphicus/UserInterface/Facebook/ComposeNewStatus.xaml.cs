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
    public partial class ComposeNewStatus : Window
    {
        public static RoutedCommand Cancel = new RoutedCommand();
        public static RoutedCommand SelectTextField = new RoutedCommand();
        public static RoutedCommand SelectLinkField = new RoutedCommand();
        public static RoutedCommand SelectAccount = new RoutedCommand();
        public static RoutedCommand Send = new RoutedCommand();

        public bool IsValidLink { get; set; }

        AccountFacebook account;

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

        public ComposeNewStatus(AccountFacebook fbAccount)
        {
            InitializeComponent();

            Cancel.InputGestures.Add(new KeyGesture(Key.Escape));
            SelectTextField.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            SelectLinkField.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
            SelectAccount.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            Send.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));
            Send.InputGestures.Add(new KeyGesture(Key.Return, ModifierKeys.Control));

            
            account = fbAccount;
            if (account == null)
            {
                foreach (IAccount iaccount in AppController.Current.AllAccounts)
                {
                    if (iaccount.GetType() == typeof(AccountFacebook))
                    {
                        account = iaccount as AccountFacebook;
                        break;
                    }
                }
            }
            comboBoxAccount.comboBoxAccounts.SelectedItem = account;

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
            textBoxMessage.Focus();

        }

        #region keyboard shortcuts

        private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void SelectTextFieldExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            textBoxMessage.Focus();
        }

        private void SelectLinkFieldExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            textBoxLink.Focus();
        }

        private void SelectAccountExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            comboBoxAccount.comboBoxAccounts.Focus();
            comboBoxAccount.comboBoxAccounts.IsDropDownOpen = true;
        }

        private void SendExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            buttonSendStatus_Click(null, null);
        }

        #endregion

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonSendStatus_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxMessage.Text.Length > 0)
            {
                account = comboBoxAccount.comboBoxAccounts.SelectedItem as AccountFacebook;
                if (!IsValidLink)
                {
                    account.PostTextStatus(textBoxMessage.Text);
                }
                else
                {
                    account.PostLink(textBoxMessage.Text, textBoxLink.Text);
                }
                Close();
            }
        }

        private void textBoxMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
          
            
        }

        private void textBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == (Key.Return & Key.LeftCtrl) || e.Key == (Key.Enter & Key.LeftCtrl))
            {
                if (buttonSendStatus.IsEnabled)
                {
                    buttonSendStatus_Click(null, null);
                }
            }
        }

        private void textBoxLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxLink.Text.Length == 0)
            {
                IsValidLink = false;
                textBlockLink.Visibility = System.Windows.Visibility.Visible;
                textBoxLink.Background = Brushes.White;
            }
            else
            {
                textBlockLink.Visibility = System.Windows.Visibility.Collapsed;
                // not the perfect test but surprisingly only complex RegExp would fulfill all cases...
                if ((textBoxLink.Text.StartsWith("http://") || textBoxLink.Text.StartsWith("https://")) && textBoxLink.Text.Length > 7)
                {
                    IsValidLink = true;
                    textBoxLink.Background = Brushes.White;
                }
                else
                {
                    IsValidLink = false;
                    textBoxLink.Background = Brushes.Orange;   
                }
            }
        }


    }
}
