using DigiWorldLib.Agent.Humans;
using DigiWorldLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Cluster
{
    public class OccupationCluster : ClusterBase
    {
        public string Name { get; set; }
        public Dictionary<PersonAgent, OccupationPersonnelContract> Personnel { get; set; }
        public int Profit { get; set; }
        public int Expenses { get; set; }
        //products sold as well as how much they are sold for
        public Dictionary<string, ProductData> Products { get; set; }
    }
}
