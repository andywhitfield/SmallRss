using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallRss.Data.Models
{
    [TableName("UserArticlesRead")]
    [PrimaryKey("Id")]
    public class UserArticlesRead
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int UserFeedId { get; set; }
        public int ArticleId { get; set; }
    }
}
