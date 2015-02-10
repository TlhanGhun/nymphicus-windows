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

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for IStoreListItem.xaml
    /// </summary>
    public partial class IStoreListItem : UserControl
    {
        public IStoreListItem()
        {
            InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this_DataContextChanged);
        }

        void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ExternalServices.IStore service = e.NewValue as ExternalServices.IStore;
                if (service != null)
                {
                    passwordBoxLogin.Password = service.Password;
                }
            }
        }

        private void buttonVerifyCredentials_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ExternalServices.IStore service = button.CommandParameter as ExternalServices.IStore;
                if (service != null)
                {
                    if (service.VerifyCredentials())
                    {
                        button.Content = "Login OK";
                        button.IsEnabled = false;
                    }
                    else
                    {
                        button.Content = "Try again";
                    }
                }
            }
        }

        private void passwordBoxLogin_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                ExternalServices.IStore service = passwordBox.DataContext as ExternalServices.IStore;
                if (service != null)
                {
                    service.Password = passwordBox.Password;
                }
            }
        }
    }
}
