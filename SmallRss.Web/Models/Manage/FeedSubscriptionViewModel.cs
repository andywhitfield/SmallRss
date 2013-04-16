using SmallRss.Data.Models;

namespace SmallRss.Web.Models.Manage
{
    public class FeedSubscriptionViewModel
    {
        private readonly UserFeed feed;
        private readonly RssFeed rss;

        public FeedSubscriptionViewModel(UserFeed feed, RssFeed rss)
        {
            this.feed = feed;
            this.rss = rss;
        }

        public int Id { get { return feed.Id; } }
        public string Group { get { return feed.GroupName; } }
        public string Name { get { return feed.Name; } }
        public string Url { get { return rss.Uri; } }
    }
}