using System.ComponentModel.DataAnnotations;

namespace SmallRss.Web.Models.Manage
{
    public class SaveFeedViewModel : AddFeedViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}