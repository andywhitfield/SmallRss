using Ninject;
using QDFeedParser;
using QDFeedParser.Xml;
using SmallRss.Data;
using SmallRss.Parsing;

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
            kernel.Bind<RefreshFeeds>().ToSelf();

            return kernel;
        }
    }
}
