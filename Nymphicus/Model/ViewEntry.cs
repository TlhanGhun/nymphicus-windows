using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.Model
{
    public class ViewEntry
    {
        public View view { get; set; }
        public IItem item { get; set; }

        public ViewEntry(View usedView, IItem usedItem)
        {
            view = usedView;
            item = usedItem;
        }
    }
}
