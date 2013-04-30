using PetaPoco;
using System;

namespace SmallRss.Data.Models
{
    [TableName("Log")]
    [PrimaryKey("Id")]
    public class Log
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public DateTime? Date { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
    }
}
