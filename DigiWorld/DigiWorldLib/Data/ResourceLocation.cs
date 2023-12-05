using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Data
{
    public class ResourceLocation
    {
        public Vector2 Location { get; set; }
        //May use bezier curves at some point to define size
        public int Width { get; set; }
        public int Height { get; set; }
        public NaturalResourceData Data {get; set;}
    }
}
