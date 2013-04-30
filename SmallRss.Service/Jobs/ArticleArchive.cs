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
                    ArchiveArticles(rssFeed);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error archiving RSS feed {0} [{1}]: ", rssFeed.Id, rssFeed.Uri), ex);
                }
            }
        }

        private void ArchiveArticles(RssFeed feed)
        {
            
        }
    }
}
