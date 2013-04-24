using log4net;
using SmallRss.Data.Models;
using SmallRss.Web.Models.Manage;
using System.Linq;
using System.Web.Mvc;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ManageController));

        private readonly IDatastore datastore;

        public ManageController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        public ActionResult Index()
        {
            return View(CreateIndexViewModel(this.CurrentUser(datastore)));
        }

        public ActionResult Edit(int id)
        {
            var vm = CreateEditViewModel(this.CurrentUser(datastore), id);
            if (vm == null)
                return RedirectToAction("index");

            return View(vm);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var user = this.CurrentUser(datastore);
            var feeds = datastore.LoadAll<UserFeed>("UserAccountId", user.Id);

            var feed = feeds.FirstOrDefault(f => f.Id == id);
            if (feed == null)
                return RedirectToAction("index");

            var removeCount = datastore.RemoveUserArticleRead(user, feed);
            log.InfoFormat("Removed {0} user article read records: {1}:{2}", removeCount, feed.Id, feed.Name);

            removeCount = datastore.Remove(feed);
            log.InfoFormat("Removed {0} feed: {1}:{2}", removeCount, feed.Id, feed.Name);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Add(AddFeedViewModel addFeed)
        {
            var user = this.CurrentUser(datastore);

            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(addFeed.GroupSel) && string.IsNullOrWhiteSpace(addFeed.Group)))
            {
                var vm = CreateIndexViewModel(user);
                vm.Error = "Missing feed URL, group or name. Please complete all fields and try again.";
                return View("Index", vm);
            }

            var rss = datastore.LoadAll<RssFeed>("Uri", addFeed.Url).FirstOrDefault();
            if (rss == null)
            {
                rss = datastore.Store(new RssFeed { Uri = addFeed.Url });
                log.InfoFormat("Created new RSS feed: {0}", addFeed.Url);
            }

            var newFeed = new UserFeed {
                GroupName = string.IsNullOrWhiteSpace(addFeed.GroupSel) ? addFeed.Group : addFeed.GroupSel,
                Name = addFeed.Name,
                RssFeedId = rss.Id,
                UserAccountId = user.Id
            };
            datastore.Store(newFeed);

            log.InfoFormat("Created new user feed: {0} - {1}", addFeed.Name, addFeed.Url);

            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Save(SaveFeedViewModel saveFeed)
        {
            var user = this.CurrentUser(datastore);
            if (!ModelState.IsValid || (string.IsNullOrWhiteSpace(saveFeed.GroupSel) && string.IsNullOrWhiteSpace(saveFeed.Group)))
            {
                var vm = CreateEditViewModel(user, saveFeed.Id);
                if (vm == null)
                    return RedirectToAction("index");

                vm.Error = "Could not update feed due to a missing feed URL, group or name. Please complete all fields and try again.";
                return View("Edit", vm);
            }

            var rss = datastore.LoadAll<RssFeed>("Uri", saveFeed.Url).FirstOrDefault();
            if (rss == null)
            {
                rss = datastore.Store(new RssFeed { Uri = saveFeed.Url });
                log.InfoFormat("Updating user feed, created new rss: {0}", saveFeed.Url);
            }

            var feed = datastore.Load<UserFeed>(saveFeed.Id);
            if (feed == null || feed.UserAccountId != user.Id)
            {
                var vm = CreateEditViewModel(user, saveFeed.Id);
                if (vm == null)
                    return RedirectToAction("index");

                vm.Error = "Could not update feed due to a security check failure. Please try again";
                return View("Edit", vm);
            }

            feed.GroupName = string.IsNullOrWhiteSpace(saveFeed.GroupSel) ? saveFeed.Group : saveFeed.GroupSel;
            feed.Name = saveFeed.Name;
            feed.RssFeedId = rss.Id;
            datastore.Update(feed);

            log.InfoFormat("Updating user feed: {0}", saveFeed.Name);

            return RedirectToAction("index");
        }

        private IndexViewModel CreateIndexViewModel(UserAccount user)
        {
            var feeds = datastore.LoadUserRssFeeds(user.Id);
            return new IndexViewModel { Feeds = feeds.Select(f => new FeedSubscriptionViewModel(f.Item1, f.Item2)).OrderBy(f => f.Name).OrderBy(f => f.Group) };
        }

        private EditViewModel CreateEditViewModel(UserAccount user, int feedId)
        {
            var feeds = datastore.LoadAll<UserFeed>("UserAccountId", user.Id);
            var feed = feeds.FirstOrDefault(f => f.Id == feedId);
            if (feed == null)
                return null;

            var rss = datastore.Load<RssFeed>(feed.RssFeedId);
            return new EditViewModel { Feed = new FeedSubscriptionViewModel(feed, rss), CurrentGroups = feeds.Select(f => f.GroupName).Distinct().OrderBy(g => g) };
        }
    }
}
