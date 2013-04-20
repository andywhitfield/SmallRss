using log4net;
using SmallRss.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class FeedStatusController : ApiController
    {
        private static ILog log = LogManager.GetLogger(typeof(FeedStatusController));

        private readonly IDatastore datastore;

        public FeedStatusController(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        // GET api/feedstatus
        public IEnumerable<object> Get()
        {
            var user = this.CurrentUser(datastore);
            return datastore
                .GetUnreadFeeds(user.Id)
                .GroupBy(f => f.GroupName)
                .Select(group =>
                    new
                    {
                        label = group.Key,
                        unread = group.Sum(g => g.UnreadCount),
                        items = group.Select(f => new { value = f.FeedId, unread = f.UnreadCount })
                    }
                );
        }

        // POST api/feedstatus
        public void Post(FeedStatusViewModel status)
        {
            log.DebugFormat("Updating user settings - show all: {0}; group: {1}; expanded: {2}", status.ShowAll, status.Group, status.Expanded);

            var user = this.CurrentUser(datastore);
            if (status.Expanded.HasValue && !string.IsNullOrEmpty(status.Group))
            {
                if (status.Expanded.Value) user.ExpandedGroups.Add(status.Group);
                else user.ExpandedGroups.Remove(status.Group);
            }
            if (status.ShowAll.HasValue)
            {
                user.ShowAllItems = status.ShowAll.Value;
            }
            datastore.UpdateAccount(user);
        }
    }
}