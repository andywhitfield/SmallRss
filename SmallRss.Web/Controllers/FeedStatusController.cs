using Raven.Client;
using Raven.Client.Linq;
using SmallRss.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class FeedStatusController : ApiController
    {
        private readonly IDocumentSession documentSession;

        public FeedStatusController(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        // GET api/feedstatus
        public IEnumerable<object> Get()
        {
            var currentUser = this.CurrentUser(documentSession);
            var feedsByGroup = documentSession.Load<Feed>(currentUser.Feeds.Cast<ValueType>()).Take(FeedConstants.MaxFeeds).GroupBy(f => f.GroupName).ToList();

            foreach (var group in feedsByGroup)
            {
                var unreadFeedsInGroup = new List<Tuple<int, int>>();
                var readArticles = group.SelectMany(f => f.ArticlesRead.Take(FeedConstants.MaxArticles)).ToList();
                using (var feedStatusForGroupSession = documentSession.Advanced.DocumentStore.OpenSession())
                {
                    var rssIdsForGroup = group.Select(f => f.RssId).ToList();
                    var unreadArticles = feedStatusForGroupSession
                          .Query<Article>()
                          .Where(a => a.RssId.In(rssIdsForGroup))
                          .Take(group.Count() * FeedConstants.MaxArticles) // we allow MaxArticles per feed, so we need to be able to load MaxArticles for all the feeds in the group
                          .Select(a => new { Id = a.Id, RssId = a.RssId })
                          .ToList()
                          .Where(a => !readArticles.Contains(a.Id));

                    Trace.TraceInformation("Getting unread items for {0}: {1}", group.Key, unreadArticles.Count());

                    foreach (var feed in group)
                    {
                        var unreadFeedCount = unreadArticles.Count(a => feed.RssId == a.RssId);
                        unreadFeedsInGroup.Add(Tuple.Create(feed.Id, unreadFeedCount));
                        Trace.TraceInformation("Getting unread items for {0}/{1}: {2}", group.Key, feed.Name, unreadFeedCount);
                    }
                }
                yield return new
                {
                    label = group.Key,
                    items = unreadFeedsInGroup.Select(g =>
                        new { value = g.Item1, unread = g.Item2 }),
                    unread = unreadFeedsInGroup.Sum(f => f.Item2)
                };
            }
        }

        // POST api/feedstatus
        public void Post(FeedStatusViewModel status)
        {
            var currentUser = this.CurrentUser(documentSession);
            if (status.Expanded.HasValue && !string.IsNullOrWhiteSpace(status.Group))
            {
                if (status.Expanded.Value) currentUser.Expanded.Add(status.Group);
                else currentUser.Expanded.Remove(status.Group);
            }
            if (status.ShowAll.HasValue)
            {
                currentUser.ShowAllItems = status.ShowAll.Value;
            }
        }
    }
}