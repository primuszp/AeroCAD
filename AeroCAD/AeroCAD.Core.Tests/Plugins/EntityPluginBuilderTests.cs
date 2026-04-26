using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class EntityPluginBuilderTests
    {
        [Fact]
        public void BuildPlugin_IncludesStrategiesAndInteractiveCommands()
        {
            var command = InteractiveCommandRegistrationBuilder
                .Create("TESTENTITY")
                .WithAliases("TE")
                .WithDescription("Create test entity.")
                .Build();

            var plugin = EntityPluginBuilder
                .Create("Test.Entity")
                .WithRenderStrategy(new NoOpRenderStrategy())
                .WithBoundsStrategy(new NoOpBoundsStrategy())
                .WithInteractiveCommand(command)
                .BuildPlugin();

            Assert.Equal("Test.Entity", plugin.Descriptor.Name);
            Assert.NotNull(plugin.Descriptor.RenderStrategy);
            Assert.NotNull(plugin.Descriptor.BoundsStrategy);
            Assert.Single(plugin.Descriptor.InteractiveCommands);
            Assert.True(plugin.Descriptor.Capabilities.HasFlag(EntityPluginCapability.InteractiveCommand));
            Assert.False(plugin.Descriptor.Capabilities.HasFlag(EntityPluginCapability.Tool));
        }

        private sealed class NoOpRenderStrategy : IEntityRenderStrategy
        {
            public bool CanHandle(Entity entity) => entity is TestEntity;
            public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context) { }
        }

        private sealed class NoOpBoundsStrategy : IEntityBoundsStrategy
        {
            public bool CanHandle(Entity entity) => entity is TestEntity;
            public Rect GetBounds(Entity entity) => Rect.Empty;
        }

        private sealed class TestEntity : Entity
        {
            public override int GripCount => 0;
            public override Point GetGripPoint(int index) => default;
            public override void MoveGrip(int index, Point newPosition) { }
            public override Entity Clone() => new TestEntity();
            public override Entity Duplicate() => new TestEntity();
            public override void RestoreState(Entity sourceState) { }
            public override void Translate(Vector delta) { }
        }
    }
}
