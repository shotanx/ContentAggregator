namespace ContentAggregator.Core.Models.YTModels
{
    public class YTVideosResponse
    {
        public required string Kind { get; set; }
        public required string Etag { get; set; }
        public required List<VideoItem> Items { get; set; }
        public required PageInfo PageInfo { get; set; }
    }

    public class VideoItem
    {
        public required string Kind { get; set; }
        public required string Etag { get; set; }
        public required string Id { get; set; }
        public ContentDetails? ContentDetails { get; set; } // optional part={parameter}
        public VideoSnippet? Snippet { get; set; } // optional part={parameter}
    }

    public class ContentDetails
    {
        public required string Duration { get; set; }
        public required string Dimension { get; set; }
        public required string Definition { get; set; }
        public required string Caption { get; set; }
        public required bool LicensedContent { get; set; }
        public required string Projection { get; set; }
    }

    public class VideoSnippet : YTSnippet
    {
        public int CategoryId { get; set; }
        public required string defaultAudioLanguage { get; set; }
        public required string ChannelId { get; set; }
        public required string ChannelTitle { get; set; }
        public required string LiveBroadcastContent { get; set; }
    }
}