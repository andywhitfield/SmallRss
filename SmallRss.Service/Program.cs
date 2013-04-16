﻿using Ninject;
using Topshelf;

namespace SmallRss.Service
{
    public class Program
    {
        static void Main(string[] args)
        {
            var ninject = new NinjectConfig();

            HostFactory.Run(h =>
            {
                h.Service<RssServices>(s =>
                {
                    s.ConstructUsing(() => new RssServices(ninject.GetRegistry().Get<RefreshFeeds>()));
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
