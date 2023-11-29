using DigiWorldLib.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Pathways
{
    public class PathwayBase
    {
        public List<PathwayBase> NextPathways { get; set; }
        public List<PathwayBase> PrevPathways { get; set; }
        public List<ClusterBase> ConnectedClusters { get; set; }
        public int Length { get; set; }
        public List<string> PathTypes { get; set; }
    }
}
