using DigiWorldLib.Cluster;
using DigiWorldLib.Pathways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Agent
{
    public class AgentBase
    {
        //public string Name { get; set; }
        //public ClusterBase OwningCluster { get; set; }
        //public ClusterBase? CurrentCluster { get; set; }
        public PathwayBase OccupiedPathway { get; set; }
        public string PathwayType { get; set; }

        //public ClusterBase? Occupation { get; set; }
        //public int Income { get; set; }
        //public int Savings { get; set; }

        //public int Health { get; set; }
        //public int Fitness { get; set; }

        //public bool Male { get; set; }
        //public int Age { get; set; }

        //public int Satisfaction { get; set; }

        //public AgentBase()
        //{
        //    Male = Faker.BooleanFaker.Boolean();
        //    if (Male)
        //    {
        //        Name = Faker.NameFaker.MaleName();
        //    }
        //    else
        //    {
        //        Name = Faker.NameFaker.FemaleName();
        //    }
        //    var birthdate = Faker.DateTimeFaker.BirthDay();
        //    Age = DateTime.Now.Year - birthdate.Year;
        //}

        public void Step()
        {

        }
    }
}
