using log4net;
using QDFeedParser;
using System;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class RssController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RssController));

        private IFeedFactory feedFactory;

        public RssController(IFeedFactory feedFactory)
        {
            this.feedFactory = feedFactory;
        }

        // GET api/rss
        public object Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new { Error = "No URL specified. Please enter an RSS or Atom feed URL and try again." };

            try
            {
                var feed = feedFactory.CreateFeed(new Uri(url));
                return new { Title = feed.Title };
            }
            catch (Exception ex)
            {
                log.Warn("Could not create feed for URL: " + url, ex);
                return new { Error = "Could not load feed, please check the URL and try again." };
            }
        }
    }
}