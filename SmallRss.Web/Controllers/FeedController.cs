using HtmlAgilityPack;
using log4net;
using SmallRss.Data.Models;
using SmallRss.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class FeedController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FeedController));

        private readonly IDatastore datastore;

        public FeedController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        // GET api/feed
        public IEnumerable<object> Get()
        {
            log.Debug("Getting all feeds from db");
            
            var loggedInUser = this.CurrentUser(datastore);
            var userFeeds = datastore.LoadAll<UserFeed>("UserAccountId", loggedInUser.Id);

            if (!userFeeds.Any())
                return new[] { new { id = "", item = "" } };

            return userFeeds.GroupBy(f => f.GroupName).OrderBy(g => g.Key).Select(group =>
                new
                {
                    id = group.Key,
                    item = group.Key,
                    props = new { isFolder = true, open = loggedInUser.ExpandedGroups.Contains(group.Key) },
                    items = group.OrderBy(g => g.Name).Select(g =>
                        new { id = g.Id, item = g.Name, props = new { isFolder = false } })
                });
        }

        // GET api/feed/5
        public IEnumerable<object> Get(int id, int? offset)
        {
            log.DebugFormat("Getting articles for feed {0} from db, using client UTC offset {1}", id, offset);

            var loggedInUser = this.CurrentUser(datastore);
            var feed = datastore.Load<UserFeed>(id);
            var readArticles = datastore.LoadAll<UserArticlesRead>("UserFeedId", feed.Id).ToList();

            IEnumerable<Article> articles;
            if (loggedInUser.ShowAllItems)
            {
                articles = datastore.LoadAll<Article>("RssFeedId", feed.RssFeedId);
            }
            else
            {
                articles = datastore.LoadUnreadArticlesInUserFeed(feed);
            }

            return articles
                .OrderBy(a => a.Published)
                .Select(a => new { read = readArticles.Any(uar => uar.ArticleId == a.Id), feed = id, story = a.Id, heading = a.Heading, article = HtmlPreview.Preview(a.Body), posted = FriendlyDate.ToString(a.Published, offset) });
        }
    }
}
