using QDFeedParser;
using System;
using System.Web.Http;

namespace SmallRss.Web.Controllers
{
    [Authorize]
    public class RssController : ApiController
    {
        private IFeedFactory feedFactory;

        public RssController(IFeedFactory feedFactory)
        {
            this.feedFactory = feedFactory;
        }

        // GET api/rss
        public object Get(string url)
        {
            try
            {
                var feed = feedFactory.CreateFeed(new Uri(url));
                return new { Title = feed.Title };
            }
            catch (Exception ex)
            {
                return new { Error = ex.ToString() };
            }
        }
    }
}