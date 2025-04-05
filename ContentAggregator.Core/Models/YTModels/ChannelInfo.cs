using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentAggregator.Core.Models.YTModels
{
    public class ChannelInfo
    {
        public int Id { get; set; }
        public required string ChannelId { get; set; }
    }
}
