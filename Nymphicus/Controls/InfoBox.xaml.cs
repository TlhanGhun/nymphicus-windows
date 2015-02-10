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
using System.Text.RegularExpressions;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for InfoBox.xaml
    /// </summary>
    public partial class InfoBox : UserControl
    {
        public string InfoTitle
        {
            get { return (string)GetValue(TitleObjectProperty); }
            set { SetValue(TitleObjectProperty, value); }
        }

        public string InfoContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        public string InfoTextBox
        {
            get
            {
                return textBoxContent.Text;
            }
            set
            {
                textBoxContent.Text = value;
            }
        }

        public bool autoHideIfEmpty { get; set; }

        public static readonly DependencyProperty TitleObjectProperty =
        DependencyProperty.Register(
            "InfoTitle",
            typeof(string),
            typeof(InfoBox),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnTitleObjectChanged)));

        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register(
            "InfoContent",
            typeof(string),
            typeof(InfoBox),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnContentChanged)));

        private static void OnTitleObjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            string text = args.NewValue as string;
            if (text == null)
            {
                text = string.Empty;
            }
            InfoBox infobox = (InfoBox)obj;
            infobox.textBlockTitle.Text = text;
            if (infobox.autoHideIfEmpty)
            {
                if (string.IsNullOrEmpty(infobox.textBlockTitle.Text) && string.IsNullOrEmpty(infobox.textBlockContent.Text))
                {
                    infobox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    infobox.Visibility = Visibility.Visible;
                }
            }
        }

        private static void OnContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            string text = args.NewValue as string;
            if (text == null)
            {
                text = string.Empty;
            }
            InfoBox infobox = (InfoBox)obj;
            infobox.textBlockContent.Text = text;
            if (infobox.autoHideIfEmpty)
            {
                if (string.IsNullOrEmpty(infobox.textBlockTitle.Text) && string.IsNullOrEmpty(infobox.textBlockContent.Text))
                {
                    infobox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    infobox.Visibility = Visibility.Visible;
                }
            }

            infobox.textBlockContent.Inlines.Clear();
            string[] words = Regex.Split(text, @"([ \(\)\{\}\[\];])");
            foreach (string word in words)
            {

                if (word.ToLower().StartsWith("http://") || word.ToLower().StartsWith("https://"))
                {
                    Hyperlink link = new Hyperlink();
                    link.TextDecorations = null;
                    Uri target;
                    Uri.TryCreate(word, UriKind.Absolute, out target);
                    link.NavigateUri = target;
                    link.ToolTip = "Open " + word +  " in default browser";
                    link.Inlines.Add(word);
                    link.Click += new RoutedEventHandler(link_Click);
                    infobox.textBlockContent.Inlines.Add(link);
                }
                else
                {
                    infobox.textBlockContent.Inlines.Add(word);
                }
            }
        }

        static void link_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            try
            {
                System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri); ;
            }
            catch (Exception exp)
            {
                AppController.Current.sendNotification("Error", "Opening link failed", "While tryping to open the link Windows failed with following message: " + exp.Message, null, null);
                AppController.Current.Logger.writeToLogfile(exp);
            }
        }

        public InfoBox()
        {
            autoHideIfEmpty = true;
            InitializeComponent();

            textBoxContent.Visibility = Visibility.Hidden;
        }

        public void setContent(string title, string text, Uri linkTarget)
        {
            this.textBlockTitle.Text = title;
            this.textBlockContent.Text = text;
        }

        public void useAsInputBox()
        {
            textBlockContent.Visibility = Visibility.Collapsed;
            textBoxContent.Visibility = Visibility.Visible;
        }
    }
}
