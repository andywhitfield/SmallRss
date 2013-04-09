using QDFeedParser;
using Raven.Client;
using Raven.Client.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SmallRss.Web.Models.BackgroundJobs
{
    public class RefreshFeeds
    {
        private readonly IFeedFactory feedFactory;
        private readonly IDocumentStore documentStore;

        public RefreshFeeds(IFeedFactory feedFactory, IDocumentStore documentStore)
        {
            this.feedFactory = feedFactory;
            this.documentStore = documentStore;
        }

        public void Refresh()
        {
            Trace.TraceInformation("Refreshing all RSS feeds...");
            try
            {
                IList<Rss> allRssFeeds;
                using (var documentSession = documentStore.OpenSession())
                {
                    allRssFeeds = documentSession.Query<Rss>().Take(FeedConstants.MaxFeeds).ToList();
                }

                foreach (var rssFeed in allRssFeeds)
                {
                    try
                    {
                        Trace.TraceInformation("Refreshing {0} ", rssFeed.Url);

                        RefreshRssFeed(rssFeed);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceInformation("Error refreshing RSS feed {0} [{1}]: {2}", rssFeed.Id, rssFeed.Url, ex);
                    }
                }

                Trace.TraceInformation("Done refreshing all RSS feeds ({0} feeds).", allRssFeeds.Count);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Error refreshing RSS feeds: {0}", ex);
            }
        }

        private void RefreshRssFeed(Rss rssFeed)
        {
            using (var documentSession = documentStore.OpenSession())
            {
                var feed = feedFactory.CreateFeed(new Uri(rssFeed.Url));
                var lastItemUpdate = feed.Items.Select(i => i.DatePublished.ToUniversalTime()).Concat(new [] { feed.LastUpdated.ToUniversalTime() }).Max();

                Trace.TraceInformation("Feed {0} was last updated {1} - our version was updated: {2}", rssFeed.Url, lastItemUpdate, rssFeed.LastUpdated);
                if (lastItemUpdate > rssFeed.LastUpdated)
                {
                    UpdateFeedItems(documentSession, feed, rssFeed);
                    rssFeed.LastUpdated = lastItemUpdate;
                }
                rssFeed.LastRefresh = DateTime.UtcNow;
                documentSession.Store(rssFeed);
                documentSession.SaveChanges();
            }
        }

        private static void UpdateFeedItems(IDocumentSession documentSession, IFeed feed, Rss rssFeed)
        {
            Trace.TraceInformation("There are new items, updating...");

            var feedItemGuids = feed.Items.Select(f => f.Id).ToList();
            var existing = (from article in documentSession.Query<Article>()
                            where article.ArticleGuid.In(feedItemGuids) && article.RssId == rssFeed.Id
                            select article).Take(FeedConstants.MaxArticles).ToList();
            foreach (var itemInFeed in feed.Items)
            {
                var existingArticle = existing.FirstOrDefault(e => e.ArticleGuid == itemInFeed.Id);
                if (existingArticle != null)
                {
                    if (itemInFeed.DatePublished.ToUniversalTime() > existingArticle.Date)
                    {
                        Trace.TraceInformation("Article {0} has updated ({1} - our version {2}), updating our instance", existingArticle.ArticleGuid, itemInFeed.DatePublished.ToUniversalTime(), existingArticle.Date);
                        existingArticle.Heading = itemInFeed.Title;
                        existingArticle.ArticleBody = itemInFeed.Content;
                        existingArticle.ArticleUrl = itemInFeed.Link;
                        existingArticle.Date = itemInFeed.DatePublished.ToUniversalTime();
                        documentSession.Store(existingArticle);
                    }
                }
                else
                {
                    Trace.TraceInformation("Add new article {0}|{1} to feed {2}", itemInFeed.Id, itemInFeed.Title, rssFeed.Url);
                    documentSession.Store(new Article
                    {
                        Heading = itemInFeed.Title,
                        ArticleBody = itemInFeed.Content,
                        ArticleUrl = itemInFeed.Link,
                        Date = itemInFeed.DatePublished.ToUniversalTime(),
                        ArticleGuid = itemInFeed.Id,
                        RssId = rssFeed.Id
                    });
                }
            }
        }
    }
}