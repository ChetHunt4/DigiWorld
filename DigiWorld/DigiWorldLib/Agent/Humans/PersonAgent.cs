using DigiWorldLib.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Agent.Humans
{
    public class PersonAgent : AgentBase
    {
        public string Name { get; set; }

        //public ClusterBase? Occupation { get; set; }
        //This should go to the residence the agent belongs to

        public int HoursOfWorkPerDay { get; set; }
        public int HourlyRate { get; set; }
        public int MonetarySavings { get; set; }
        public PersonAgent? Employer { get; set; }

        public int PhysicalHealth { get; set; }
        public int MentalHealth { get; set; }
        //Health Issues can effect things like a person's ability to work or travel
        public List<HealthIssue> HealthIssues { get; set; }

        public Gender Gender { get; set; }
        public int Age { get; set; }

        public PersonAgent()
        {
            
        }
    }
}
