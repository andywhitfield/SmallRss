using SmallRss.Data.Models;
using SmallRss.Web.Models;
using SmallRss.Web.Areas.Mobile.Models.Home;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmallRss.Web.Areas.Mobile.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatastore datastore;

        public HomeController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        [Authorize]
        public ActionResult Index()
        {
            Response.AppendHeader(
                "X-XRDS-Location",
                new Uri(Request.Url, Response.ApplyAppPathModifier("~/m/home/xrds")).AbsoluteUri);

            var loggedInUser = this.CurrentUser(datastore);
            var userFeeds = datastore.LoadAll<UserFeed>("UserAccountId", loggedInUser.Id);
            var unreadCount = datastore.GetUnreadFeeds(loggedInUser.Id);

            var userGroupsAndFeeds = userFeeds.GroupBy(f => f.GroupName).OrderBy(g => g.Key).Select(group =>
                new IndexViewModel.Item
                {
                    Id = group.Key,
                    Name = group.Key,
                    Open = loggedInUser.ExpandedGroups.Contains(group.Key),
                    Unread = unreadCount.Sum(u => u.GroupName == group.Key ? u.UnreadCount : 0),
                    Items = group.OrderBy(g => g.Name).Select(g => new IndexViewModel.Item {
                        Id = g.Id.ToString(), Name = g.Name, Unread = unreadCount.Sum(u => u.FeedId == g.Id ? u.UnreadCount : 0)
                    })
                });

            return View(new IndexViewModel { Groups = userGroupsAndFeeds });
        }

        public ActionResult Xrds() { return View(); }
    }
}
