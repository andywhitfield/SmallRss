using log4net;
using SmallRss.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallRss.Service.Jobs
{
    public class RemoveOrphanedRssFeeds : IDailyJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RemoveOrphanedRssFeeds));

        private readonly IDatastore datastore;

        public RemoveOrphanedRssFeeds(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        public void Run()
        {
            IEnumerable<RssFeed> orphanedRssFeeds = datastore.LoadAll<RssFeed>("select * from RssFeed where Id not in (select RssFeedId from UserFeed)").ToList();
            foreach (var rssFeed in orphanedRssFeeds)
            {
                try
                {
                    log.InfoFormat("Archiving all articles of {0} ", rssFeed.Uri);
                    var archived = datastore.RemoveArticles(rssFeed, 0);
                    log.DebugFormat("Archived {0}...articles removed: {1}", rssFeed.Uri, archived);
                    datastore.Remove(rssFeed);
                    log.InfoFormat("Removed feed {0}.", rssFeed.Uri);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error removing RSS feed {0} [{1}]: ", rssFeed.Id, rssFeed.Uri), ex);
                }
            }
        }
    }
}
