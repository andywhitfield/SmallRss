using System.ComponentModel.DataAnnotations;

namespace SmallRss.Web.Models.Manage
{
    public class AddFeedViewModel
    {
        public string GroupSel { get; set; }
        public string Group { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public string Name { get; set; }
    }
}