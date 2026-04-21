using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.ViewModels
{
    public class LayerDefaultsTests
    {
        [Fact]
        public void CreateAutoCadSeeds_ReturnsFiveLayerDefaults()
        {
            var seeds = LayerDefaults.CreateAutoCadSeeds();

            Assert.Equal(5, seeds.Count);
            Assert.Equal("0", seeds[0].Name);
            Assert.Equal("DEFPOINTS", seeds[1].Name);
            Assert.Equal("DIMS", seeds[2].Name);
            Assert.Equal("OBJECT", seeds[3].Name);
            Assert.Equal("TEXT", seeds[4].Name);

            Assert.Contains(seeds, seed => seed.LineWeight % 1d != 0d);
            Assert.True(seeds.Select(seed => seed.Color).Distinct().Count() >= 5);
        }
    }
}
