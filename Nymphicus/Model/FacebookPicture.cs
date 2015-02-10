using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.Model
{

    public class FacebookPicture
    {
        public string Id { get; set; }
        public string ThumbnailPath
        {
            get
            {
                if (_thumbnailPath != null)
                {
                    return _thumbnailPath;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _thumbnailPath = value;
            }
        }
        private string _thumbnailPath { get; set; }
        public string FullImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Link { get; set; }
        public string Caption { get; set; }

        public FacebookPicture()
        {
            FullImagePath = "";
            ThumbnailPath = "";
        }
    }
}
