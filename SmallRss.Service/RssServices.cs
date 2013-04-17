using System;
using System.Diagnostics;
using System.Timers;

namespace SmallRss.Service
{
    public class RssServices
    {
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
                Trace.TraceError("Error refreshing feeds: {0}", ex);
            }
            finally
            {
                Trace.TraceInformation("Done refresh, will run again in " + interval);
                refreshTimer.Start();
            }
        }
    }
}
