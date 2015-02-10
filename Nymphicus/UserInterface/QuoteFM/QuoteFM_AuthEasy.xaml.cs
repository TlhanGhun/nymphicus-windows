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
using QuoteSharp;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Nymphicus.UserInterface.QuoteFM
{
    /// <summary>
    /// Interaction logic for QuoteFM_AuthEasy.xaml
    /// </summary>
    public partial class QuoteFM_AuthEasy : Window
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
        private User user { get; set; }
        private QuoteFmUser qfmUser { get; set; }

        public QuoteFM_AuthEasy()
        {
            InitializeComponent();
        }

        private void buttonCheckUsername_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textboxUsername.Text))
            {
                User user = QuoteSharp.API.getUser(textboxUsername.Text);
                if (user != null)
                {
                    buttonAddAccount.IsEnabled = true;
                    qfmUser = QuoteFmUser.createFromApi(user, getFollowings: true);
                    textblockUserInfo.Text = "Username check has been successfull. You now can add this account by pressing the button on the bottom right\n\n";
                    textblockUserInfo.Text += "Account data:\n\n";
                    textblockUserInfo.Text +=  qfmUser.DescriptiveText;
                    buttonAddAccount.IsEnabled = true;
                }
               
            }
        }

        private void buttonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            if (qfmUser != null)
            {
                AccountQuoteFM account = new AccountQuoteFM();
                account.User = qfmUser;
                account.UpdateItems();
                AppController.Current.AllAccounts.Add(account);
                
                this.Close();
            }
        }
    }
}
