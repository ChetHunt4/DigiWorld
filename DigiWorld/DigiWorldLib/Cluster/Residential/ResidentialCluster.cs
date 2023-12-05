using DigiWorldLib.Agent.Humans;
using DigiWorldLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Cluster.Residential
{
    public class ResidentialCluster : ClusterBase
    {
        public List<PersonAgent> Residents { get; set; }
        public int Savings { get; set; }
        public int Debt { get; set; }
        public Dictionary<string, ProductData> HouseholdProducts { get; set; }
    }
}
