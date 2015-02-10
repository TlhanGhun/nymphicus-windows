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
using System.Collections.ObjectModel;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextboxForTwitterUsernames.xaml
    /// </summary>
    public partial class AutoCompleteTextboxForTwitterUsernames : UserControl
    {
        public IEnumerable<string> Usernames { get; set; }
        public IEnumerable<string> Hashtags { get; set; }
        private ObservableCollection<string> MatchingTexts { get; set; }
        private bool UsernameStarted { get; set; }
        private bool WordStart { get; set; }
        private int StartPosition { get; set; }
        private int UsernameLenght { get; set; }
        private string CurrentText { get; set; }
        private bool HashtagStarted { get; set; }
        private int HashtagLenght { get; set; }
        private bool ReplacingNow { get; set; }
        private bool ListboxClicked { get; set; }

        public string Text
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

        public AutoCompleteTextboxForTwitterUsernames()
        {
            InitializeComponent();


            WordStart = true;

            MatchingTexts = new ObservableCollection<string>();

            listBoxMatchingUsernames.ItemsSource = MatchingTexts;
            ListCollectionView _matchingUsernamesView = CollectionViewSource.GetDefaultView(MatchingTexts) as ListCollectionView;
            _matchingUsernamesView.CustomSort = new UsernamesSorter();
            ListCollectionView _MatchingUsernamessView = CollectionViewSource.GetDefaultView(MatchingTexts) as ListCollectionView;
            _MatchingUsernamessView.CustomSort = new UsernamesSorter();

 

        }

        public class UsernamesSorter : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Escape:
                    popupMatchingUsernames.IsOpen = false;
                    break;

                case Key.Down:
                    if (popupMatchingUsernames.IsOpen && listBoxMatchingUsernames.Items.Count > 0)
                    {
                        listBoxMatchingUsernames.Focus();
                        listBoxMatchingUsernames.SelectedIndex = 0;
                    }
                    break;
            }
            
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ReplacingNow)
            {
                return;
            }

            if (!WordStart)
            {
                foreach (TextChange change in e.Changes)
                {
                    if (change.AddedLength > 0)
                    {
                        switch (textBoxContent.Text.Substring(change.Offset, 1))
                        {
                            case " ":
                            case ",":
                            case ".":
                            case ":":
                            case ";":
                            case "(":
                            case ")":
                            case "!":
                            case "?":
                            case "\"":
                                WordStart = true;
                                UsernameStarted = false;
                                HashtagStarted = false;
                                break;

                            default:
                                WordStart = false;
                                break;
                        }
                        if (WordStart) { break; }
                    }
                }
            }
            if (UsernameStarted || HashtagStarted)
            {
                foreach (TextChange change in e.Changes)
                {
                    if (change.AddedLength > 0)
                    {
                        switch (textBoxContent.Text.Substring(change.Offset, 1))
                        {
                            case " ":
                            case ",":
                            case ".":
                            case ":":
                            case ";":
                            case "(":
                            case ")":
                            case "!":
                            case "?":
                            case "\"":
                                WordStart = true;
                                UsernameStarted = false;
                                HashtagStarted = false;
                                break;

                            default:
                                WordStart = false;
                                break;
                        }
                        if (WordStart) { break; }
                    }
                }
            }

        
            
            if (WordStart && (!UsernameStarted && !HashtagStarted))
            {
                foreach (TextChange change in e.Changes)
                {
                    if (change.AddedLength > 0)
                    {
                        if (textBoxContent.Text.Substring(change.Offset, 1) == " ")
                        {
                            WordStart = true;
                            continue;
                        }
                        else
                        {
                            WordStart = false;
                        }
                        if (textBoxContent.Text.Substring(change.Offset, 1) == "@")
                        {
                            UsernameStarted = true;
                            StartPosition = change.Offset;
                            CurrentText = "";
                        }
                        else if (textBoxContent.Text.Substring(change.Offset, 1) == "#")
                        {
                            HashtagStarted = true;
                            StartPosition = change.Offset;
                            CurrentText = "";
                        }
                    }
                }
            }

            if (UsernameStarted)
            {
                foreach (TextChange change in e.Changes)
                {
                    if(change.AddedLength > 0) {
                        CurrentText += textBoxContent.Text.Substring(change.Offset, change.AddedLength);
                    }
                    else if (change.RemovedLength > 0)
                    {
                        if (change.RemovedLength >= CurrentText.Length)
                        {
                            UsernameStarted = false;
                            CurrentText = "";
                            MatchingTexts.Clear();
                            popupMatchingUsernames.IsOpen = false;
                            return;

                        }
                        else if (change.RemovedLength == CurrentText.Length) {
                            WordStart = true;
                            CurrentText = "";
                            popupMatchingUsernames.IsOpen = false;
                            MatchingTexts.Clear();
                        }
                        else
                        {
                            CurrentText = CurrentText.Substring(0, CurrentText.Length - change.RemovedLength);
                        }
                    }
                }
                if (Usernames.Where(name => name.ToLower().StartsWith(CurrentText.ToLower())).Count() > 0)
                {
                    MatchingTexts.Clear();
                    foreach (string username in Usernames.Where(name => name.ToLower().StartsWith(CurrentText.ToLower())))
                    {
                        MatchingTexts.Add(username);
                        if (MatchingTexts.Count >= Properties.Settings.Default.MaxNumberOfEntriesInAutocomplete) { break; }
                    }
                    popupMatchingUsernames.PlacementRectangle = textBoxContent.GetRectFromCharacterIndex(textBoxContent.CaretIndex, true);
                    popupMatchingUsernames.IsOpen = true;
                }
                else
                {
                    MatchingTexts.Clear();
                    popupMatchingUsernames.IsOpen = false;
                }
            }
            else if (HashtagStarted)
            {
                foreach (TextChange change in e.Changes)
                {
                    if (change.AddedLength > 0)
                    {
                        CurrentText += textBoxContent.Text.Substring(change.Offset, change.AddedLength);
                    }
                    else if (change.RemovedLength > 0)
                    {
                        if (change.RemovedLength >= CurrentText.Length)
                        {
                            HashtagStarted = false;
                            CurrentText = "";
                            MatchingTexts.Clear();
                            popupMatchingUsernames.IsOpen = false;
                            return;

                        }
                        else if (change.RemovedLength == CurrentText.Length)
                        {
                            WordStart = true;
                            CurrentText = "";
                            popupMatchingUsernames.IsOpen = false;
                            MatchingTexts.Clear();
                        }
                        else
                        {
                            CurrentText = CurrentText.Substring(0, CurrentText.Length - change.RemovedLength);
                        }
                    }
                }
                if (Hashtags.Where(tag => tag.ToLower().StartsWith(CurrentText.ToLower())).Count() > 0)
                {
                    MatchingTexts.Clear();
                    foreach (string hashtag in Hashtags.Where(tag => tag.ToLower().StartsWith(CurrentText.ToLower())))
                    {
                        MatchingTexts.Add(hashtag);
                        if (MatchingTexts.Count >= Properties.Settings.Default.MaxNumberOfEntriesInAutocomplete) { break; }
                    }
                    popupMatchingUsernames.PlacementRectangle = textBoxContent.GetRectFromCharacterIndex(textBoxContent.CaretIndex, true);
                    popupMatchingUsernames.IsOpen = true;
                }
                else
                {
                    MatchingTexts.Clear();
                    popupMatchingUsernames.IsOpen = false;
                }
            }
        }


         private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListboxClicked)
            {
                ListBox listbox = sender as ListBox;
                if (listbox != null)
                {
                    ExecuteSelectedItem(listbox);
                }
            }
            ListboxClicked = false;
        }

         private void listBox1_MouseDown(object sender, MouseButtonEventArgs e)
         {
             ListboxClicked = true;
         }

         private void listBox1_PreviewKeyDown(object sender, KeyEventArgs e)
         {
             if (e.Key == Key.Escape)
             {
                 popupMatchingUsernames.IsOpen = false;
                 MatchingTexts.Clear();
                 e.Handled = true;
                 textBoxContent.Focus();
             }
             if (e.Key == Key.Return || e.Key == Key.Enter)
             {
                 ListBox listbox = sender as ListBox;
                 if (listbox != null)
                 {
                     ExecuteSelectedItem(listbox);
                     e.Handled = true;
                 }
             }
         }

        private void ExecuteSelectedItem(ListBox listbox) {
            if(listbox == null) {return;}
            string selected = listbox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selected))
            {
                ReplacingNow = true;
                textBoxContent.Text = textBoxContent.Text.Remove(StartPosition, CurrentText.Length);
                textBoxContent.Text = textBoxContent.Text.Insert(StartPosition, selected + " ");
                UsernameStarted = false;
                HashtagStarted = false;
                WordStart = true;
                MatchingTexts.Clear();
                popupMatchingUsernames.IsOpen = false;
                textBoxContent.Focus();
                textBoxContent.SelectionStart = StartPosition + selected.Length + 1;
                ListboxClicked = false;
                ReplacingNow = false;
            }
        }

        private void listBox1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListboxClicked = false;
        }

    }
}
