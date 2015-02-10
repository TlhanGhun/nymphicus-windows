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
    /// Interaction logic for ReenableAccounts.xaml
    /// </summary>
    public partial class ReenableAccounts : Window
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

        public ReenableAccounts()
        {
            InitializeComponent();

            listBoxNotWorkingAccounts.ItemsSource = AppController.Current.NotWorkingAccounts;
        }

  
        private void buttonRetry_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                AccountTwitter account  = button.CommandParameter as AccountTwitter;
                if (account != null)
                {
                    AccountTwitter retryAccount = account;
                    if(retryAccount.verifyCredentials()) {
                        retryAccount.registerAccount();
                        AppController.Current.NotWorkingAccounts.Remove(account);
                        AppController.Current.AllAccounts.Add(retryAccount);
                        if (AppController.Current.NotWorkingAccounts.Count == 0)
                        {
                            AppController.Current.loadStoredViews();
                            retryViews();
                            AppController.Current.openMainWindow();
                            AppController.Current.updateAllAccounts();
                            Close();
                        }
                    }
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
            AccountTwitter account = button.CommandParameter as AccountTwitter;
            if (account != null)
            {
                    AppController.Current.NotWorkingAccounts.Remove(account);
                    if (AppController.Current.NotWorkingAccounts.Count == 0)
                    {
                        if (AppController.Current.AllAccounts.Count > 0)
                        {
                            AppController.Current.loadStoredViews();
                            retryViews();
                            AppController.Current.openMainWindow();
                            AppController.Current.updateAllAccounts();
                        }
                        else
                        {
                            AppController.Current.openPreferences();
                        }
                        Close();
                    }
                }
            }
        }

        private void retryViews()
        {
            List<string> reenabledViews = new List<string>();
            foreach (string viewString in AppController.Current.NotWorkingViews)
            {
                View view = new View();
                view.readStorableSettings(viewString);
                if (view.Name != "ERROR")
                {
                    reenabledViews.Add(viewString);
                    
                    AppController.Current.AllViews.Add(view);
                }
            }

            foreach (string viewString in reenabledViews)
            {
                AppController.Current.NotWorkingViews.Remove(viewString);
            }
        }

    }
}
