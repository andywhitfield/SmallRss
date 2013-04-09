using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Reflection;

namespace SmallRss.Web.App_Start
{
    public class RavenDBNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDocumentStore>()
                .ToMethod(context =>
                {
                    Trace.TraceInformation("Creating new RavenDB document store...");
                    var documentStore = new DocumentStore { Url = "http://localhost:8088", DefaultDatabase = "SmallRss" };
                    documentStore.Initialize();
                    documentStore.Conventions.JsonContractResolver = new DefaultContractResolver(true)
                    {
                        DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    };
                    Trace.TraceInformation("Successfully created RavenDB document store in the App_Data/RavenDb directory.");
                    return documentStore;
                })
                .InSingletonScope();

            Bind<IDocumentSession>()
                .ToMethod(context =>
                {
                    var session = context.Kernel.Get<IDocumentStore>().OpenSession();
                    Trace.TraceInformation("Request started, opened RavenDb session.");
                    return session;
                })
                .InRequestScope()
                .OnDeactivation((context, session) =>
                {
                    Trace.TraceInformation("Request completed, saving RavenDb session changes.");
                    session.SaveChanges();
                    session.Dispose();
                });
        }
    }
}