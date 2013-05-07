using System.Collections.Generic;

namespace SmallRss.Web.Areas.Mobile.Models.User
{
    public class FeedViewModel
    {
        public int FeedId { get; set; }
        public string GroupName { get; set; }
        public IEnumerable<ArticleSummary> Articles { get; set; }
        public bool ShowingAll { get; set; }

        public class ArticleSummary
        {
            public int Id { get; set; }
            public bool Read { get; set; }
            public string Posted { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
        }
    }
}