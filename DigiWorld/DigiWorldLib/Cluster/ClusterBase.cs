using DigiWorldLib.Agent;
using DigiWorldLib.Pathways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Cluster
{
    public class ClusterBase
    {
        public ClusterBase? Parent { get; set; }
        public PathwayBase? ParentRoadway { get; set; }
        public List<ClusterBase>? Children { get; set; }
        public List<AgentBase> Agents { get; set; }
        public int LocalTime { get; set; }

        //Determines the distance based on layer of connected pathways
        public int DistanceMultiplier { get; set; }

        public void Step()
        {

        }

        public AgentBase SampleChildAgents()
        {
            throw new NotImplementedException();
        }

    }
}
