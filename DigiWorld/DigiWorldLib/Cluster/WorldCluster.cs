using DigiWorldLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiWorldLib.Cluster
{
    public class WorldCluster : ClusterBase
    {
        public Dictionary<string, List<ResourceLocation>> Resources { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public WorldCluster(int width, int height) {
            Width = width;
            Height = height;
        }

        public void DistributeResource(string resourceType, int minResources, int maxResources, int minWidth, int maxWidth, int minHeight, int maxHeight, NaturalResourceData data)
        {
            Random rand = new Random();
            var amount = rand.Next(minResources, maxResources);
            List<ResourceLocation> resources = new List<ResourceLocation>();
            for (int i = 0; i < amount; i++)
            {
                var x = rand.Next(0, Width);
                var y = rand.Next(0, Height);
                var width = rand.Next(minWidth, maxWidth);
                var height = rand.Next(minHeight, maxHeight);
                var newResource = new ResourceLocation
                {
                    Location = new System.Numerics.Vector2(x, y),
                    Width = width,
                    Height = height,
                    Data = data
                };
                resources.Add(newResource);
            }
            if (Resources == null)
            {
                Resources = new Dictionary<string, List<ResourceLocation>>();
            }
            if (Resources.ContainsKey(resourceType))
            {
                Resources[resourceType] = resources;
            }
            else
            {
                Resources.Add(resourceType, resources);
            }
        }
    }
}
