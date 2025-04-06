namespace ContentAggregator.Core.Models.DTOs
{
    public class YtChannelDto
    {
        public required string ChannelSuffix { get; set; }
        public byte ActivityLevel { get; set; }
        public string? ChannelTitle { get; set; }
        public string? TitleKeywords { get; set; }
    }
}
