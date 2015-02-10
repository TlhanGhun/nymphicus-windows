using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace Nymphicus.UserInterface.Apn
{
    /// <summary>
    /// Interaction logic for AddMarkdownLink.xaml
    /// </summary>
    public partial class AddMarkdownLink : Window
    {
        
        public static RoutedCommand insert_link_command = new RoutedCommand();
        public static RoutedCommand cancel_command = new RoutedCommand();

        public AddMarkdownLink()
        {
            InitializeComponent();

            insert_link_command.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));
            insert_link_command.InputGestures.Add(new KeyGesture(Key.Return, ModifierKeys.Control));
            cancel_command.InputGestures.Add(new KeyGesture(Key.Escape, ModifierKeys.None));

            textbox_url.Focus();
            if (Clipboard.ContainsText())
            {
                if (Clipboard.GetText().ToLower().StartsWith("http://") || Clipboard.GetText().ToLower().StartsWith("https://"))
                {
                    textbox_url.Text = Clipboard.GetText();
                    try
                    {
                        string html_title = get_title_of_url(textbox_url.Text);
                        if (!string.IsNullOrEmpty(html_title))
                        {
                            textbox_title.Text = html_title;
                        }
                    }
                    catch { }
                    textbox_title.Focus();
                }
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_insert_Click(object sender, RoutedEventArgs e)
        {
            InsertLinkEventArgs insert_link_args = new InsertLinkEventArgs();
            insert_link_args.url = textbox_url.Text;
            insert_link_args.title = textbox_title.Text;
            InsertLink(this, insert_link_args);
            this.Close();
        }


        public event InsertLinkEventHandler InsertLink;
        public delegate void InsertLinkEventHandler(object sender, InsertLinkEventArgs e);
        public class InsertLinkEventArgs : EventArgs
        {
            public bool success
            {
                get
                {
                    return !string.IsNullOrEmpty(insert_string);
                }
            }
            public string url { get; set; }
            public string title { get; set; }
            public string insert_string
            {
                get
                {
                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(title))
                    {
                        return string.Format("[{0}]({1})", title, url);
                    }
                    else if (!string.IsNullOrEmpty(url))
                    {
                        return url;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Get title from an HTML string.
        /// </summary>
        static string get_title_of_url(string url)
        {
            string html_source = get_html_of_url(url);
            Match m = Regex.Match(html_source, @"<title>\s*(.+?)\s*</title>",RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Get the html source of the given URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string get_html_of_url(string url)
        {

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();

            return result;
        }


        #region keyboard shortcuts

        private void command_insert_link_executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_insert_Click(null, null);
        }

        private void command_cancel_executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
