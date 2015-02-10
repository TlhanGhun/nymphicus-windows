using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.Model
{

    public class FacebookApplication
    {
        public string Name { get; set; }
        public string CanvasName { get; set; }
        public string Id { get; set; }
        public string Link
        {
            get
            {
                if (!string.IsNullOrEmpty(_link))
                {
                    return _link;
                }
                else
                {
                    return "https://www.facebook.com/apps/application.php?id=" + Id;
                }
            }
            set
            {
                _link = value;
            }
        }
        private string _link { get; set; }
    }
}
