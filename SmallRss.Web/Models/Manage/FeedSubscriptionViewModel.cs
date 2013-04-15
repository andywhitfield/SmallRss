namespace SmallRss.Web.Models.Manage
{
    public class FeedSubscriptionViewModel
    {
        //private readonly Feed feed;
        //private readonly Rss rss;
        private readonly SmallRss.Data.Models.UserFeed feed;
        private readonly SmallRss.Data.Models.RssFeed rss;

        public FeedSubscriptionViewModel(SmallRss.Data.Models.UserFeed feed, SmallRss.Data.Models.RssFeed rss)
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