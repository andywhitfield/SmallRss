using log4net;
using System;
using System.Collections.Generic;

namespace SmallRss.Service.Jobs
{
    public class DailyJobs
    {
        private static ILog log = LogManager.GetLogger(typeof(DailyJobs));

        private readonly IEnumerable<IDailyJob> jobs;

        public DailyJobs(IEnumerable<IDailyJob> jobs)
        {
            this.jobs = jobs;
        }

        public void RunAll()
        {
            foreach (var job in jobs)
            {
                try
                {
                    log.DebugFormat("Running daily job: {0}", job.GetType().FullName);
                    job.Run();
                }
                catch (Exception ex)
                {
                    log.Error("Error running job " + job.GetType().FullName + ": ", ex);
                }
            }
        }
    }
}
