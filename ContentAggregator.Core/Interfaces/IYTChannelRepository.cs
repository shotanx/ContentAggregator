﻿using ContentAggregator.Core.Entities;
using ContentAggregator.Core.Models.YTModels;

namespace ContentAggregator.Core.Interfaces
{
    public interface IYTChannelRepository
    {
        Task<YTChannel?> GetChannelByIdAsync(string id, CancellationToken cancellationToken);
        Task<IEnumerable<YTChannel>> GetAllChannelsAsync(CancellationToken cancellationToken);
        Task AddChannelAsync(YTChannel channel, CancellationToken cancellationToken);
        Task<bool> UpdateChannelAsync(YTChannel channel, CancellationToken cancellationToken);
        Task<bool> DeleteChannelAsync(string id, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
