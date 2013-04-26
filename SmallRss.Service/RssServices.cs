using log4net;
using SmallRss.Service.Api;
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
        private RefreshFeeds refreshFeeds;
        private ISmallRssApi wcfApi;
        private ServiceHost wcfHost;

        public RssServices(RefreshFeeds refreshFeeds, ISmallRssApi wcfApi)
        {
            refreshTimer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
            refreshTimer.Elapsed += (s, e) => RefreshAllFeeds();

            this.refreshFeeds = refreshFeeds;
            this.wcfApi = wcfApi;
        }

        public void Start()
        {
            refreshTimer.Start();
            StartWcfServiceHost();
        }

        public void Stop()
        {
            refreshTimer.Stop();
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
                log.InfoFormat("Done refresh, will run again in {0}", interval);
                refreshTimer.Start();
            }
        }
    }
}
