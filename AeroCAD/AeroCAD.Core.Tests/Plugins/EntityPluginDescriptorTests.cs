using Primusz.AeroCAD.Core.Plugins;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class EntityPluginDescriptorTests
    {
        [Fact]
        public void LinePlugin_ExposesExpectedCapabilities()
        {
            var plugin = new LineEntityPlugin();

            Assert.Equal(
                EntityPluginCapability.Render |
                EntityPluginCapability.Bounds |
                EntityPluginCapability.GripPreview |
                EntityPluginCapability.SelectionMovePreview |
                EntityPluginCapability.TransientPreview |
                EntityPluginCapability.Offset |
                EntityPluginCapability.TrimExtend |
                EntityPluginCapability.InteractiveCommand,
                plugin.Descriptor.Capabilities);
        }

        [Fact]
        public void RectanglePlugin_ExposesExpectedCapabilities()
        {
            var plugin = new RectangleEntityPlugin();

            Assert.True(plugin.Descriptor.Capabilities.HasFlag(EntityPluginCapability.TrimExtend));
            Assert.True(plugin.Descriptor.Capabilities.HasFlag(EntityPluginCapability.InteractiveCommand));
        }
    }
}
