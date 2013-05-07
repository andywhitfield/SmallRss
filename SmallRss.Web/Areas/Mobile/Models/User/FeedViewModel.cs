using System.Collections.Generic;
using System.Linq;

namespace SmallRss.Web.Areas.Mobile.Models.User
{
    public class FeedViewModel
    {
        public int FeedId { get; set; }
        public string GroupName { get; set; }
        public IEnumerable<ArticleSummary> Articles { get; set; }
        public bool ShowingAll { get; set; }
        public int MaxArticleId { get { return Articles.Max(a => a.Id); } }

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