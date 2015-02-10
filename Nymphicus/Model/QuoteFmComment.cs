using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.Model
{
    public class QuoteFmComment
    {
        public QuoteFmUser Commenter { get; set; }
        public string Text { get; set; }
    }
}
