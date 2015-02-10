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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Nymphicus.Model;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for NotLicensed.xaml
    /// </summary>
    public partial class NotLicensed : Window
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

        public NotLicensed()
        {
            InitializeComponent();
        }

        private void ignoreForNow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonGetLicense_Click(object sender, RoutedEventArgs e)
        {
            hyperlinkToPurchasePage_Click(null, null);
        }

        private void hyperlinkToPurchasePage_Click(object sender, RoutedEventArgs e)
        {
            string purchaseLink = "http://www.li-ghun.de/Nymphicus/Purchase/";
            if (AppController.Current.AllAccounts.Count > 0)
            {
                try
                {
                    if (AppController.Current.AllTwitterAccounts.Count > 0)
                    {
                        AccountTwitter account = AppController.Current.AllTwitterAccounts[0] as AccountTwitter;
                        if (account != null)
                        {
                            purchaseLink += "?username=" + account.Login.Username;
                        }
                    }
                    else if (AppController.Current.AllFacebookAccounts.Count > 0)
                    {
                        AccountFacebook account = AppController.Current.AllFacebookAccounts[0] as AccountFacebook;
                        if (account != null)
                        {
                            purchaseLink += "?username=" + account.Id.ToString();
                        }
                    }
                }
                catch
                {
                }
            }
            try
            {
                System.Diagnostics.Process.Start(purchaseLink);
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }
    }
}
