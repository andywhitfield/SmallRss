[assembly: WebActivator.PreApplicationStartMethod(typeof(SmallRss.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(SmallRss.Web.App_Start.NinjectWebCommon), "Stop")]

namespace SmallRss.Web.App_Start
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using QDFeedParser;
    using QDFeedParser.Xml;
    using SmallRss.Data;
    using SmallRss.Parsing;
    using SmallRss.Web.ServiceApi;
    using System;
    using System.Web;
    using System.Web.Http;
    using WebApiContrib.IoC.Ninject;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);

            GlobalConfiguration.Configuration.DependencyResolver = new NinjectResolver(kernel);

            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IFeedXmlParser>().To<SmallRssFeedParser>();
            kernel.Bind<IFeedFactory>().To<HttpFeedFactory>();
            kernel.Bind<PetaPoco.Database>().ToConstructor(c => new PetaPoco.Database("SmallRssDb"));
            kernel.Bind<IDatastore>().To<PetaPocoDatastore>();
            kernel.Bind<SmallRssApi>().To<ServiceApiProxy.ServiceApi>();
        }
    }
}
