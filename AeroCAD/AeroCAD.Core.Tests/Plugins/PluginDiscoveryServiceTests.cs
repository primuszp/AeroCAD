using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class PluginDiscoveryServiceTests
    {
        [Fact]
        public void Discover_ReturnsModulesAndPluginsFromAssembly()
        {
            var service = new PluginDiscoveryService();
            var result = service.Discover(new[] { typeof(TestDiscoveryModule).GetTypeInfo().Assembly });

            Assert.Contains(result.Modules, module => module.Name == "Test.Discovery.Module");
            Assert.Contains(result.Plugins, plugin => plugin.Descriptor.Name == "Test.Discovery.Plugin");
        }

        [Fact]
        public void Discover_ReportsPluginTypesWithoutParameterlessConstructor()
        {
            var service = new PluginDiscoveryService();
            var result = service.Discover(new[] { typeof(PluginWithoutDefaultConstructor).GetTypeInfo().Assembly });

            Assert.Contains(result.Issues, issue => issue.Source.Contains(nameof(PluginWithoutDefaultConstructor)));
        }

        public sealed class TestDiscoveryModule : CadModuleBase
        {
            public override string Name => "Test.Discovery.Module";

            public override IEnumerable<IEntityPlugin> Plugins
            {
                get { yield break; }
            }
        }

        public sealed class TestDiscoveryPlugin : IEntityPlugin
        {
            public EntityPluginDescriptor Descriptor { get; } = new EntityPluginDescriptor(
                "Test.Discovery.Plugin",
                new NoOpRenderStrategy(),
                new NoOpBoundsStrategy());
        }

        public sealed class PluginWithoutDefaultConstructor : IEntityPlugin
        {
            public PluginWithoutDefaultConstructor(string dependency)
            {
            }

            public EntityPluginDescriptor Descriptor { get; } = new EntityPluginDescriptor(
                "Test.Bad.Plugin",
                new NoOpRenderStrategy(),
                new NoOpBoundsStrategy());
        }

        private sealed class NoOpRenderStrategy : IEntityRenderStrategy
        {
            public bool CanHandle(Entity entity) => false;

            public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
            {
            }
        }

        private sealed class NoOpBoundsStrategy : IEntityBoundsStrategy
        {
            public bool CanHandle(Entity entity) => false;

            public Rect GetBounds(Entity entity) => Rect.Empty;
        }
    }
}
