using SmallRss.Parsing;
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
            /*
            DateTime d;
            DateParser.TryParseRfc822DateTime("Sun, 25 Mar 2013 21:00:35 GMT", out d);
            Console.WriteLine("HERE: " + d.ToString("s"));

            DateTime.TryParseExact("Sun, 25 Mar 2013 21:00:35 +00:00", "ddd, dd MMM yyyy HH:mm:ss zzz", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.AdjustToUniversal, out d);
            Console.WriteLine("HERE: " + d.ToString("s"));
             */
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
