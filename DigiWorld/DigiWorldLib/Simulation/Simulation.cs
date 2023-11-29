using DigiWorldLib.Agent;
using DigiWorldLib.Agent.Humans;
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
        public WorldCluster World { get; set; }

        public Simulation(int width, int height)
        {
            InitializeWorld(width, height);
        }

        public void InitializeWorld(int width, int height)
        {
            World = new WorldCluster(width, height);
            World.Agents = new List<Agent.AgentBase>();
            var firstAgent = new PersonAgent();
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
