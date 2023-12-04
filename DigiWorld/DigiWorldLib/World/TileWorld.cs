using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.World
{
    public class BasicColor
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
    }

    public class TileWorld
    {
        public string ColormapFileLocation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileSize { get; set; }
        //public List<SubTile> SubTiles { get; set; }
        public List<string> SubtileFileLocations { get; set; }
        
    }

    public class SubTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public List<TileResource> Resources { get; set; }
    }

    public class TileResource
    {
        public string ResourceName { get; set; }
        public BasicColor ResourceRepColor { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<Vector2> ResourceLocations { get; set; }
    }
}
