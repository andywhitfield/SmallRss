using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data.Models
{
    [TableName("RssFeed")]
    [PrimaryKey("Id")]
    public class RssFeed
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public string Link { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? LastRefreshed { get; set; }
    }
}
