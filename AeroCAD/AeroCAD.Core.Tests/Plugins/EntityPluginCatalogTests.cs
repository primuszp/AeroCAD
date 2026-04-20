using Xunit;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class EntityPluginCatalogTests
    {
        [Fact]
        public void GetByCapability_ReturnsPluginsWithCapability()
        {
            var catalog = new EntityPluginCatalog(new IEntityPlugin[]
            {
                new LineEntityPlugin(),
                new RectangleEntityPlugin()
            });

            var trimPlugins = catalog.GetByCapability(EntityPluginCapability.TrimExtend);

            Assert.Equal(2, trimPlugins.Count);
            Assert.Contains(trimPlugins, plugin => plugin.Name == "AeroCAD.Line");
            Assert.Contains(trimPlugins, plugin => plugin.Name == "AeroCAD.Rectangle");
        }

        [Fact]
        public void Find_ReturnsPluginByName()
        {
            var catalog = new EntityPluginCatalog(new IEntityPlugin[]
            {
                new LineEntityPlugin()
            });

            var plugin = catalog.Find("AeroCAD.Line");

            Assert.NotNull(plugin);
            Assert.Equal("AeroCAD.Line", plugin.Name);
        }
    }
}
