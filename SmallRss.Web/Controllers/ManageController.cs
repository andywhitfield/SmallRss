using Raven.Client;
using Raven.Client.Linq;
using SmallRss.Web.Models;
using SmallRss.Web.Models.Manage;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly IDocumentSession documentSession;

        public ManageController(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public ActionResult Index()
        {
            var user = this.CurrentUser(documentSession);
            var feeds = documentSession.Query<Feed>().Where(f => f.Id.In(user.Feeds)).Take(FeedConstants.MaxFeeds).ToList();
            var rssIds = feeds.Select(f => f.RssId);
            var rss = documentSession.Query<Rss>().Where(r => r.Id.In(rssIds)).Take(FeedConstants.MaxFeeds).ToList();
            var feedSubscriptions = from f in feeds
                                    join r in rss on f.RssId equals r.Id
                                    orderby f.GroupName, f.Name
                                    select new FeedSubscriptionViewModel(f, r);
            return View(new IndexViewModel { Feeds = feedSubscriptions });
        }

        public ActionResult Edit(int id)
        {
            var user = this.CurrentUser(documentSession);

            var feeds = documentSession.Query<Feed>().Where(f => f.Id.In(user.Feeds)).Take(FeedConstants.MaxFeeds).ToList();
            var feed = feeds.FirstOrDefault(f => f.Id == id);
            if (feed == null || !user.Feeds.Contains(id))
                return RedirectToAction("index");

            var rss = documentSession.Load<Rss>(feed.RssId);
            return View(new EditViewModel { Feed = new FeedSubscriptionViewModel(feed, rss), CurrentGroups = feeds.Select(f => f.GroupName).Distinct().OrderBy(g => g) });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var user = this.CurrentUser(documentSession);

            if (!user.Feeds.Contains(id))
                return RedirectToAction("index");

            var feed = documentSession.Load<Feed>(id);
            if (feed == null)
                return RedirectToAction("index");

            user.Feeds.Remove(id);
            documentSession.Store(user);
            documentSession.Delete(feed);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Add(AddFeedViewModel addFeed)
        {
            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(addFeed.GroupSel) && string.IsNullOrWhiteSpace(addFeed.Group)))
            {
                return RedirectToAction("index");
            }

            var user = this.CurrentUser(documentSession);

            var rss = documentSession.Query<Rss>().Where(r => r.Url.Equals(addFeed.Url, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rss == null)
            {
                rss = new Rss { Url = addFeed.Url };
                documentSession.Store(rss);
            }

            var newFeed = new Feed { GroupName = string.IsNullOrWhiteSpace(addFeed.Group) ? addFeed.GroupSel : addFeed.Group, Name = addFeed.Name, RssId = rss.Id };
            documentSession.Store(newFeed);

            user.Feeds.Add(newFeed.Id);
            documentSession.Store(user);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Save(SaveFeedViewModel saveFeed)
        {
            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(saveFeed.GroupSel) && string.IsNullOrWhiteSpace(saveFeed.Group)))
            {
                return RedirectToAction("index");
            }

            var user = this.CurrentUser(documentSession);

            var rss = documentSession.Query<Rss>().Where(r => r.Url.Equals(saveFeed.Url, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (rss == null)
            {
                rss = new Rss { Url = saveFeed.Url };
                documentSession.Store(rss);
            }

            var feed = documentSession.Load<Feed>(saveFeed.Id);
            if (feed == null || !user.Feeds.Contains(feed.Id))
                return RedirectToAction("index");

            feed.GroupName = string.IsNullOrWhiteSpace(saveFeed.Group) ? saveFeed.GroupSel : saveFeed.Group;
            feed.Name = saveFeed.Name;
            feed.RssId = rss.Id;
            documentSession.Store(feed);

            return RedirectToAction("index");
        }
    }
}
