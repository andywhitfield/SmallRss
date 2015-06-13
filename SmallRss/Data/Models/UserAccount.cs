using PetaPoco;
using System;
using System.Collections.Generic;

namespace SmallRss.Data.Models
{
    [TableName("UserAccount")]
    [PrimaryKey("Id")]
    public class UserAccount
    {
        public UserAccount()
        {
            AuthenticationIds = new HashSet<string>();
            ShowAllItems = false;
            ExpandedGroups = new HashSet<string>();
            SavedLayout = new Dictionary<string, string>();
            PocketAccessToken = string.Empty;
        }

        public int Id { get; set; }
        public DateTime? LastLogin { get; set; }

        [Ignore]
        public ISet<string> AuthenticationIds { get; private set; }
        [Ignore]
        public bool ShowAllItems { get; set; }
        [Ignore]
        public ISet<string> ExpandedGroups { get; private set; }
        [Ignore]
        public IDictionary<string, string> SavedLayout { get; private set; }
        [Ignore]
        public string PocketAccessToken { get; set; }
        [Ignore]
        public bool HasPocketAccessToken { get { return !string.IsNullOrWhiteSpace(PocketAccessToken); } }
    }
}
