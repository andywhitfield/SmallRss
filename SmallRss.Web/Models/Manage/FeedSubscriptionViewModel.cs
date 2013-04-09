namespace SmallRss.Web.Models.Manage
{
    public class FeedSubscriptionViewModel
    {
        private readonly Feed feed;
        private readonly Rss rss;

        public FeedSubscriptionViewModel(Feed feed, Rss rss)
        {
            this.feed = feed;
            this.rss = rss;
        }

        public int Id { get { return feed.Id; } }
        public string Group { get { return feed.GroupName; } }
        public string Name { get { return feed.Name; } }
        public string Url { get { return rss.Url; } }
    }
}