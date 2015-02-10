using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Nymphicus.Model
{
    public class SubscribableItemsCollection
    {
        public string DisplayTitle { get; set; }
        public ThreadSaveObservableCollection<IItem> ItemCollection { get; set; }
        public string Identifier { get; set; }
    }
}
