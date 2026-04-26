using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class ExternalEntityIntegrationTests
    {
        [Fact]
        public void TrimExtendService_UsesPluginDescriptorToAcceptExternalBoundaryEntity()
        {
            var descriptor = new EntityPluginDescriptor(
                "Test.ExternalEntity",
                new ExternalRenderStrategy(),
                new ExternalBoundsStrategy(),
                trimExtendStrategy: new ExternalTrimExtendStrategy());
            var service = new EntityTrimExtendService(
                new[] { descriptor.TrimExtendStrategy },
                new[] { descriptor });

            Assert.True(service.CanUseAsBoundary(new ExternalEntity()));
        }

        [Fact]
        public void BuiltInOffsetCommand_DoesNotRestrictSelectionToBuiltInEntityTypes()
        {
            var module = new BuiltInModifyModule();
            var offset = module.InteractiveCommands.Single(command => command.CommandName == "OFFSET");
            var definition = offset.CreateCommandDefinition();

            Assert.Empty(definition.Policy.SupportedSelectionEntityTypes);
        }

        private sealed class ExternalEntity : Entity
        {
            public override int GripCount => 0;
            public override Point GetGripPoint(int index) => default;
            public override void MoveGrip(int index, Point newPosition) { }
            public override Entity Clone() => new ExternalEntity();
            public override Entity Duplicate() => new ExternalEntity();
            public override void RestoreState(Entity sourceState) { }
            public override void Translate(Vector delta) { }
        }

        private sealed class ExternalRenderStrategy : IEntityRenderStrategy
        {
            public bool CanHandle(Entity entity) => entity is ExternalEntity;
            public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context) { }
        }

        private sealed class ExternalBoundsStrategy : IEntityBoundsStrategy
        {
            public bool CanHandle(Entity entity) => entity is ExternalEntity;
            public Rect GetBounds(Entity entity) => new Rect(0, 0, 1, 1);
        }

        private sealed class ExternalTrimExtendStrategy : IEntityTrimExtendStrategy
        {
            public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target) => false;
            public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target) => false;
            public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint) => new Entity[0];
            public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint) => new Entity[0];
        }
    }
}
