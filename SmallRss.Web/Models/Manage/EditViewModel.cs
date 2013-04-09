using System.Collections.Generic;

namespace SmallRss.Web.Models.Manage
{
    public class EditViewModel
    {
        public FeedSubscriptionViewModel Feed { get; set; }
        public IEnumerable<string> CurrentGroups { get; set; }
    }
}