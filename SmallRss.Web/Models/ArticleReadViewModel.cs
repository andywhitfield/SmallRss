namespace SmallRss.Web.Models
{
    public class ArticleReadViewModel
    {
        public int? Feed { get; set; }
        public int? Story { get; set; }
        public bool Read { get; set; }
    }
}