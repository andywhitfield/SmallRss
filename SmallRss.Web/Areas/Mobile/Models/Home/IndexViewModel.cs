using System.Collections.Generic;

namespace SmallRss.Web.Areas.Mobile.Models.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Item> Groups { get; set; }

        public class Item
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool Open { get; set; }
            public int Unread { get; set; }
            public IEnumerable<Item> Items { get; set; }
        }
    }
}