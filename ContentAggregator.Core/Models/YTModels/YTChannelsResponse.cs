namespace ContentAggregator.Core.Models.YTModels
{
    public class YTChannelsResponse
    {
        public required string Kind { get; set; }
        public required string Etag { get; set; }
        public required List<ChannelItem> Items { get; set; }
        public required PageInfo PageInfo { get; set; }
    }

    public class ChannelItem
    {
        public required string Kind { get; set; }
        public required string Etag { get; set; }
        public required string Id { get; set; }
        public ChannelSnippet? Snippet { get; set; } // optional part={parameter}
    }

    public class ChannelSnippet : YTSnippet
    {
        public required string CustomUrl { get; set; }
    }
}
