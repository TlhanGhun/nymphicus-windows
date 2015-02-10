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
using Nymphicus.Model;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for ComboboxAccounts.xaml
    /// </summary>
    public partial class ComboboxAccounts : UserControl
    {
        public AccountTwitter selectedAccount
        {
            get
            {
                try
                {
                    return comboBoxAccounts.SelectedItem as AccountTwitter;
                }
                catch
                {
                    return null;
                }
            }
        }

        private bool _showOnlyTwitter { get; set; }
        public bool ShowOnlyTwitter
        {
            get
            {

                    return _showOnlyTwitter;

            }
            set
            {
                comboBoxAccounts.ItemsSource = AppController.Current.AllTwitterAccounts;
                if (AppController.Current.AllTwitterAccounts.Count > 0)
                {
                    comboBoxAccounts.SelectedIndex = 0;
                }
                _showOnlyTwitter = value;
            }
        }

        private bool _showOnlyFacebook { get; set; }
        public bool ShowOnlyFacebook
        {
            get
            {

                    return _showOnlyFacebook;

            }
            set
            {
                comboBoxAccounts.ItemsSource = AppController.Current.AllFacebookAccounts;
                if (AppController.Current.AllFacebookAccounts.Count > 0)
                {
                    comboBoxAccounts.SelectedIndex = 0;
                }
                _showOnlyFacebook = value;
            }
        }

        private bool _showOnlyApn { get; set; }
        public bool ShowOnlyApn
        {
            get
            {

                return _showOnlyApn;

            }
            set
            {
                comboBoxAccounts.ItemsSource = AppController.Current.AllApnAccounts;
                if (AppController.Current.AllApnAccounts.Count > 0)
                {
                    comboBoxAccounts.SelectedIndex = 0;
                }
                _showOnlyApn = value;
            }
        }

        public ComboboxAccounts()
        {
            InitializeComponent();

            _showOnlyFacebook = false;
            _showOnlyTwitter = false;

    
            

            if(AppController.Current != null) {
                if (ShowOnlyTwitter)
                {
                    comboBoxAccounts.ItemsSource = AppController.Current.AllTwitterAccounts;
                    if (AppController.Current.AllTwitterAccounts.Count > 0)
                    {
                        comboBoxAccounts.SelectedIndex = 0;
                    }
                }
                else if (ShowOnlyFacebook)
                {
                    comboBoxAccounts.ItemsSource = AppController.Current.AllFacebookAccounts;
                    if (AppController.Current.AllFacebookAccounts.Count > 0)
                    {
                        comboBoxAccounts.SelectedIndex = 0;
                    }
                }
                else
                {
                    comboBoxAccounts.ItemsSource = AppController.Current.AllAccounts;
                    if (AppController.Current.AllAccounts.Count > 0)
                    {
                        comboBoxAccounts.SelectedIndex = 0;
                    }
                }
            }
        }
    }
}
