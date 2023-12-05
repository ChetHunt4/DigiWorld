using DigiWorldLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Cluster.Industrial
{
    public class IndustrialOccupationCluster : OccupationCluster
    {
        public Dictionary<string, NaturalResourceData> ProcessedResources { get; set; }
    }
}
