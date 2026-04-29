using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
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

        [Fact]
        public void TypedStrategies_HandleOnlyTheirEntityType()
        {
            var entity = new TestEntity();
            var other = new OtherEntity();
            var renderStrategy = new TypedRenderStrategy();
            var boundsStrategy = new TypedBoundsStrategy();
            var previewStrategy = new TypedGripPreviewStrategy();

            Assert.True(renderStrategy.CanHandle(entity));
            Assert.False(renderStrategy.CanHandle(other));
            Assert.Equal(new Rect(1, 2, 3, 4), boundsStrategy.GetBounds(entity));
            Assert.Same(GripPreview.Empty, previewStrategy.CreatePreview(entity, 0, new Point(5, 6)));
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

        private sealed class OtherEntity : Entity
        {
            public override int GripCount => 0;
            public override Point GetGripPoint(int index) => default;
            public override void MoveGrip(int index, Point newPosition) { }
            public override Entity Clone() => new OtherEntity();
            public override Entity Duplicate() => new OtherEntity();
            public override void RestoreState(Entity sourceState) { }
            public override void Translate(Vector delta) { }
        }

        private sealed class TypedRenderStrategy : EntityRenderStrategy<TestEntity>
        {
            protected override void Render(TestEntity entity, DrawingContext drawingContext, EntityRenderContext context) { }
        }

        private sealed class TypedBoundsStrategy : EntityBoundsStrategy<TestEntity>
        {
            protected override Rect GetBounds(TestEntity entity) => new Rect(1, 2, 3, 4);
        }

        private sealed class TypedGripPreviewStrategy : GripPreviewStrategy<TestEntity>
        {
            protected override GripPreview CreatePreview(TestEntity entity, int gripIndex, Point newPosition) => null;
        }
    }
}
