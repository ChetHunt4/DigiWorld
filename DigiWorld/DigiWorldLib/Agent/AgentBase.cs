using DigiWorldLib.Cluster;
using DigiWorldLib.Pathways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Agent
{
    public abstract class AgentBase
    {
        public PathwayBase OccupiedPathway { get; set; }
        public string PathwayType { get; set; }
        public int DistancePerStep { get; set; }

        public virtual void Step()
        {

        }
    }
}
