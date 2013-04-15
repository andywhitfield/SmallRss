using SmallRss.Data.Models;
using SmallRss.Web.Models.Manage;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly IDatastore datastore;

        public ManageController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        public ActionResult Index()
        {
            var user = this.CurrentUser(datastore);
            var feeds = datastore.LoadUserRssFeeds(user.Id);

            return View(new IndexViewModel { Feeds = feeds.Select(f => new FeedSubscriptionViewModel(f.Item1, f.Item2)).OrderBy(f => f.Group).OrderBy(f => f.Name) });
        }

        public ActionResult Edit(int id)
        {
            var user = this.CurrentUser(datastore);

            var feeds = datastore.Load<UserFeed>("UserAccountId", user.Id);
            var feed = feeds.FirstOrDefault(f => f.Id == id);
            if (feed == null)
                return RedirectToAction("index");

            var rss = datastore.Load<RssFeed>(feed.RssFeedId);
            return View(new EditViewModel { Feed = new FeedSubscriptionViewModel(feed, rss), CurrentGroups = feeds.Select(f => f.GroupName).Distinct().OrderBy(g => g) });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var user = this.CurrentUser(datastore);
            var feeds = datastore.Load<UserFeed>("UserAccountId", user.Id);

            var feed = feeds.FirstOrDefault(f => f.Id == id);
            if (feed == null)
                return RedirectToAction("index");

            var removeCount = datastore.Remove(feed);
            Trace.TraceInformation("Removed {0} feed: {1}:{2}", removeCount, feed.Id, feed.Name);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Add(AddFeedViewModel addFeed)
        {
            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(addFeed.GroupSel) && string.IsNullOrWhiteSpace(addFeed.Group)))
            {
                return RedirectToAction("index");
            }

            var user = this.CurrentUser(datastore);

            var rss = datastore.Load<RssFeed>("Uri", addFeed.Url).FirstOrDefault();
            if (rss == null)
            {
                rss = datastore.Store(new RssFeed { Uri = addFeed.Url });
            }

            var newFeed = new UserFeed {
                GroupName = string.IsNullOrWhiteSpace(addFeed.Group) ? addFeed.GroupSel : addFeed.Group,
                Name = addFeed.Name,
                RssFeedId = rss.Id,
                UserAccountId = user.Id
            };
            datastore.Store(newFeed);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Save(SaveFeedViewModel saveFeed)
        {
            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(saveFeed.GroupSel) && string.IsNullOrWhiteSpace(saveFeed.Group)))
            {
                return RedirectToAction("index");
            }

            var user = this.CurrentUser(datastore);

            var rss = datastore.Load<RssFeed>("Uri", saveFeed.Url).FirstOrDefault();
            if (rss == null)
            {
                rss = datastore.Store(new RssFeed { Uri = saveFeed.Url });
            }

            var feed = datastore.Load<UserFeed>(saveFeed.Id);
            if (feed == null || feed.UserAccountId != user.Id)
                return RedirectToAction("index");

            feed.GroupName = string.IsNullOrWhiteSpace(saveFeed.Group) ? saveFeed.GroupSel : saveFeed.Group;
            feed.Name = saveFeed.Name;
            feed.RssFeedId = rss.Id;
            datastore.Update(feed);

            return RedirectToAction("index");
        }
    }
}
