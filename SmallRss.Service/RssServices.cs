using log4net;
using SmallRss.Service.Api;
using SmallRss.Service.Jobs;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Timers;

namespace SmallRss.Service
{
    public class RssServices
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RssServices));

        private Timer refreshTimer;
        private Timer dailyTasksTimer;
        private RefreshFeeds refreshFeeds;
        private DailyJobs dailyJobs;
        private ISmallRssApi wcfApi;
        private ServiceHost wcfHost;

        public RssServices(RefreshFeeds refreshFeeds, DailyJobs dailyJobs, ISmallRssApi wcfApi)
        {
            refreshTimer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
            refreshTimer.Elapsed += (s, e) => RefreshAllFeeds();
            log.DebugFormat("The refresh feed tasks will run in {0}ms", refreshTimer.Interval);

            dailyTasksTimer = new Timer(TimeUntil(TimeSpan.FromHours(2)));
            dailyTasksTimer.Elapsed += (s, e) => RunDailyTasks();
            log.DebugFormat("The daily tasks will run in {0}ms", dailyTasksTimer.Interval);

            this.refreshFeeds = refreshFeeds;
            this.dailyJobs = dailyJobs;
            this.wcfApi = wcfApi;
        }

        public void Start()
        {
            refreshTimer.Start();
            dailyTasksTimer.Start();
            StartWcfServiceHost();
        }

        public void Stop()
        {
            refreshTimer.Stop();
            dailyTasksTimer.Stop();
            wcfHost.Close();
        }

        private void StartWcfServiceHost()
        {
            wcfHost = new ServiceHost(wcfApi, new Uri(ConfigurationManager.AppSettings["SmallRss.Service.Uri"]));
            wcfHost.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true, MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 } });
            wcfHost.Open();
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
                log.InfoFormat("Done refresh, will run again in {0}ms", refreshTimer.Interval);
                refreshTimer.Start();
            }
        }

        private void RunDailyTasks()
        {
            dailyTasksTimer.Stop();
            try
            {
                dailyJobs.RunAll();
            }
            catch (Exception ex)
            {
                log.Error("Error running daily tasks: ", ex);
            }
            finally
            {
                dailyTasksTimer.Interval = TimeUntil(TimeSpan.FromHours(2));
                log.InfoFormat("Done running all daily tasks, will run again in {0}ms", dailyTasksTimer.Interval);
                dailyTasksTimer.Start();
            }
        }

        // returns the number of milliseconds until the time specified by TimeSpan,
        // for example, if TimeSpan is "2 hours and 30 minutes", that means run
        // when the time is "02:30 AM".
        private double TimeUntil(TimeSpan time)
        {
            var now = DateTime.UtcNow;
            var atTime = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, 0);
            if (atTime <= now)
                atTime = atTime.AddDays(1);
            return (atTime - now).TotalMilliseconds;
        }
    }
}
