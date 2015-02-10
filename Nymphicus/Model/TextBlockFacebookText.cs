using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xaml;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace Nymphicus.Model
{
    public class TextBlockFacebookText : TextBlock
    {
        public FacebookItem Item
        {
            get { return (FacebookItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
                DependencyProperty.Register(
                "FacebookItem",
                typeof(FacebookItem),
                typeof(TextBlockFacebookText),
                new FrameworkPropertyMetadata(new FacebookItem(), new PropertyChangedCallback(OnItemChanged)));



        private static void OnItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FacebookItem item = args.NewValue as FacebookItem;
                       if (item == null)
            {
                return;
            }

                       TextBlockFacebookText textblock = (TextBlockFacebookText)obj;
                     //  textblock = item.Textblock;
        }
    }
}
