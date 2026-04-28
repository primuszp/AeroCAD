using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class ExternalEntityContractTests
    {
        [Fact]
        public void GripPreviewService_CreatesFallbackPreviewForExternalEntityWithoutStrategy()
        {
            var service = new GripPreviewService(Array.Empty<IGripPreviewStrategy>());
            var entity = new ExternalGripEntity(new Point(1, 2));

            var preview = service.CreatePreview(entity, 0, new Point(4, 6));

            Assert.True(preview.HasContent);
            Assert.Single(preview.Strokes);
            Assert.Equal(0, entity.MoveGripCallCount);
        }

        [Fact]
        public void SelectionMovePreviewService_CreatesFallbackPreviewForExternalEntityWithoutStrategy()
        {
            var service = new SelectionMovePreviewService(Array.Empty<ISelectionMovePreviewStrategy>());
            var entity = new ExternalGripEntity(new Point(1, 2));

            var preview = service.CreatePreview(new[] { entity }, new Vector(3, 4));

            Assert.True(preview.HasContent);
            Assert.Single(preview.Strokes);
            Assert.Equal(new Point(1, 2), entity.Point);
            Assert.Equal(0, entity.TranslateCallCount);
        }

        [Fact]
        public void ModifyEntityCommand_UndoRedoRestoresExternalEntityStateWithoutChangingIdentity()
        {
            var entity = new ExternalGripEntity(new Point(1, 2));
            var originalId = entity.Id;
            var before = entity.Clone();
            var after = entity.Clone();
            after.Translate(new Vector(5, 7));
            var command = new ModifyEntityCommand(entity, before, after);

            command.Execute();
            Assert.Equal(new Point(6, 9), entity.Point);
            Assert.Equal(originalId, entity.Id);

            command.Undo();
            Assert.Equal(new Point(1, 2), entity.Point);
            Assert.Equal(originalId, entity.Id);

            command.Redo();
            Assert.Equal(new Point(6, 9), entity.Point);
            Assert.Equal(originalId, entity.Id);
        }

        [Fact]
        public void ExternalEntityClonePreservesIdentityAndDuplicateCreatesNewIdentity()
        {
            var entity = new ExternalGripEntity(new Point(1, 2));

            var clone = entity.Clone();
            var duplicate = entity.Duplicate();

            Assert.Equal(entity.Id, clone.Id);
            Assert.NotEqual(entity.Id, duplicate.Id);
        }

        [Fact]
        public void TrimExtendService_AcceptsBoundaryGeometryWithoutPluginDescriptor()
        {
            var service = new EntityTrimExtendService(Array.Empty<IEntityTrimExtendStrategy>());

            Assert.True(service.CanUseAsBoundary(new ExternalBoundaryEntity()));
        }

        private sealed class ExternalGripEntity : Entity
        {
            private Point point;

            public ExternalGripEntity(Point point)
            {
                this.point = point;
            }

            public Point Point => point;

            public int MoveGripCallCount { get; private set; }

            public int TranslateCallCount { get; private set; }

            public override int GripCount => 1;

            public override Point GetGripPoint(int index)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return point;
            }

            public override void MoveGrip(int index, Point newPosition)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                point = newPosition;
                MoveGripCallCount++;
                InvalidateGeometry();
            }

            public override Entity Clone()
            {
                var clone = new ExternalGripEntity(point) { Thickness = Thickness };
                CopyIdentityTo(clone);
                return clone;
            }

            public override Entity Duplicate()
            {
                return new ExternalGripEntity(point) { Thickness = Thickness };
            }

            public override void RestoreState(Entity sourceState)
            {
                var source = sourceState as ExternalGripEntity;
                if (source == null)
                    return;

                point = source.point;
                RestoreBaseFrom(source);
                InvalidateGeometry();
            }

            public override void Translate(Vector delta)
            {
                point += delta;
                TranslateCallCount++;
                InvalidateGeometry();
            }
        }

        private sealed class ExternalBoundaryEntity : Entity, ITrimExtendBoundaryGeometry
        {
            public override int GripCount => 0;
            public override Point GetGripPoint(int index) => throw new ArgumentOutOfRangeException(nameof(index));
            public override void MoveGrip(int index, Point newPosition) => throw new ArgumentOutOfRangeException(nameof(index));
            public override Entity Clone() => new ExternalBoundaryEntity();
            public override Entity Duplicate() => new ExternalBoundaryEntity();
            public override void RestoreState(Entity sourceState) { }
            public override void Translate(Vector delta) { }

            public IReadOnlyList<LineIntersectionPoint> GetLineIntersections(Line target, bool restrictTargetToSegment)
            {
                return Array.Empty<LineIntersectionPoint>();
            }

            public IReadOnlyList<CircularIntersectionPoint> GetCircularIntersections(Point center, double radius)
            {
                return Array.Empty<CircularIntersectionPoint>();
            }
        }
    }
}
