using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data.Models
{
    [TableName("UserFeed")]
    [PrimaryKey("Id")]
    public class UserFeed
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int RssFeedId { get; set; }

        public string GroupName { get; set; }
        public string Name { get; set; }
    }
}
