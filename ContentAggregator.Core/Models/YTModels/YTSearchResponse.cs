using System;
using System.Collections.Generic;

namespace ContentAggregator.Core.Models.YTModels
{
    public class YTSearchResponse
    {
        public required string Kind { get; set; }
        public required string Etag { get; set; }
        public required List<SearchItem> Items { get; set; }
        public required PageInfo PageInfo { get; set; }
    }

    public class SearchItem
    {
        public required string Kind { get; set; }
        public required string Etag { get; set; }
        public required Id Id { get; set; }
        public required SearchSnippet Snippet { get; set; }
    }

    public class Id
    {
        public required string Kind { get; set; }
        public required string VideoId { get; set; }
    }

    public class SearchSnippet :YTSnippet
    {
        public DateTime PublishTime { get; set; }
        public required string ChannelId { get; set; }
        public required string ChannelTitle { get; set; }
        public required string LiveBroadcastContent { get; set; }
    }

    public class Thumbnails
    {
        public required Thumbnail Default { get; set; }
        public required Thumbnail Medium { get; set; }
        public required Thumbnail High { get; set; }
    }

    public class Thumbnail
    {
        public required string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class PageInfo
    {
        public int TotalResults { get; set; }
        public int ResultsPerPage { get; set; }
    }
}