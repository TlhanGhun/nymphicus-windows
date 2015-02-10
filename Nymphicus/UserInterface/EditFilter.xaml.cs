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
using Nymphicus.Controls;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for EditFilter.xaml
    /// </summary>
    public partial class EditFilter : Window
    {

        private Filter filter { get; set; }
        private bool isExistingFilter { get; set; }

        public event FilterCreatedEventHandler FilterCreated;
        public delegate void FilterCreatedEventHandler(object sender, FilterCreatedEventArgs e);
        public class FilterCreatedEventArgs : EventArgs
        {
            public Filter newFilter
            {
                get;
                set;
            }
        }

        public event FilterChangedEventHandler FilterChanged;
        public delegate void FilterChangedEventHandler(object sender, FilterChangedEventArgs e);
        public class FilterChangedEventArgs : EventArgs
        {
            public Filter changedFilter
            {
                get;
                set;
            }
        }

        public EditFilter()
        {
            InitializeComponent();

            filter = new Filter();
            infoBoxFilterName.useAsInputBox();
            infoBoxFilter.useAsInputBox();
            isExistingFilter = false;
        }

        public void LoadFilter(Filter loadedFilter)
        {
            isExistingFilter = true;
            filter = loadedFilter;
            infoBoxFilterName.textBoxContent.Text = loadedFilter.Name;
            infoBoxFilter.textBoxContent.Text = loadedFilter.FilterString;
            checkBoxFilterText.IsChecked = loadedFilter.FilterText;
            checkBoxFilterUsername.IsChecked = loadedFilter.FilterAuthor;
            checkBoxFilterClient.IsChecked = loadedFilter.FilterClient;
            checkBoxIsExcludeFilter.IsChecked = loadedFilter.IsExcludeFilter;
            checkBoxIsStopFilter.IsChecked = loadedFilter.IsStopFilter;
            checkBoxFilterRetweeter.IsChecked = loadedFilter.FilterRetweeter;
            checkBoxIsRegexp.IsChecked = loadedFilter.IsRegularExpression;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            filter.Name = infoBoxFilterName.textBoxContent.Text;
            filter.FilterString = infoBoxFilter.textBoxContent.Text;
            filter.FilterText = checkBoxFilterText.IsChecked.Value;
            filter.FilterAuthor = checkBoxFilterUsername.IsChecked.Value;
            filter.FilterClient = checkBoxFilterClient.IsChecked.Value;
            filter.IsExcludeFilter = checkBoxIsExcludeFilter.IsChecked.Value;
            filter.IsStopFilter = checkBoxIsStopFilter.IsChecked.Value;
            filter.FilterRetweeter = checkBoxFilterRetweeter.IsChecked.Value;
            filter.IsRegularExpression = checkBoxIsRegexp.IsChecked.Value;

            if (!isExistingFilter)
            {
                AppController.Current.AllFilters.Add(filter);
                FilterCreatedEventArgs eventArgs = new FilterCreatedEventArgs();
                eventArgs.newFilter = filter;
                if (FilterCreated != null)
                {
                    FilterCreated(this, eventArgs);
                }
            }
            else
            {
                FilterChangedEventArgs eventArgs = new FilterChangedEventArgs();
                eventArgs.changedFilter = filter;
                if (FilterChanged != null)
                {
                    FilterChanged(this, eventArgs);
                }
            }


            Close();
        }

        private void checkBoxIsExcludeFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxIsStopFilter != null)
            {
                checkBoxIsStopFilter.Visibility = Visibility.Visible;
            }
        }

        private void checkBoxIsExcludeFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkBoxIsStopFilter != null)
            {
                checkBoxIsStopFilter.Visibility = Visibility.Hidden;
            }
        }
    }
}
