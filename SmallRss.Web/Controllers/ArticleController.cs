using Raven.Client;
using SmallRss.Web.Models;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class ArticleController : ApiController
    {
        private readonly IDocumentSession documentSession;

        public ArticleController(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        // GET api/article/5
        public Article Get(int id)
        {
            var article = documentSession.Load<Article>(id);
            return new Article { Id = id, ArticleBody = HttpUtility.HtmlDecode(article.ArticleBody), ArticleUrl = article.ArticleUrl };
        }

        // POST api/article
        public void Post(ArticleReadViewModel feed)
        {
            Trace.TraceInformation("Marking story as {1}: {0}", feed.Story, feed.Read ? "read" : "unread");
            var currentUser = this.CurrentUser(documentSession);

            if (feed.Feed.HasValue && !feed.Story.HasValue)
            {
                var feedToMarkAllAsRead = documentSession.Load<Feed>(feed.Feed.Value);
                if (feedToMarkAllAsRead != null && currentUser.Feeds.Any(f => f == feedToMarkAllAsRead.Id))
                {
                    foreach (var article in documentSession.Query<Article>().Where(a => a.RssId == feedToMarkAllAsRead.RssId).Take(FeedConstants.MaxArticles))
                        MarkAsRead(feedToMarkAllAsRead, article.Id, feed.Read);
                    documentSession.Store(feedToMarkAllAsRead);
                }
                else
                {
                    Trace.TraceWarning("Feed {0} could not be found or is not associated with the current user, will not make any changes", feed.Feed);
                }
            }
            else if (feed.Story.HasValue)
            {
                var article = documentSession.Load<Article>(feed.Story.Value);
                var feedToMarkAsRead = documentSession.Query<Feed>().Where(f => f.RssId == article.RssId).FirstOrDefault();
                if (feedToMarkAsRead != null && currentUser.Feeds.Any(f => f == feedToMarkAsRead.Id))
                {
                    MarkAsRead(feedToMarkAsRead, feed.Story.Value, feed.Read);
                    documentSession.Store(feedToMarkAsRead);
                }
                else
                {
                    Trace.TraceWarning("Feed {0} could not be found or is not associated with the current user, will not make any changes", feed.Feed);
                }
            }
        }

        private void MarkAsRead(Feed feed, int articleId, bool read)
        {
            if (read) feed.ArticlesRead.Add(articleId);
            else feed.ArticlesRead.Remove(articleId);
        }
    }
}
