using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nymphicus.Converter
{
    public class ItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double width = (double)value;
                double difference = System.Convert.ToInt64(parameter);
                if (width != 0)
                {
                    return width - difference;
                }
                else
                {
                    return value;
                }

            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double width = (double)value;
                double difference = System.Convert.ToInt64(parameter);
                if (width != 0)
                {
                    return width + difference;
                }
                else
                {
                    return value;
                }

            }
            catch
            {
                return value;
            }
        }
    }
}
