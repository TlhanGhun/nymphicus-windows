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
    /// Interaction logic for SearchTextbox.xaml
    /// </summary>
    public partial class SearchTextbox : UserControl
    {
        public SearchTextbox()
        {
            InitializeComponent();
        }

        public event ExecuteSearchEventHandler ExecuteSearch;
        public delegate void ExecuteSearchEventHandler(object sender, EventArgs e);

        public event ExecuteSearchEnterEventHandler ExecuteSearchEnter;
        public delegate void ExecuteSearchEnterEventHandler(object sender, EventArgs e);

        public event SearchClearedEventHandler SearchCleared;
        public delegate void SearchClearedEventHandler(object sender, EventArgs e);


        private void ClearSearchfield_Click(object sender, RoutedEventArgs e)
        {
            textboxSearchString.Text = "";
            if (SearchCleared != null)
            {
                SearchCleared(this, new EventArgs());
            }
        }

        private void textboxSearchString_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ExecuteSearch != null)
            {
                ExecuteSearch(this, new EventArgs());
            }
            
        }

        private void textboxSearchString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (ExecuteSearchEnter != null)
                {
                    ExecuteSearchEnter(this, new EventArgs());
                }
            }
        }

    }
}
