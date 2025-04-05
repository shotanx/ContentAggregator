using ContentAggregator.Core.Entities;

namespace ContentAggregator.Core.Interfaces
{
    public interface IYoutubeContentRepository
    {
        Task<YoutubeContent?> GetYTContentsByIdAsync(int id);
        Task<List<YoutubeContent>> GetYTContentsWithoutEngSRT();
        Task<List<YoutubeContent>> GetYTContentsWithoutEngSummaries();
        Task<List<YoutubeContent>> GetYTContentsWithoutGeoSummaries();
        Task<List<YoutubeContent>> GetYTContentsForFBPost();
        Task AddYTContentFeature(YoutubeContentFeature contentFeature);
        Task AddYTContents(List<YoutubeContent> contents);
        Task UpdateYTContentsAsync(YoutubeContent yTContent);
        Task UpdateYTContentsRangeAsync(List<YoutubeContent> yTContents);
    }
}
