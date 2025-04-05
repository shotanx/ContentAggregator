using ContentAggregator.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentAggregator.Core.Interfaces
{
    public interface IFeatureRepository
    {
        Task<Feature?> GetFeatureByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<Feature>> GetAllFeaturesAsync(CancellationToken cancellationToken);
        Task AddFeatureAsync(Feature feature, CancellationToken cancellationToken);
        Task<bool> UpdateFeatureAsync(Feature feature, CancellationToken cancellationToken);
        Task<bool> DeleteFeatureAsync(int id, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
