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
            World.DistributeResource("water", 3, 5, width / 64, width / 32, height / 64, height / 32, new Data.NaturalResourceData
            {
                ProcessDifficulty = 0,
                ResourceQuantity = 1000000
            });
            World.DistributeResource("trees", 4, 6, width / 64, width / 32, height / 64, height / 32, new Data.NaturalResourceData
            {
                ProcessDifficulty = 4,
                ResourceQuantity = 1000
            });
            World.DistributeResource("fertilesoil", 4, 6, width / 64, width / 32, height / 64, height / 32, new Data.NaturalResourceData
            {
                ProcessDifficulty = 4,
                ResourceQuantity = 10000
            });
            World.DistributeResource("oil", 1, 3, width / 128, width / 64, height / 128, height / 64, new Data.NaturalResourceData
            {
                ProcessDifficulty = 20,
                ResourceQuantity = 100
            });
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
