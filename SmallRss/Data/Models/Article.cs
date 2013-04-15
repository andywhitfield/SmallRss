using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data.Models
{
    [TableName("Article")]
    [PrimaryKey("Id")]
    public class Article
    {
        public int Id { get; set; }
        public int RssFeedId { get; set; }
        public string ArticleGuid { get; set; }
        public string Heading { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public DateTime? Published { get; set; }
    }
}
