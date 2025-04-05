namespace ContentAggregator.Core.Entities
{
    public class YoutubeContentFeature
    {
        public int YoutubeContentId { get; set; }
        public int FeatureId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
