using Xunit;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class CadModuleCatalogTests
    {
        [Fact]
        public void Modules_ReturnRegisteredModules()
        {
            var catalog = new CadModuleCatalog(new ICadModule[]
            {
                new BuiltInGeometryModule(),
                new BuiltInModifyModule()
            });

            Assert.Equal(2, catalog.Modules.Count);
        }

        [Fact]
        public void Find_ReturnsModuleByName()
        {
            var catalog = new CadModuleCatalog(new ICadModule[]
            {
                new BuiltInGeometryModule(),
                new BuiltInModifyModule()
            });

            var module = catalog.Find("AeroCAD.BuiltInModify");

            Assert.NotNull(module);
            Assert.Equal("AeroCAD.BuiltInModify", module.Name);
        }
    }
}
