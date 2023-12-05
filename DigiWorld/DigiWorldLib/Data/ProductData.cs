using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Data
{
    public class ProductData
    {
        public int Cost { get; set; }
        public int Quantity { get; set; }
        public List<string> ProductTypes { get; set; }
    }
}
