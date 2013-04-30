﻿using log4net.Config;
using Ninject;
using SmallRss.Service.Api;
using SmallRss.Service.Jobs;
using Topshelf;

namespace SmallRss.Service
{
    public class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            var ninject = new NinjectConfig();

            HostFactory.Run(h =>
            {
                h.Service<RssServices>(s =>
                {
                    s.ConstructUsing(() => ninject.GetRegistry().Get<RssServices>());
                    s.WhenStarted(rs => rs.Start());
                    s.WhenStopped(rs => rs.Stop());
                });
                h.RunAsLocalSystem();
                h.SetDescription("SmallRss Service providing the feed subscription management.");
                h.SetDisplayName("SmallRss Service");
                h.SetServiceName("SmallRss.Service");
            });            
        }
    }
}
