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
using Nymphicus;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for NewList.xaml
    /// </summary>
    public partial class NewList : Window
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

        AccountTwitter _account;
        public Person person { get; set; }

        public NewList(AccountTwitter account)
        {
            InitializeComponent();
            _account = account;
            comboBoxAccounts.Visibility = Visibility.Collapsed;
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

        public NewList()
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

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (_account == null && comboBoxAccounts.comboBoxAccounts.SelectedItem != null)
            {
                _account = comboBoxAccounts.comboBoxAccounts.SelectedItem as AccountTwitter;
                if (_account == null)
                {
                    MessageBox.Show("Unable to detect the account for this list", "List adding failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            TweetList list = API.Functions.createList(_account, textBoxName.Text, textBoxDescription.Text, checkBoxPublic.IsChecked.Value);
            if (list != null)
            {
                CreateSuccessEventArgs args = new CreateSuccessEventArgs();
                args.success = true;
                args.list = list;
                if (CreateSuccess != null)
                {
                    CreateSuccess(this, args);
                }
                if (person != null)
                {
                    API.Functions.addPersonToList(person, list, _account);
                }
                Close();
            }
        
        }
        public event CreateSuccessEventHandler CreateSuccess;
        public delegate void CreateSuccessEventHandler(object sender, CreateSuccessEventArgs e);
        public class CreateSuccessEventArgs : EventArgs
        {
            public bool success
            {
                get;
                set;
            }

            public TweetList list
            {
                get;
                set;
            }
        }

        
    }
}
