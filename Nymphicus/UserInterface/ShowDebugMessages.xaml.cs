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
using System.IO;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for ShowDebugMessages.xaml
    /// </summary>
    public partial class ShowDebugMessages : Window
    {
        public ShowDebugMessages()
        {
            InitializeComponent();
            listViewMessages.ItemsSource = AppController.Current.Logger.DebugMessages;

            foreach (var debugType in Enum.GetValues(typeof(Nymphicus.DebugMessage.DebugMessageTypes)).Cast<DebugMessage.DebugMessageTypes>())
            {
                comboBoxFilters.Items.Add(debugType.ToString());
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            string debugText = "Debug messages text version\n\n";
            foreach (Nymphicus.DebugMessage message in AppController.Current.Logger.DebugMessages)
            {
                debugText += message.ToString().Replace("\n", " -LINEBREAK- ").Replace("\r", "") + "\n";
            }
            try
            {
                Clipboard.SetText(debugText);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Copying to clipboard failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonSaveToLog_Click(object sender, RoutedEventArgs e)
        {
            string debugText = "Debug messages text version\n\n";
            foreach (Nymphicus.DebugMessage message in AppController.Current.Logger.DebugMessages)
            {
                debugText += message.ToString().Replace("\n","LINEBREAK").Replace("\r","") + "\n";
            }
            try
            {
                if (System.IO.Directory.Exists(AppController.Current.appDataPath))
                {
                    File.WriteAllText(AppController.Current.appDataPath + "\\debugMessages.log", debugText);
                }
                else
                {
                    MessageBox.Show("Directory not available", "Saving failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Saving failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonOpenLogFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(AppController.Current.appDataPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", AppController.Current.appDataPath);
            }
        }

        private void comboBoxFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxFilters.SelectedItem != null)
            {
                listViewMessages.Items.Filter = delegate(object obj)
                {
                    DebugMessage message = obj as DebugMessage;
                    if (message == null)
                    {
                        return false;
                    }
                    if (message.type.ToString() == comboBoxFilters.SelectedItem.ToString())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                };
            }
            else
            {
                listViewMessages.Items.Filter = null;
            }
        }

        private void buttonResetFilter_Click(object sender, RoutedEventArgs e)
        {
            comboBoxFilters.SelectedItem = null;
            listViewMessages.Items.Filter = null;
        }
    }
}
