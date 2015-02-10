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
using System.Net;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for UpdateAvailable.xaml
    /// </summary>
    public partial class UpdateAvailable : Window
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

        public event updateCheckDoneEventHandler updateCheckDone;
        public delegate void updateCheckDoneEventHandler(object sender, updateCheckDoneEventArgs e);
        public class updateCheckDoneEventArgs : EventArgs
        {
            public bool tryNextTimeAgain
            {
                get;
                set;
            }
            public bool updateFound { get; set; }
            public bool closeApp { get; set; }
            public string toBeIgnoredVersion { get; set; }
            public string title { get; set; }
            public string text { get; set; }
        }

        updateCheckDoneEventArgs doneArgs;

        private string newVersion = "";

        public UpdateAvailable(string newVersionString, string titleLable, string text) {
            InitializeComponent();

            doneArgs = new updateCheckDoneEventArgs();
            doneArgs.tryNextTimeAgain = true;
            doneArgs.updateFound = false;
            doneArgs.closeApp = false;

            label_oldNew.Content = titleLable;
            textBox_news.Text = text;
            newVersion = newVersionString;

            Show();
        }

        public static updateCheckDoneEventArgs checkNow() {            
            Version installedVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            updateCheckDoneEventArgs doneArgs = new updateCheckDoneEventArgs();

            doneArgs = new updateCheckDoneEventArgs();
            doneArgs.tryNextTimeAgain = true;
            doneArgs.updateFound = false;
            doneArgs.closeApp = false;

            XmlDocument XMLdoc = null;
            HttpWebRequest request;
            HttpWebResponse response = null;
            try
            {
                if (!AppController.Current.isBetaVersion)
                {
                    request = (HttpWebRequest)WebRequest.Create(string.Format("http://www.li-ghun.de/Downloads/nymphicusUpdateInfo.xml"));
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(string.Format("http://www.li-ghun.de/Downloads/nymphicusBetaUpdateInfo.xml"));
                }
                request.UserAgent = @"Nymphicus update check " + installedVersion.ToString();
                response = (HttpWebResponse)request.GetResponse(); 
                XMLdoc = new XmlDocument();
                XMLdoc.Load(response.GetResponseStream()); 

                string availableVersionString = XMLdoc.SelectSingleNode("Nymphicus/Version").InnerText;

                if (availableVersionString == Properties.Settings.Default.IgnoredNewVersion)
                {
                    // this version shall be ignored
                    return doneArgs;
                }
                Version availableVersion = new Version(availableVersionString);
                if (availableVersion > installedVersion)
                {
                    doneArgs.title = string.Format("Installed version: {0}  - now available: {1}", Converter.prettyVersion.getNiceVersionString(installedVersion.ToString()), Converter.prettyVersion.getNiceVersionString(availableVersionString));
                    doneArgs.text = XMLdoc.SelectSingleNode("Nymphicus/Description").InnerText;
                    doneArgs.updateFound = true;
                    doneArgs.toBeIgnoredVersion = availableVersionString;
                }
                else
                {

                    doneArgs.updateFound = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                doneArgs.updateFound = false;
            }
            return doneArgs;
        }

        private void button_getUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!AppController.Current.isBetaVersion)
                {
                    System.Diagnostics.Process.Start("http://www.li-ghun.de/Nymphicus/Download/");
                }
                else
                {
                    System.Diagnostics.Process.Start("http://www.nymphicusapp.com/windows/nymphicuswindows20beta/");
                }
                doneArgs.closeApp = true;
                doneArgs.updateFound = true;
                updateCheckDone(this, doneArgs);
                Close();
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        private void button_ignore_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBox_remindMeAgain.IsChecked)
            {
                doneArgs.tryNextTimeAgain = false;
                doneArgs.toBeIgnoredVersion = newVersion;
                updateCheckDone(this, doneArgs);
            }
            Close();
        }
    }
}
