using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Data
{
    public class NaturalResourceData
    {
        public int ResourceQuantity { get; set; }
        public float ProcessDifficulty { get; set; }
        public List<ResourceConditionBase> ResourceConditions { get; set; }

    }
}
