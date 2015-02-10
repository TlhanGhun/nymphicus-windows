using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;

namespace Nymphicus.Themes
{
    public class Theme : INotifyPropertyChanged
    {
        public static Theme CurrentTheme 
        {
            get
            {
                if (AppController.Current != null)
                {
                    return AppController.Current.CurrentTheme;
                }
                else
                {
                    return new Theme();
                }
            }
        }

        public Theme()
        {
            ItemBoxFontSizeContent = 12;
            GeneralBackgroundColor = Brushes.White;
            GeneralFontColor = Brushes.Black;
            GeneralBorderColor = Brushes.LightGray;
        }

        public Brush GeneralBackgroundColor
        {
            get
            {
                return _generalBackgroundColor;
            }
            set
            {
                _generalBackgroundColor = value;
                NotifyPropertyChanged("GeneralBackgroundColor");
            }
        }
        private Brush _generalBackgroundColor
        {
            get;
            set;
        }

        public Brush GeneralFontColor
        {
            get;
            set;
        }


        public Brush GeneralBorderColor
        {
            get;
            set;
        }

        public int GeneralBorderRadius
        {
            get;
            set;
        }

        public int ItemBoxFontSizeContent
        {
            get
            {
                return _itemBoxFontSizeContent;
            }
            set
            {
                _itemBoxFontSizeContent = value;
                NotifyPropertyChanged("ItemBoxFontSizeContent");
            }
        }
        private int _itemBoxFontSizeContent
        {
            get;
            set;
        }

     

        public int ItemBoxFontSizeFooter
        {
            get;
            set;
        }

        public Brush ItemBoxBackgroundColor
        {
            get;
            set;
        }

        public Color ItemBoxTextColor
        {
            get;
            set;
        }

        public Color ItemBoxLinkColor
        {
            get;
            set;
        }

        public Color ItemBoxHashTagColor
        {
            get;
            set;
        }

        public Color ItemBoxUsernameInTextColor
        {
            get;
            set;
        }

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

        public event PropertyChangedEventHandler PropertyChanged;


        public int ItemBoxBorderRadius
        {
            get;
            set;
        }
    }
}
