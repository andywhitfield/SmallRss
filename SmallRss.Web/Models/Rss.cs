using System;

namespace SmallRss.Web.Models
{
    public class Rss
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastRefresh { get; set; }
    }
}