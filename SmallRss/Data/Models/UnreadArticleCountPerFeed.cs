using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data.Models
{
    public class UnreadArticleCountPerFeed
    {
        public int FeedId { get; set; }
        public string GroupName { get; set; }
        public int UnreadCount { get; set; }
    }
}
