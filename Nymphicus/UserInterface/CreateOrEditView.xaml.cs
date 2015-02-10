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
using Nymphicus.UserInterface;
using Nymphicus.Controls;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for CreateOrEditView.xaml
    /// </summary>
    public partial class CreateOrEditView : Window
    {
        

        View view;

        public CreateOrEditView()
        {
            InitializeComponent();

            Title = "Create new view";
            view = new View();
            infoBoxViewName.useAsInputBox();
            infoBoxViewName.textBoxContent.KeyDown += new KeyEventHandler(textBoxContent_KeyDown);

            if (AppController.Current.HasTwitterAccounts && AppController.Current.AllAccounts.Count > AppController.Current.AllTwitterAccounts.Count)
            {
                UserInterface.AskForTypeOfView askViewType = new AskForTypeOfView(view);
                askViewType.Closing += askViewType_Closing;
                askViewType.WindowStyle = System.Windows.WindowStyle.ToolWindow;
                askViewType.Show();

                gridMain.IsEnabled = false;
                gridMain.Opacity = 0.4;
                return;

            }
            else if(AppController.Current.HasTwitterAccounts) {
                view.isTwitterOnlyView = true;
            }
            createInitialDisplay();
        }

        void askViewType_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            createInitialDisplay();
            gridMain.IsEnabled = true;
            gridMain.Opacity = 1.0;
        }

        private void createInitialDisplay()
        {

            if (!view.isTwitterOnlyView)
            {
                imageTwitterOnlyView.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!AppController.Current.HasFacebookAccounts || view.isTwitterOnlyView)
            {
                borderFacebook.Visibility = Visibility.Collapsed;
            }
            if (!AppController.Current.HasTwitterAccounts || !view.isTwitterOnlyView)
            {
                borderTwitter.Visibility = System.Windows.Visibility.Collapsed;
            }

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

            createViewTree();
        }


        void textBoxContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (infoBoxViewName.textBoxContent.LineCount > 0 && infoBoxViewName.textBoxContent.GetLineLength(0) > 0)
            {
                buttonSave.IsEnabled = true;
            }
            else
            {
                buttonSave.IsEnabled = false;
            }
        }

        private void createViewTree()
        {
            treeViewSelector = GeneralFunctions.treeViewForViews.CreateViewTreeView(view,treeViewSelector);
         
        }

        void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Filter filter = button.CommandParameter as Filter;
                if (filter != null)
                {
                    EditFilter editFilter = new EditFilter();
                    editFilter.LoadFilter(filter);
                    editFilter.Show();
                }
            }
        }

        void createNewFilter_Click(object sender, RoutedEventArgs e)
        {
            applyViewSettings();
            EditFilter newFilter = new EditFilter();
            newFilter.FilterCreated += new EditFilter.FilterCreatedEventHandler(newFilter_FilterCreated);
            newFilter.Show();
        }

        void newFilter_FilterCreated(object sender, EditFilter.FilterCreatedEventArgs e)
        {
            if (e != null)
            {
                if (e.newFilter != null)
                {
                    createViewTree();
                }
            }
        }

        private void applyViewSettings()
        {
            ItemCollection itemCollection = treeViewSelector.Items;

            GeneralFunctions.treeViewForViews.applyViewSettings(view, itemCollection);
        }

        private static decimal getIdOfCheckboxName(string checkboxName)
        {
            decimal id = 0;
            string[] split = checkboxName.Split('_');
            if (split.Length == 2)
            {
                try
                {
                    id = Convert.ToDecimal(split[1]);
                }
                catch
                {
                    id = 0;
                }
            }
            return id;
        }

        private static bool IsType(object obj, string type)
        {
            return (obj.GetType().ToString().ToLower().EndsWith(type.ToLower()));
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            applyViewSettings();
            view.Name = infoBoxViewName.textBoxContent.Text;
            AppController.Current.AllViews.Add(view);
            Close();
        }

        public void buttonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesInTreeview(treeViewSelector, true);
        }

        private void buttonInvertSlection_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.invertAllCheckboxesInTreeview(treeViewSelector);
        }

        private void buttonSelectAllTimelines_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "timeline", true);
        }

        private void buttonSelectAllMentions_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "mentions", true);
        }

        private void buttonSelectAllDMs_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "directMessages", true);
        }

        private void buttonSelectAllLists_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "list", true);
        }

        private void buttonSelectAllSearches_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "search", true);
        }

        private void buttonSelectAllRetweets_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "retweets", true);
        }

        private void buttonSelectAllStatusMessages_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "statusMessages", true);
        }

        private void buttonSelectAllLinks_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "links", true);
        }

        private void buttonSelectAllPhotos_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "photos", true);
        }

        private void buttonSelectAllVideos_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "videos", true);
        }

        private void buttonSelectAllIns_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "checkIns", true);
        }

        private void buttonSelectAllEvents_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "events", true);
        }

        private void buttonSelectAllNotes_Click(object sender, RoutedEventArgs e)
        {
            GeneralFunctions.treeViewForViews.setAllCheckboxesOfOneKindInTreeview(treeViewSelector, "notes", true);
        }

   
    }
}
