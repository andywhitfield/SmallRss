using log4net;
using SmallRss.Data.Models;
using System;

namespace SmallRss.Service.Jobs
{
    public class Log4NetPurge : IDailyJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Log4NetPurge));

        private readonly IDatastore datastore;

        public Log4NetPurge(IDatastore datastore)
        {
            this.datastore = datastore;
        }

        public void Run()
        {
            var oneMonth = DateTime.UtcNow.AddMonths(-1);
            log.InfoFormat("Removing log entries older than {0}", oneMonth);

            var removed = datastore.RemoveAll<Log>(Tuple.Create<string, object, ClauseComparsion>("Date", oneMonth, ClauseComparsion.LessThan));
            log.InfoFormat("Removed {0} log rows.", removed);
        }
    }
}
