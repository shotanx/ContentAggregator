namespace ContentAggregator.Core.Models.YTModels
{
    public class YTSnippet
    {
        public DateTimeOffset PublishedAt { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required Thumbnails Thumbnails { get; set; }
    }
}
