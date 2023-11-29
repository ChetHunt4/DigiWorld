using DigiWorldLib.Agent;
using DigiWorldLib.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Simulation
{
    public class Simulation
    {
        public ClusterBase World { get; set; }

        public Simulation()
        {
            InitializeWorld();
        }

        public void InitializeWorld()
        {
            World = new ClusterBase();
            World.Agents = new List<Agent.AgentBase>();
            var firstAgent = new AgentBase();
            World.Agents.Add(firstAgent);
        }

        public void Step()
        {
            if (World != null)
            {
                World.Step();
            }
        }
    }
}
