using log4net;
using QDFeedParser;
using SmallRss.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallRss.Service
{
    public class RefreshFeeds
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RefreshFeeds));

        private readonly IFeedFactory feedFactory;
        private readonly IDatastore datastore;
        private readonly object feedUpdateLock = new object();

        public RefreshFeeds(IFeedFactory feedFactory, IDatastore datastore)
        {
            this.feedFactory = feedFactory;
            this.datastore = datastore;
        }

        private Tuple<string, object, ClauseComparsion> LastUpdated(TimeSpan timeAgo)
        {
            var lastUpdatePeriod = DateTime.UtcNow - timeAgo;
            return Tuple.Create("LastUpdated", (object)lastUpdatePeriod, ClauseComparsion.GreaterThanOrEqual);
        }

        private IEnumerable<RssFeed> LoadRssFeedsForCounter(int counter)
        {
            if (counter == 0)
            {
                log.Debug("Refresh all feeds");
                return datastore.LoadAll<RssFeed>("1", 1);
            }

            if (counter % 30 == 0)
            {
                log.Debug("Refresh only feeds updated in the last 3 days");
                return datastore.LoadAll<RssFeed>(LastUpdated(TimeSpan.FromDays(3)));
            }

            if (counter % 15 == 0)
            {
                log.Debug("Refresh only feeds updated in the last day");
                return datastore.LoadAll<RssFeed>(LastUpdated(TimeSpan.FromDays(1)));
            }

            if (counter % 10 == 0)
            {
                log.Debug("Refresh only feeds updated in the last 12 hours");
                return datastore.LoadAll<RssFeed>(LastUpdated(TimeSpan.FromHours(12)));
            }

            if (counter % 5 == 0)
            {
                log.Debug("Refresh only feeds updated in the last 6 hours");
                return datastore.LoadAll<RssFeed>(LastUpdated(TimeSpan.FromHours(6)));
            }

            log.Debug("Refresh only feeds updated in the last 2 hours");
            return datastore.LoadAll<RssFeed>(LastUpdated(TimeSpan.FromHours(2)));
        }

        public void Refresh(int counter)
        {
            log.Debug("Refreshing RSS feeds...");
            try
            {
                IEnumerable<RssFeed> allRssFeeds = LoadRssFeedsForCounter(counter).ToList();
                int refreshed = 0;

                foreach (var rssFeed in allRssFeeds)
                {
                    try
                    {
                        log.InfoFormat("Refreshing {0} ", rssFeed.Uri);

                        RefreshRssFeed(rssFeed);
                        refreshed++;
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Error refreshing RSS feed {0} [{1}]: ", rssFeed.Id, rssFeed.Uri), ex);
                    }
                }

                log.DebugFormat("Done refreshing RSS feeds ({0} feeds).", refreshed);
            }
            catch (Exception ex)
            {
                log.Error("Error refreshing RSS feeds: ", ex);
            }
        }

        public void Refresh(int userAccountId, int feedId)
        {
            try
            {
                var rssFeed = datastore.Load<RssFeed>(feedId);
                if (rssFeed == null)
                    return;
                var userFeeds = datastore.LoadAll<UserFeed>(Tuple.Create<string, object, ClauseComparsion>("RssFeedId", rssFeed.Id, ClauseComparsion.Equals), Tuple.Create<string, object, ClauseComparsion>("UserAccountId", userAccountId, ClauseComparsion.Equals));
                if (!userFeeds.Any())
                    return;

                log.InfoFormat("Refreshing {0} ", rssFeed.Uri);

                RefreshRssFeed(rssFeed);
            }
            catch (Exception ex)
            {
                log.Error("Error refreshing RSS feed: ", ex);
            }
        }

        private void RefreshRssFeed(RssFeed rssFeed)
        {
            lock (feedUpdateLock)
            {
                var feed = feedFactory.CreateFeed(new Uri(rssFeed.Uri));
                var lastItemUpdate = feed.Items.Select(i => i.DatePublished.ToUniversalTime()).Concat(new[] { feed.LastUpdated.ToUniversalTime() }).Max();

                log.DebugFormat("Feed {0} was last updated {1} - our version was updated: {2}", rssFeed.Uri, lastItemUpdate, rssFeed.LastUpdated);
                if (!rssFeed.LastUpdated.HasValue || lastItemUpdate > rssFeed.LastUpdated)
                {
                    UpdateFeedItems(feed, rssFeed);
                    rssFeed.LastUpdated = lastItemUpdate;
                }
                rssFeed.LastRefreshed = DateTime.UtcNow;
                rssFeed.Link = feed.Link;
                datastore.Update(rssFeed);
            }
        }

        private void UpdateFeedItems(IFeed feed, RssFeed rssFeed)
        {
            log.Debug("There are new items, updating...");

            var existing = datastore.LoadAll<Article>("RssFeedId", rssFeed.Id).ToList();
            var oldestItem = existing.Any() ? existing.Min(a => a.Published ?? DateTime.MinValue) : DateTime.MinValue;

            foreach (var itemInFeed in feed.Items)
            {
                var existingArticle = existing.FirstOrDefault(e => e.ArticleGuid == itemInFeed.Id);
                if (existingArticle != null)
                {
                    if (itemInFeed.DatePublished.ToUniversalTime() > existingArticle.Published)
                    {
                        log.InfoFormat("Article {0} has updated ({1} - our version {2}), updating our instance", existingArticle.ArticleGuid, itemInFeed.DatePublished.ToUniversalTime(), existingArticle.Published);
                        existingArticle.Heading = itemInFeed.Title;
                        existingArticle.Body = itemInFeed.Content;
                        existingArticle.Url = itemInFeed.Link;
                        existingArticle.Published = itemInFeed.DatePublished.ToUniversalTime();
                        datastore.Update(existingArticle);
                    }
                }
                else
                {
                    log.InfoFormat("Add new article {0}|{1} to feed {2}", itemInFeed.Id, itemInFeed.Title, rssFeed.Uri);
                    if (itemInFeed.DatePublished.ToUniversalTime() < oldestItem)
                    {
                        log.InfoFormat("Article {0} is older than the oldest item...has probably already been archived so not adding.", itemInFeed.Id);
                        continue;
                    }

                    datastore.Store(new Article
                    {
                        Heading = itemInFeed.Title,
                        Body = itemInFeed.Content,
                        Url = itemInFeed.Link,
                        Published = itemInFeed.DatePublished.ToUniversalTime(),
                        ArticleGuid = itemInFeed.Id,
                        RssFeedId = rssFeed.Id
                    });
                }
            }
        }
    }
}
