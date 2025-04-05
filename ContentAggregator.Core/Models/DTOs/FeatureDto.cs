using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentAggregator.Core.Models.DTOs
{
    public class FeatureDto
    {
        public required string FirstNameEng { get; set; }
        public required string LastNameEng { get; set; }
        public required string FirstNameGeo { get; set; }
        public required string LastNameGeo { get; set; }
    }
}
