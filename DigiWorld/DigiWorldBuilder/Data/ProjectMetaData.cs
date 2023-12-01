using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DigiWorldBuilder.Data
{
    public enum ColorChannel
    {
        R,
        G,
        B
    }

    public class ProjectMetaData
    {
        public string ColorMapFileName { get; set; }
        public ResourceMetaData FreshWaterMetaData { get; set; }
        public ResourceMetaData OceanWaterMetaData { get; set; }
        public Dictionary<string, ResourceMetaData> ResourceMetaData { get; set; }
    }

    public class ResourceMetaData
    {
        public string ResourceName { get; set; }
        public string ResourceFilename { get; set; }
        public SKColorChannel ResourceColorChannel { get; set; }
        //Color to represent this resource as in the editor
        public SKColor RepColor { get; set; }
    }

    //Data package to return includes file location
    public class GraphicData
    {
        public SKBitmap Image { get; set; }
        public string FileName { get; set; }
    }

    public class ExtendedResourceData : ResourceMetaData
    {
        public SKBitmap OriginalImage { get; set; }
        public SKBitmap ConvertedImage { get; set; }
        //If true we can preview the data
        public bool IsPublished { get; set; }
        public bool IsVisible { get; set; }
    }
}
