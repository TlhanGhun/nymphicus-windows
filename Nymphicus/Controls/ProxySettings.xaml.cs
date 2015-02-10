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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nymphicus.API
{
    /// <summary>
    /// Interaction logic for ProxySettings.xaml
    /// </summary>
    public partial class ProxySettings : UserControl
    {
        public ProxySettings()
        {
            InitializeComponent();

            checkBoxEnableProxy.IsChecked = Properties.Settings.Default.ProxyEnabled;
            textBoxProxyServerHost.Text = Properties.Settings.Default.ProxyServer;
            textBoxProxyPort.Text = Properties.Settings.Default.ProxyPort.ToString();
            textBoxProxyUser.Text = Properties.Settings.Default.ProxyUser;
            passwordBoxProxy.Password = Properties.Settings.Default.ProxyPassword;
        }

        #region Proxy

        private void checkBoxEnableProxy_Checked(object sender, RoutedEventArgs e)
        {

            textBoxProxyPort.IsEnabled = true;
            textBoxProxyServerHost.IsEnabled = true;
            textBoxProxyUser.IsEnabled = true;
            passwordBoxProxy.IsEnabled = true;

            textBoxProxyServerHost.Focus();

            Properties.Settings.Default.ProxyEnabled = true;
            Properties.Settings.Default.Save();

            AppController.Current.ApplyProxySettings();
        }

        private void checkBoxEnableProxy_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxProxyPort.IsEnabled = false;
            textBoxProxyServerHost.IsEnabled = false;
            textBoxProxyUser.IsEnabled = false;
            passwordBoxProxy.IsEnabled = false;

            Properties.Settings.Default.ProxyEnabled = false;
            Properties.Settings.Default.Save();

            AppController.Current.ApplyProxySettings();
        }

        private void textBoxProxyServerHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            var thisTextBox = sender as System.Windows.Controls.TextBox;
            Properties.Settings.Default.ProxyServer = thisTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void textBoxProxyUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            var thisTextBox = sender as System.Windows.Controls.TextBox;
            Properties.Settings.Default.ProxyUser = thisTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void passwordBoxProxy_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var thisTextBox = sender as System.Windows.Controls.PasswordBox;
            Properties.Settings.Default.ProxyPassword = thisTextBox.Password;
            Properties.Settings.Default.Save();
        }

        private void textBoxProxyPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            var thisTextBox = sender as System.Windows.Controls.TextBox;
            if (thisTextBox.Text != "")
            {
                try
                {
                    Properties.Settings.Default.ProxyPort = Convert.ToInt32(thisTextBox.Text);
                    textBoxProxyPort.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    Properties.Settings.Default.Save();
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                    textBoxProxyPort.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 150, 150));
                }
            }
        }

        #endregion
    }
}
