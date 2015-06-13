using SmallRss.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmallRss.Web.Models.Manage
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            Feeds = new List<FeedSubscriptionViewModel>();
        }

        public UserAccount UserAccount { get; set; }
        public string Error { get; set; }
        public IEnumerable<FeedSubscriptionViewModel> Feeds { get; set; }
        public IEnumerable<string> CurrentGroups { get { return Feeds.Select(f => f.Group).Distinct().OrderBy(g => g); } }
    }
}