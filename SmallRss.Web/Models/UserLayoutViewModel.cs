namespace SmallRss.Web.Models
{
    public class UserLayoutViewModel
    {
        public static readonly string LayoutKeySplitWest = "SplitterWestPosition";
        public static readonly string LayoutKeySplitNorth = "SplitterNorthPosition";

        public int? SplitWest { get; set; }
        public int? SplitNorth { get; set; }
    }
}