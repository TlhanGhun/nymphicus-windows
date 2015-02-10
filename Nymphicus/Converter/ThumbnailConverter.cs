using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nymphicus.Converter
{
    public class ThumbnailConverter : IValueConverter
    {
        public int DecodeWidth
        {
            get;
            set;
        }

         public ThumbnailConverter()
         {
             DecodeWidth = 48;
         }

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
        var path = value as string;

        if ( !string.IsNullOrEmpty( path ) ) {

            try
            {
                BitmapImage bi = new BitmapImage();

                bi.BeginInit();
                bi.DecodePixelWidth = DecodeWidth;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(path);
                bi.EndInit();

                return bi;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
        throw new NotImplementedException();
    }
    }
}
