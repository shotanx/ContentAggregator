namespace ContentAggregator.Core.Entities
{
    public class Feature
    {
        public int Id { get; set; }
        public required string FirstNameEng { get; set; }
        public required string LastNameEng { get; set; }
        public required string FirstNameGeo { get; set; }
        public required string LastNameGeo { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<YoutubeContent> YoutubeContents { get; set; } = [];
        public ICollection<YoutubeContentFeature> YoutubeContentFeatures { get; set; } = [];
    }
}
