using DigiWorldLib.Agent.Humans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Agent.Transportation
{
    public class VehicleAgent : AgentBase
    {
        public PersonAgent Driver { get; set; }
        public List<PersonAgent> Occupants { get; set; }
        public int MaxOccupants { get; set; }

        public override void Step()
        {
            base.Step();
        }
    }
}
