using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Nymphicus.Model
{
    /// <summary>
    /// Defines a filter for items based on author and/or text
    /// </summary>
    public class Filter : INotifyPropertyChanged
    {
        public decimal Id { get; private set; }
        
        /// <summary>
        /// Unique name of the filter
        /// </summary>
        /// 
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
                NotifyPropertyChanged("Description");
            }
        }
        private string _name
        {
            get;
            set;
        }

        /// <summary>
        /// The filter itself - implemented as a (!)contains on ToLower based strings
        /// </summary>
        public string FilterString
        {
            get
            {
                return _filterString;
            }
            set
            {
                _filterString = value;
                NotifyPropertyChanged("FilterString");
                NotifyPropertyChanged("Description");
            }
        }
        private string _filterString {get;set;}

        /// <summary>
        /// Shall the Filter words be be used as "Contains Not" - blacklisting (Default)
        /// </summary>
        public Boolean IsRegularExpression
        {
            get
            {
                return _isRegularExpression;
            }
            set
            {
                _isRegularExpression = value;
                NotifyPropertyChanged("IsRegularExpression");
                NotifyPropertyChanged("Description");
            }
        }
        private Boolean _isRegularExpression { get; set; }

        /// <summary>
        /// Shall the filter be used as "Contains" - whitelisting
        /// </summary>
        public Boolean IsExcludeFilter
        {
            get
            {
                return _isExcludeFilter;
            }
            set
            {
                _isExcludeFilter = value;
                NotifyPropertyChanged("IsExcludeFilter");
                NotifyPropertyChanged("Description");
            }
        }
        private Boolean _isExcludeFilter { get; set; }

        /// <summary>
        /// Shall the filter be used as "Contains" - whitelisting
        /// </summary>
        public Boolean IsIncludeFilter
        {
            get
            {
                return !IsExcludeFilter;
            }
        }

        public Boolean IsStopFilter
        {
            get
            {
                return _isStopFilter;
            }
            set
            {
                _isStopFilter = value;
                NotifyPropertyChanged("IsStopFilter");
                NotifyPropertyChanged("Description");
            }
        }
        private Boolean _isStopFilter { get; set; }


        /// <summary>
        /// Shall the author login name be filtered (not fullname!)
        /// </summary>
        public bool FilterAuthor
        {
            get
            {
                return _filterAuthor;
            }
            set
            {
                _filterAuthor = value;
                NotifyPropertyChanged("FilterAuthor");
                NotifyPropertyChanged("Description");
            }
        }
        private bool _filterAuthor { get; set; }

        /// <summary>
        /// Shall the item text be filtered
        /// </summary>
        public bool FilterRetweeter
        {
            get
            {
                return _filterRetweeter;
            }
            set
            {
                _filterRetweeter = value;
                NotifyPropertyChanged("FilterRetweeter");
                NotifyPropertyChanged("Description");
            }
        }
        private bool _filterRetweeter { get; set; }

        /// <summary>
        /// Shall the item text be filtered
        /// </summary>
        public bool FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                NotifyPropertyChanged("FilterText");
                NotifyPropertyChanged("Description");
            }
        }
        private bool _filterText { get; set; }

        /// <summary>
        /// Shall the client name being filtered
        /// </summary>
        public bool FilterClient
        {
            get
            {
                return _filterClient;
            }
            set
            {
                _filterClient = value;
                NotifyPropertyChanged("FilterClient");
                NotifyPropertyChanged("Description");
            }
        }
        private bool _filterClient { get; set; }

        public string Description
        {
            get
            {
                string desc = "This ";
                bool addComma = false;
                if (IsRegularExpression)
                {
                    desc += "regexp ";
                }
                desc += "filter searches ";
                if (FilterAuthor)
                {
                    if (addComma)
                    {
                        desc += ", ";
                    }
                    addComma = true;
                    desc += "the author name";
                }
                if (FilterText)
                {
                    if (addComma)
                    {
                        desc += ", ";
                    }
                    addComma = true;
                    desc += "the text";
                }
                if (FilterClient)
                {
                    if (addComma)
                    {
                        desc += ", ";
                    }
                    addComma = true;
                    desc += "the client name";
                }
                if (FilterRetweeter)
                {
                    if (addComma)
                    {
                        desc += ", ";
                    }
                    addComma = true;
                    desc += "the reposter name";
                }
                desc += " for " + FilterString;

                return desc;
            }
        }

        public Filter()
        {
            Name = "No name";
            FilterString = "";
            IsExcludeFilter = true;
            FilterAuthor = true;
            FilterRetweeter = true;
            FilterText = true;
            FilterClient = false;
            IsStopFilter = true;
            if (AppController.Current.AllFilters.Count > 0)
            {
                Id = AppController.Current.AllFilters.Max(f => f.Id) + 1;
            }
            else
            {
                Id = 0;
            }
        }

        /// <summary>
        /// Will return true if this filter would like the item to be displayed
        /// </summary>
        /// <param name="item">The item which shall be checked for filtering</param>
        /// <returns></returns>
        public bool ShallItemBeDisplayed(IItem item)
        {
            if (item == null || string.IsNullOrEmpty(FilterString)) { return false; }
            #region Reposts and retweets detection
            bool isRepost = false;
            if (item.GetType() == typeof(TwitterItem))
            {
                TwitterItem tempItem = item as TwitterItem;
                isRepost = tempItem.isRetweeted;
            }
            else if (item.GetType() == typeof(ApnItem))
            {
                ApnItem tempItem = item as ApnItem;
                isRepost = tempItem.isReposted;
            }
            #endregion
            bool ItemShallBeDisplayed = true;
            if (IsExcludeFilter)
            {
                // was ein grausamer Code bzw. Kot
                if (FilterAuthor)
                {
                    ItemShallBeDisplayed = !checkIfStringIsInText(item.AuthorName);
                }
                if (!ItemShallBeDisplayed)
                {
                    return ItemShallBeDisplayed;
                }
                if (FilterRetweeter)
                {
                    ItemShallBeDisplayed = !isRepost;
                    if (isRepost)
                    {
                        ItemShallBeDisplayed = !checkIfStringIsInText(item.AuthorName);
                    }
                }
                if (!ItemShallBeDisplayed)
                {
                    return ItemShallBeDisplayed;
                }

                if (FilterText)
                {
                    ItemShallBeDisplayed = !checkIfStringIsInText(item.Text);
                }
                if (!ItemShallBeDisplayed)
                {
                    return ItemShallBeDisplayed;
                }

                if (FilterClient)
                {
                    ItemShallBeDisplayed = !checkIfStringIsInText(item.ClientName);
                }
            }
            else
            {
                ItemShallBeDisplayed = false;
                if (FilterAuthor)
                {
                    ItemShallBeDisplayed = checkIfStringIsInText(item.AuthorName);
                }
                if (ItemShallBeDisplayed)
                {
                    return ItemShallBeDisplayed;
                }

                if (FilterRetweeter)
                {
                    ItemShallBeDisplayed = !isRepost;
                    if (isRepost)
                    {
                        ItemShallBeDisplayed = checkIfStringIsInText(item.AuthorName);
                    }
                }
                if (ItemShallBeDisplayed)
                {
                    return ItemShallBeDisplayed;
                }

                if (FilterText)
                {
                    ItemShallBeDisplayed = checkIfStringIsInText(item.Text);
                }
                if (ItemShallBeDisplayed)
                {
                    return ItemShallBeDisplayed;
                }

                if (FilterClient)
                {
                    ItemShallBeDisplayed = checkIfStringIsInText(item.ClientName);
                }
            }
            return ItemShallBeDisplayed;
        }

        private bool checkIfStringIsInText(string text)
        {
            try
            {
                if (!IsRegularExpression)
                {
                    return text.ToLower().Contains(FilterString.ToLower());
                }
                else
                {
                    return Regex.Match(text, FilterString, RegexOptions.IgnoreCase).Success;
                }
            }
            catch
            {
                return false;
            }
        }

        public string getStorableSettings()
        {
            string delimiter = "|||";
            string storableString = this.Name;
            storableString += delimiter + this.FilterString;
            storableString += delimiter + this.Id.ToString();
            storableString += delimiter + this.FilterText.ToString();
            storableString += delimiter + this.FilterAuthor.ToString();
            storableString += delimiter + this.IsExcludeFilter.ToString();
            storableString += delimiter + this.IsStopFilter.ToString();
            storableString += delimiter + this.FilterClient.ToString();
            storableString += delimiter + this.FilterRetweeter.ToString();
            storableString += delimiter + this.IsRegularExpression.ToString();
            return storableString;
        }

        public void readStorableSettings(string storedSettingsString)
        {
            try
            {
                string[] delimiter = { "|||" };
                string[] storedViews = storedSettingsString.Split(delimiter, StringSplitOptions.None);
                if (storedViews.Length < 7)
                {
                    return;
                }
                Name = storedViews[0];
                FilterString = storedViews[1];
                Id = Convert.ToDecimal(storedViews[2]);
                FilterText = Boolean.Parse(storedViews[3]);
                FilterAuthor = Boolean.Parse(storedViews[4]);
                IsExcludeFilter = Boolean.Parse(storedViews[5]);
                IsStopFilter = Boolean.Parse(storedViews[6]);
                if (storedViews.Length > 7)
                {
                    FilterClient = Boolean.Parse(storedViews[7]);
                }
                if (storedViews.Length > 9)
                {
                    FilterRetweeter = Boolean.Parse(storedViews[8]);
                    IsRegularExpression = Boolean.Parse(storedViews[9]);
                }
            }
            catch
            {
                this.Name = "### ERROR ###";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
            {
                PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }

}
