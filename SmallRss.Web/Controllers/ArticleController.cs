using log4net;
using SmallRss.Data.Models;
using SmallRss.Web.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class ArticleController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ArticleController));

        private readonly IDatastore datastore;

        public ArticleController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        // GET api/article/5
        public Article Get(int id)
        {
            var article = datastore.Load<Article>(id);
            return new Article { Id = id, Body = HttpUtility.HtmlDecode(article.Body), Url = article.Url };
        }

        // POST api/article
        public void Post(ArticleReadViewModel feed)
        {
            log.DebugFormat("Marking story as {1}: {0}", feed.Story, feed.Read ? "read" : "unread");
            var user = this.CurrentUser(datastore);
            if (feed.Feed.HasValue && !feed.Story.HasValue)
            {
                var feedToMarkAllAsRead = datastore.Load<UserFeed>(feed.Feed.Value);
                if (feedToMarkAllAsRead != null && feedToMarkAllAsRead.UserAccountId == user.Id)
                {
                    foreach (var article in datastore.LoadUnreadArticlesInUserFeed(feedToMarkAllAsRead).ToList())
                        MarkAsRead(feedToMarkAllAsRead, article.Id, feed.Read);
                }
            }
            else if (feed.Story.HasValue)
            {
                var article = datastore.Load<Article>(feed.Story.Value);
                var feedToMarkAsRead = datastore.LoadAll<UserFeed>(Tuple.Create<string, object, ClauseComparsion>("RssFeedId", article.RssFeedId, ClauseComparsion.Equals), Tuple.Create<string, object, ClauseComparsion>("UserAccountId", user.Id, ClauseComparsion.Equals)).FirstOrDefault();
                if (feedToMarkAsRead != null && feedToMarkAsRead.UserAccountId == user.Id)
                {
                    MarkAsRead(feedToMarkAsRead, article.Id, feed.Read);
                }
                else
                {
                    log.WarnFormat("Feed {0} could not be found or is not associated with the current user, will not make any changes", feed.Feed);
                }
            }
        }

        private void MarkAsRead(UserFeed feed, int articleId, bool read)
        {
            var userArticleRead = new UserArticlesRead { UserAccountId = feed.UserAccountId, UserFeedId = feed.Id, ArticleId = articleId };
            if (read) datastore.Store(userArticleRead);
            else datastore.RemoveUserArticleRead(userArticleRead);
        }
    }
}
