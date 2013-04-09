using System.Collections.Generic;

namespace SmallRss.Web.Models
{
    public class UserAccount
    {
        public UserAccount()
        {
            AuthencationIds = new List<string>();
            Expanded = new HashSet<string>();
            Feeds = new List<int>();
        }

        public int Id { get; set; }
        public ICollection<string> AuthencationIds { get; set; }
        public ISet<string> Expanded { get; set; }
        public ICollection<int> Feeds { get; set; }
        public bool ShowAllItems { get; set; }
    }
}