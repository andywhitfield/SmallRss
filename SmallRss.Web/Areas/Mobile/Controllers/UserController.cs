using log4net;
using SmallRss.Data.Models;
using SmallRss.Web.Areas.Mobile.Models.User;
using SmallRss.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmallRss.Web.Areas.Mobile.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private static ILog log = LogManager.GetLogger(typeof(UserController));

        private readonly IDatastore datastore;

        public UserController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        public ActionResult Feed(int id, int? offset)
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

            return View(new FeedViewModel
            {
                FeedId = feed.Id,
                GroupName = feed.Name,
                ShowingAll = loggedInUser.ShowAllItems,
                Articles =
                    articles
                    .OrderBy(a => a.Published)
                    .Select(a => new FeedViewModel.ArticleSummary { Id = a.Id, Read = readArticles.Any(uar => uar.ArticleId == a.Id), Title = a.Heading, Summary = HtmlPreview.Preview(a.Body), Posted = FriendlyDate.ToString(a.Published, offset ?? 0) })
            });
        }

        public ActionResult Article(int id)
        {
            var loggedInUser = this.CurrentUser(datastore);
            var article = datastore.Load<Article>(id);
            if (article == null)
                return RedirectToAction("", "");

            var rssFeed = datastore.Load<RssFeed>(article.RssFeedId);
            if (rssFeed == null)
                return RedirectToAction("", "");

            var userFeed = datastore.LoadAll<UserFeed>(Tuple.Create("RssFeedId", (object)rssFeed.Id, ClauseComparsion.Equals), Tuple.Create("UserAccountId", (object)loggedInUser.Id, ClauseComparsion.Equals)).FirstOrDefault();
            if (userFeed == null)
                return RedirectToAction("", "");

            MarkAsRead(userFeed, article.Id, true);

            return View(new ArticleViewModel
            {
                ArticleId = article.Id,
                FeedId = userFeed.Id,
                FeedName = userFeed.Name,
                GroupName = userFeed.GroupName,
                Title = article.Heading,
                Body = HttpUtility.HtmlDecode(article.Body),
                Url = article.Url
            });
        }

        public ActionResult ExpandCollapse(string group, bool? expanded)
        {
            log.DebugFormat("Updating user settings - group: {0}; expanded: {1}", group, expanded);

            var user = this.CurrentUser(datastore);
            if (expanded.HasValue && !string.IsNullOrEmpty(group))
            {
                if (expanded.Value) user.ExpandedGroups.Add(group);
                else user.ExpandedGroups.Remove(group);
            }
            datastore.UpdateAccount(user);

            return new EmptyResult();
        }

        public ActionResult ShowAllUnread(bool showAll)
        {
            var loggedInUser = this.CurrentUser(datastore);
            loggedInUser.ShowAllItems = showAll;
            datastore.UpdateAccount(loggedInUser);
            return new EmptyResult();
        }

        public ActionResult MarkAllRead(int feed)
        {
            var loggedInUser = this.CurrentUser(datastore);

            var feedToMarkAllAsRead = datastore.Load<UserFeed>(feed);
            if (feedToMarkAllAsRead != null && feedToMarkAllAsRead.UserAccountId == loggedInUser.Id)
            {
                foreach (var article in datastore.LoadUnreadArticlesInUserFeed(feedToMarkAllAsRead).ToList())
                    MarkAsRead(feedToMarkAllAsRead, article.Id, true);
            }

            return new EmptyResult();
        }

        public ActionResult KeepUnread(int article)
        {
            var loggedInUser = this.CurrentUser(datastore);
            var articleToMarkUnread = datastore.Load<Article>(article);
            var rssFeed = datastore.Load<RssFeed>(articleToMarkUnread.RssFeedId);
            var userFeed = datastore.LoadAll<UserFeed>(Tuple.Create("RssFeedId", (object)rssFeed.Id, ClauseComparsion.Equals), Tuple.Create("UserAccountId", (object)loggedInUser.Id, ClauseComparsion.Equals)).FirstOrDefault();
            if (userFeed == null)
                return new EmptyResult();

            MarkAsRead(userFeed, article, false);

            return new EmptyResult();
        }

        private void MarkAsRead(UserFeed feed, int articleId, bool read)
        {
            var userArticleRead = new UserArticlesRead { UserAccountId = feed.UserAccountId, UserFeedId = feed.Id, ArticleId = articleId };
            if (read) datastore.Store(userArticleRead);
            else datastore.RemoveUserArticleRead(userArticleRead);
        }

    }
}
