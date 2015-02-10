using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.Model
{
    public interface IItem
    {
        string TextContent { get; set; }
        decimal accountId { get; set; }
        string AuthorName { get; set; }
        string Text {get;set;}
        decimal Id { get; set; }
        string ClientName { get; }
        IAccount ReceivingAccount { get; set; }

        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        DateTime SortDate { get; }

        bool HumanReadableUpdateInProgress {get; }
        string DebugText { get; }
    }
}
