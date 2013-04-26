using log4net;
using SmallRss.Data.Models;
using System.Linq;
using System.ServiceModel;

namespace SmallRss.Service.Api
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SmallRssApi : ISmallRssApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SmallRssApi));

        private readonly RefreshFeeds refreshFeeds;
        private readonly IDatastore datastore;

        public SmallRssApi(IDatastore datastore, RefreshFeeds refreshFeeds)
        {
            this.datastore = datastore;
            this.refreshFeeds = refreshFeeds;
        }

        public void RefreshAllFeeds(int userAccountId)
        {
            log.DebugFormat("Force refresh all feeds for user {0}...", userAccountId);
            var userFeeds = datastore.LoadAll<UserFeed>("UserAccountId", userAccountId).ToList();
            foreach (var userFeed in userFeeds)
                RefreshFeed(userAccountId, userFeed.RssFeedId);
        }

        public void RefreshFeed(int userAccountId, int feedId)
        {
            log.DebugFormat("Force refresh feed {0} for user {1}...", feedId, userAccountId);
            refreshFeeds.Refresh(userAccountId, feedId);
        }
    }
}
