using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;
using AppNetDotNet.Model;
using Nymphicus.Model;
using Nymphicus;

namespace Nymphicus.Controls.AppDotNet
{
    public class TextblockApnItem : TextBlock
    {

        public TextblockApnItem()
        {

        }

        void generateElementsTextblock(Entities inlines)
        {
            this.Inlines.Clear();
            try
            {

                ApnItem item = this.DataContext as ApnItem;
                

                string text = "";

                if (item != null)
                {
                    text = item.displayText;
                }



                this.Foreground = Brushes.Black;

                if (inlines.allEntities == null)
                {
                    this.Inlines.Add(text);
                }
                else
                {
                    if (inlines.allEntities.Count == 0)
                    {
                        this.Inlines.Add(text);
                    }
                    else
                    {
                        int startPosition = 0;
                        foreach (Entities.IEntity entity in inlines.allEntities)
                        {
                            if (typeof(Entities.Link) == entity.GetType())
                            {
                                Entities.Link innerLink = entity as Entities.Link;
                                this.Inlines.Add(text.Substring(Math.Min(startPosition, text.Length - 1), Math.Max(0, innerLink.pos - startPosition)));
                                startPosition = innerLink.pos + innerLink.len + 1;

                                Hyperlink link = new Hyperlink();
                                link.TextDecorations = null;
                                link.Foreground = Brushes.Purple;
                                Uri target;
                                Uri.TryCreate(innerLink.url, UriKind.Absolute, out target);
                                link.NavigateUri = target;
                                link.Inlines.Add(innerLink.text);
                                link.ToolTip = innerLink.url;
                                link.Cursor = Cursors.Hand;
                                link.Click += new RoutedEventHandler(link_Click);
                                ContextMenu contextMenuLink = new ContextMenu();

                                MenuItem menuItemCopyToClipboard = new MenuItem();
                                menuItemCopyToClipboard.Header = "Copy link";
                                menuItemCopyToClipboard.CommandParameter = innerLink.url;
                                menuItemCopyToClipboard.Click += new RoutedEventHandler(menuItemCopyToClipboard_Click);
                                contextMenuLink.Items.Add(menuItemCopyToClipboard);

                                MenuItem menuItemOpenInBrowser = new MenuItem();
                                menuItemOpenInBrowser.Header = "Open in webbrowser";
                                menuItemOpenInBrowser.CommandParameter = innerLink.url;
                                menuItemOpenInBrowser.Click += new RoutedEventHandler(menuItemOpenInBrowser_Click);
                                contextMenuLink.Items.Add(menuItemOpenInBrowser);


                                link.ContextMenu = contextMenuLink;
                                this.Inlines.Add(link);
                                this.Inlines.Add(" ");
                            }
                            else if (typeof(Entities.Hashtag) == entity.GetType())
                            {
                                Entities.Hashtag innerHash = entity as Entities.Hashtag;
                                this.Inlines.Add(text.Substring(Math.Min(startPosition, text.Length - 1), Math.Max(0, innerHash.pos - startPosition)));
                                startPosition = innerHash.pos + innerHash.len + 1;

                                Hyperlink link = new Hyperlink();
                                link.TextDecorations = null;
                                link.Foreground = Brushes.Red;
                                link.TargetName = innerHash.name;
                                link.Inlines.Add("#" + innerHash.name);
                                link.ToolTip = "Open hashtag #" + innerHash.name;
                                link.Cursor = Cursors.Hand;
                                link.Click += new RoutedEventHandler(hashtag_Click);
                                this.Inlines.Add(link);
                                this.Inlines.Add(" ");
                            }
                            else if (typeof(Entities.Mention) == entity.GetType())
                            {
                                Entities.Mention innerMention = entity as Entities.Mention;
                                this.Inlines.Add(text.Substring(Math.Min(startPosition, text.Length - 1), Math.Max(0, innerMention.pos - startPosition)));
                                startPosition = innerMention.pos + innerMention.len + 1;

                                Hyperlink link = new Hyperlink();
                                link.TextDecorations = null;
                                link.Foreground = Brushes.DarkGreen;
                                link.TargetName = innerMention.name;
                                link.Inlines.Add("@" + innerMention.name);
                                link.ToolTip = "Open user info of @" + innerMention.name;
                                link.Cursor = Cursors.Hand;
                                link.Click += new RoutedEventHandler(username_Click);
                                this.Inlines.Add(link);
                                this.Inlines.Add(" ");
                            }
                        }
                        if (startPosition < text.Length)
                        {
                            this.Inlines.Add(text.Substring(startPosition - 1));
                        }
                    }

                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }

        }



        void menuItemOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string url = menuItem.CommandParameter as string;
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(url);
                    }
                    catch
                    {
                    }
                }
            }
        }

        void menuItemCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                if (menuItem != null)
                {
                    string url = menuItem.CommandParameter as string;
                    if (!string.IsNullOrEmpty(url))
                    {
                        Clipboard.SetText(url);
                    }
                }
            }
            catch
            {

            }
        }

        public Entities ItemEntities
        {
            get { return (Entities)GetValue(ItemEntitiesProperty); }
            set { SetValue(ItemEntitiesProperty, value); }
        }

        public static readonly DependencyProperty ItemEntitiesProperty =
         DependencyProperty.Register(
         "ItemEntities",
         typeof(Entities),
         typeof(TextblockApnItem),
         new FrameworkPropertyMetadata(new Entities(), new PropertyChangedCallback(OnItemEntitesChanged)));



        private static void OnItemEntitesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Entities entities = args.NewValue as Entities;
            if (entities == null)
            {
                entities = new Entities();
            }
            TextblockApnItem textblock = (TextblockApnItem)obj;
            textblock.generateElementsTextblock(entities);
            return;

        }





        static void hashtag_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            if (link != null)
            {
               // AppController.Current.addHashtagSearch(link.TargetName);
            }
        }

        static void username_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            if (link != null)
            {
               // Chapper.AppController.Current.showUserInfo(link.TargetName);
            }

        }

        static void link_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            try
            {
                System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
            }
            catch
            {

            }
        }

        private static BitmapImage GetImageFromURL(string Url)
        {
            BitmapImage bImage = new BitmapImage();
            bImage.BeginInit();
            bImage.UriSource = new Uri(Url);
            bImage.CacheOption = BitmapCacheOption.OnLoad;
            bImage.EndInit();


            return bImage;

        }
    }
}
