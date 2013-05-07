namespace SmallRss.Web.Areas.Mobile.Models.User
{
    public class ArticleViewModel
    {
        public int ArticleId { get; set; }
        public int FeedId { get; set; }
        public string FeedName { get; set; }
        public string GroupName { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
    }
}