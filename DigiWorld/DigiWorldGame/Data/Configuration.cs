using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldGame.Data
{
    public class Configuration
    {
        public string TileName { get; set; }
        public VideoSettings VideoSettings { get; set; }
    }

    public class VideoSettings
    {
        public bool IsFullscreen { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
