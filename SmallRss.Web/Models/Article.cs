using System;

namespace SmallRss.Web.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string ArticleGuid { get; set; }
        public string Heading { get; set; }
        public string ArticleBody { get; set; }
        public string ArticleUrl { get; set; }
        public DateTime Date { get; set; }
        public int RssId { get; set; }
    }
}