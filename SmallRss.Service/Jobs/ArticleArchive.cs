using log4net;
using SmallRss.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallRss.Service.Jobs
{
    public class ArticleArchive : IDailyJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ArticleArchive));

        private readonly IDatastore datastore;
        private readonly int maxArticleCount = 200;

        public ArticleArchive(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        public void Run()
        {
            IEnumerable<RssFeed> allRssFeeds = datastore.LoadAll<RssFeed>("1", 1).ToList();
            foreach (var rssFeed in allRssFeeds)
            {
                try
                {
                    log.InfoFormat("Archiving {0} ", rssFeed.Uri);
                    var archived = datastore.RemoveArticles(rssFeed, maxArticleCount);
                    log.DebugFormat("Archived {0}...articles removed: {1}", rssFeed.Uri, archived);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error archiving RSS feed {0} [{1}]: ", rssFeed.Id, rssFeed.Uri), ex);
                }
            }
        }
    }
}
