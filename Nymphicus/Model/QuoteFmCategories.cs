using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuoteSharp;
using System.ComponentModel;
using System.Windows.Threading;

namespace Nymphicus.Model
{
    public class QuoteFmCategories
    {
        public static QuoteFmCategories Categories;
        public ListOfCategories catagories;
        public Dictionary<decimal,string> CategoryNames { get; set; }

        public Dictionary<decimal, ThreadSaveObservableCollection<QuoteFmItem>> Collections { get; set; }

        private BackgroundWorker backgroundWorkerCategories;
        public bool InitialUpdateDone { get; set; }

        public static void Create() {
            if(Categories == null) {
                Categories = new QuoteFmCategories();
            }
        }

        private QuoteFmCategories()
        {
            Collections = new Dictionary<decimal, ThreadSaveObservableCollection<QuoteFmItem>>();
            CategoryNames = new Dictionary<decimal, string>();
            catagories = QuoteSharp.API.getCategories();
            if(catagories != null) {
                if (catagories.entities != null)
                {
                    foreach (Category category in catagories.entities)
                    {
                        if (category != null)
                        {
                            Collections.Add(category.id, new ThreadSaveObservableCollection<QuoteFmItem>());
                            CategoryNames.Add(category.id, category.name);
                            AppController.Current.registerNotificationClass("QUOTE.fm Category " + category.name);
                        }
                    }
                }
            }

            backgroundWorkerCategories = new BackgroundWorker();
            backgroundWorkerCategories.WorkerReportsProgress = true;
            backgroundWorkerCategories.WorkerSupportsCancellation = true;
            backgroundWorkerCategories.DoWork += new DoWorkEventHandler(backgroundWorkerCategories_DoWork);
            backgroundWorkerCategories.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerCategories_RunWorkerCompleted);
            backgroundWorkerCategories.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerCategories_ProgressChanged);
        }

        public void UpdateItems()
        {
            if (!backgroundWorkerCategories.IsBusy)
            {
                backgroundWorkerCategories.RunWorkerAsync();
            }
        }


        #region Background Workers
        #region DoWorks

        void backgroundWorkerCategories_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e != null)
            {
                if (e.Cancel)
                {
                    return;
                }
            }
            foreach (KeyValuePair<decimal, ThreadSaveObservableCollection<QuoteFmItem>> keyValuePair in Collections)
            {
                List<decimal> catId = new List<decimal>();
                catId.Add(keyValuePair.Key);
                ListOfArticles entries = QuoteSharp.API.getArticlesListByCategories(catId);
                if (entries != null)
                {
                    if (entries.entities != null)
                    {
                        foreach (Article article in entries.entities)
                        {
                            QuoteFmItem item = QuoteFmItem.createFromApi(article);
                            if (item != null)
                            {
                                item.IsArticleOfCategory = true;
                                item.CategoryTitle = CategoryNames[keyValuePair.Key];
                                item.QuoteType = QuoteFmItem.QuoteTypes.Recommendation;
                                IEnumerable<QuoteFmItem> availableItems = keyValuePair.Value.Where(i => i.Id == item.Id && i.IsArticleOfCategory);
                                if (availableItems.Count() > 0)
                                {
                                    continue;
                                }
                                backgroundWorkerCategories.ReportProgress(100, new KeyValuePair<decimal, QuoteFmItem>(keyValuePair.Key, item));
                            }
                        }

                    }
                }
            }
                        
         
        }

        #endregion

        #region Report progress / Progress changed


        void backgroundWorkerCategories_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            KeyValuePair<decimal, QuoteFmItem>? newItemPair = e.UserState as KeyValuePair<decimal, QuoteFmItem>?;
            if (newItemPair != null)
            {
                Collections[newItemPair.Value.Key].Add(newItemPair.Value.Value);
                if (InitialUpdateDone)
                {
                    AppController.Current.sendNotification("QUOTE.fm Category " + CategoryNames[newItemPair.Value.Key], newItemPair.Value.Value.Author.Fullname, newItemPair.Value.Value.QuotedText, newItemPair.Value.Value.Author.Avatar, newItemPair.Value.Value);
                }
            }
        }

        #endregion

        #region Worker completed

        void backgroundWorkerCategories_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!InitialUpdateDone)
            {
                InitialUpdateDone = true;
            }
        }

        #endregion
        #endregion
        
    }
}
