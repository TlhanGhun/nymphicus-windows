using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.Model;

namespace Nymphicus
{
    public class DebugMessage
    {
        public DateTime time { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public IItem item { get; set; }
        public IAccount account { get; set; }
        public View view { get; set; }
        public DebugMessageTypes type { get; set; }

        public enum DebugMessageTypes
        {
            General,
            Twitter,
            Facebook,
            GoogleReader,
            QuoteFM,
            GooglePlus,
            Retrival,
            Sending,
            View,
            Display,
            LogMessage,
            FirstStartWizard,
            Settings,
            TweetMarker,
            ScrollIntoView
        }

        public DebugMessage(string Title, string Text, IItem Item = null, IAccount Account = null, View VView = null, DebugMessageTypes Type = DebugMessageTypes.General)
        {
            this.time = DateTime.Now;
            this.type = Type;
            this.title = Title;
            this.text = Text;
            this.item = Item;
            this.account = Account;
            this.view = VView;

            if (string.IsNullOrWhiteSpace(this.title))
            {
                this.title = this.text;
            }
            if (account == null && item != null)
            {
                account = AppController.Current.getAccountForId(item.accountId);
            }
        }

        public override string ToString()
        {
            string returnCode = string.Format("{0}\t{1}\t{2}\t", this.time.ToLongTimeString(), this.title, this.text);
            if (item != null)
            {
                returnCode += item.DebugText;
            }
            returnCode += "\t";
            if (account != null)
            {
                returnCode += account.DebugText;
            }
            returnCode += "\t";
            if (view != null)
            {
                returnCode += view.ToString();
            }
            returnCode += "\t" + type.ToString();
            return returnCode;
        }
    }
}
