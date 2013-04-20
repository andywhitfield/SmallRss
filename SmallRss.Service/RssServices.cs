using log4net;
using System;
using System.Diagnostics;
using System.Timers;

namespace SmallRss.Service
{
    public class RssServices
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RssServices));

        private Timer refreshTimer;
        private RefreshFeeds refreshFeeds;

        public RssServices(RefreshFeeds refreshFeeds)
        {
            refreshTimer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
            refreshTimer.Elapsed += (s, e) => RefreshAllFeeds();

            this.refreshFeeds = refreshFeeds;
        }

        public void Start()
        {
            refreshTimer.Start();
        }

        public void Stop()
        {
            refreshTimer.Stop();
        }

        private void RefreshAllFeeds()
        {
            var interval = TimeSpan.FromMinutes(45);
            refreshTimer.Stop();
            try
            {
                refreshTimer.Interval = interval.TotalMilliseconds;
                refreshFeeds.Refresh();
            }
            catch (Exception ex)
            {
                log.Error("Error refreshing feeds: ", ex);
            }
            finally
            {
                log.InfoFormat("Done refresh, will run again in {0}", interval);
                refreshTimer.Start();
            }
        }
    }
}
