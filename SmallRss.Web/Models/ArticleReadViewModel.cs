namespace SmallRss.Web.Models
{
    public class ArticleReadViewModel
    {
        public int? Feed { get; set; }
        public int? Story { get; set; }
        public bool Read { get; set; }
        public int? MaxStory { get; set; }
        public int? Offset { get; set; }
    }
}