using log4net;
using SmallRss.Web.ServiceApi;
using System;
using System.Threading.Tasks;

namespace SmallRss.Web.ServiceApiProxy
{
    public class ServiceApi : SmallRssApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServiceApi));

        private SmallRssApiClient connection;

        public ServiceApi()
        {
            connection = new SmallRssApiClient();
            connection.Open();
        }

        public void RefreshAllFeeds(int userAccountId)
        {
            Task
                .Run(() => connection.RefreshAllFeeds(userAccountId))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        log.Warn("Error calling refresh all feeds: ", t.Exception);
                    }
                });
        }

        public Task RefreshAllFeedsAsync(int userAccountId)
        {
            return connection.RefreshAllFeedsAsync(userAccountId);
        }

        public void RefreshFeed(int userAccountId, int feedId)
        {
            Task
                .Run(() => connection.RefreshFeed(userAccountId, feedId))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        log.Warn("Error calling refresh feed: ", t.Exception);
                    }
                });
        }

        public Task RefreshFeedAsync(int userAccountId, int feedId)
        {
            return connection.RefreshFeedAsync(userAccountId, feedId);
        }
    }
}