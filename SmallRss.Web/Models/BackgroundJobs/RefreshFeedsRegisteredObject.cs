using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Hosting;

namespace SmallRss.Web.Models.BackgroundJobs
{
    public class RefreshFeedsRegisteredObject : IRegisteredObject
    {
        private readonly Timer timer;
        private readonly object lockObject = new object();
        private readonly RefreshFeeds refreshFeeds;
        private bool shuttingDown;
        private bool shutDown;

        public RefreshFeedsRegisteredObject(RefreshFeeds refreshFeeds)
        {
            HostingEnvironment.RegisterObject(this);
            this.refreshFeeds = refreshFeeds;
            timer = new Timer(state => Run(), null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(30));
            Trace.TraceInformation("Started refresh feed timer.");
        }

        public void Stop(bool immediate)
        {
            lock (lockObject)
            {
                shuttingDown = true;
                if (!shutDown)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Dispose();

                    Trace.TraceInformation("Shutdown refresh feed timer.");
                    shutDown = true;
                }
            }
            HostingEnvironment.UnregisterObject(this);
        }

        public void Run()
        {
            lock (lockObject)
            {
                if (shuttingDown)
                    return;
                refreshFeeds.Refresh();
            }
        }
    }
}