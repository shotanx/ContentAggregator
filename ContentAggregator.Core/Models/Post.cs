namespace ContentAggregator.Core.Models
{
    public class Post
    {
        public int Id { get; set; }
        public required string PageId { get; set; }
        public Uri? Url { get; set; }
        public string? CustomText { get; set; }
    }
}
