using System.Collections.Generic;

namespace SmallRss.Web.Models
{
    public class Feed
    {
        public Feed()
        {
            ArticlesRead = new HashSet<int>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public int RssId { get; set; }
        public ISet<int> ArticlesRead { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}-{3}", Id, Name, GroupName);
        }
    }
}