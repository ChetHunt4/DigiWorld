using DigiWorldLib.Agent.Humans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Data
{
    public class OccupationPersonnelContract
    {
        public PersonAgent Personnel { get; set; }
        public int HourlyPay { get; set; }
        public int ExpectedHoursOfWorkPerDay { get; set; }

    }
}
