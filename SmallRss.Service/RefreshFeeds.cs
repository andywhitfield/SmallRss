using QDFeedParser;
using SmallRss.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SmallRss.Service
{
    public class RefreshFeeds
    {
        private readonly IFeedFactory feedFactory;
        private readonly IDatastore datastore;

        public RefreshFeeds(IFeedFactory feedFactory, IDatastore datastore)
        {
            this.feedFactory = feedFactory;
            this.datastore = datastore;
        }

        public void Refresh()
        {
            Trace.TraceInformation("Refreshing all RSS feeds...");
            try
            {
                IEnumerable<RssFeed> allRssFeeds = datastore.Load<RssFeed>("1", 1).ToList();
                int refreshed = 0;

                foreach (var rssFeed in allRssFeeds)
                {
                    try
                    {
                        Trace.TraceInformation("Refreshing {0} ", rssFeed.Uri);

                        RefreshRssFeed(rssFeed);
                        refreshed++;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceInformation("Error refreshing RSS feed {0} [{1}]: {2}", rssFeed.Id, rssFeed.Uri, ex);
                    }
                }

                Trace.TraceInformation("Done refreshing all RSS feeds ({0} feeds).", refreshed);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Error refreshing RSS feeds: {0}", ex);
            }
        }

        private void RefreshRssFeed(RssFeed rssFeed)
        {
            var feed = feedFactory.CreateFeed(new Uri(rssFeed.Uri));
            var lastItemUpdate = feed.Items.Select(i => i.DatePublished.ToUniversalTime()).Concat(new[] { feed.LastUpdated.ToUniversalTime() }).Max();

            Trace.TraceInformation("Feed {0} was last updated {1} - our version was updated: {2}", rssFeed.Uri, lastItemUpdate, rssFeed.LastUpdated);
            if (!rssFeed.LastUpdated.HasValue || lastItemUpdate > rssFeed.LastUpdated)
            {
                UpdateFeedItems(feed, rssFeed);
                rssFeed.LastUpdated = lastItemUpdate;
            }
            rssFeed.LastRefreshed = DateTime.UtcNow;
            datastore.Update(rssFeed);
        }

        private void UpdateFeedItems(IFeed feed, RssFeed rssFeed)
        {
            Trace.TraceInformation("There are new items, updating...");

            var existing = datastore.Load<Article>("RssFeedId", rssFeed.Id).ToList();
            foreach (var itemInFeed in feed.Items)
            {
                var existingArticle = existing.FirstOrDefault(e => e.ArticleGuid == itemInFeed.Id);
                if (existingArticle != null)
                {
                    if (itemInFeed.DatePublished.ToUniversalTime() > existingArticle.Published)
                    {
                        Trace.TraceInformation("Article {0} has updated ({1} - our version {2}), updating our instance", existingArticle.ArticleGuid, itemInFeed.DatePublished.ToUniversalTime(), existingArticle.Published);
                        existingArticle.Heading = itemInFeed.Title;
                        existingArticle.Body = itemInFeed.Content;
                        existingArticle.Url = itemInFeed.Link;
                        existingArticle.Published = itemInFeed.DatePublished.ToUniversalTime();
                        datastore.Update(existingArticle);
                    }
                }
                else
                {
                    Trace.TraceInformation("Add new article {0}|{1} to feed {2}", itemInFeed.Id, itemInFeed.Title, rssFeed.Uri);
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
