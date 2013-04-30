using Ninject;
using QDFeedParser;
using QDFeedParser.Xml;
using SmallRss.Data;
using SmallRss.Parsing;
using SmallRss.Service.Api;
using SmallRss.Service.Jobs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmallRss.Service
{
    public class NinjectConfig
    {
        private IKernel kernel;

        public NinjectConfig()
        {
        }

        public IKernel GetRegistry()
        {
            if (kernel == null)
            {
                kernel = CreateKernel();
            }

            return kernel;
        }

        private IKernel CreateKernel()
        {
            var kernel = new StandardKernel();

            kernel.Bind<IFeedXmlParser>().To<SmallRssFeedParser>();
            kernel.Bind<IFeedFactory>().To<HttpFeedFactory>();
            kernel.Bind<PetaPoco.Database>().ToConstructor(c => new PetaPoco.Database("SmallRssDb"));
            kernel.Bind<IDatastore>().To<PetaPocoDatastore>();
            kernel.Bind<RefreshFeeds>().ToSelf().InSingletonScope();
            kernel.Bind<ISmallRssApi>().To<SmallRssApi>();

            // auto register all IDailyJob implementations...
            foreach (var dailyJobType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && typeof(IDailyJob).IsAssignableFrom(t)))
                kernel.Bind<IDailyJob>().To(dailyJobType);

            kernel.Bind<DailyJobs>().ToSelf();

            return kernel;
        }
    }
}
