using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class CustomEntityBaseTests
    {
        [Fact]
        public void Clone_PreservesIdentityGeometryAndStyle()
        {
            var entity = new TestCustomEntity(new Point(1, 2))
            {
                Thickness = 3,
                Color = EntityColor.FromAci(4)
            };

            var clone = Assert.IsType<TestCustomEntity>(entity.Clone());

            Assert.Equal(entity.Id, clone.Id);
            Assert.Equal(new Point(1, 2), clone.Point);
            Assert.Equal(3, clone.Thickness);
            Assert.Equal(EntityColorKind.Indexed, clone.Color.Kind);
            Assert.Equal((byte)4, clone.Color.AciIndex);
        }

        [Fact]
        public void Duplicate_CreatesNewIdentityAndPreservesGeometryAndStyle()
        {
            var entity = new TestCustomEntity(new Point(1, 2))
            {
                Thickness = 3,
                Color = EntityColor.FromRgb(Colors.SteelBlue)
            };

            var duplicate = Assert.IsType<TestCustomEntity>(entity.Duplicate());

            Assert.NotEqual(entity.Id, duplicate.Id);
            Assert.Equal(new Point(1, 2), duplicate.Point);
            Assert.Equal(3, duplicate.Thickness);
            Assert.Equal(EntityColorKind.TrueColor, duplicate.Color.Kind);
        }

        [Fact]
        public void RestoreState_RestoresGeometryAndBaseStyleAndInvalidatesOnce()
        {
            var entity = new TestCustomEntity(new Point(10, 20))
            {
                Thickness = 1,
                Color = EntityColor.FromAci(1)
            };
            var snapshot = entity.Clone();
            int geometryChangedCount = 0;
            entity.GeometryChanged += (_, _) => geometryChangedCount++;

            entity.Translate(new Vector(5, 6));
            entity.Thickness = 9;
            entity.Color = EntityColor.FromAci(6);
            geometryChangedCount = 0;

            entity.RestoreState(snapshot);

            Assert.Equal(new Point(10, 20), entity.Point);
            Assert.Equal(1, entity.Thickness);
            Assert.Equal((byte)1, entity.Color.AciIndex);
            Assert.Equal(1, geometryChangedCount);
        }

        private sealed class TestCustomEntity : CustomEntityBase
        {
            public TestCustomEntity(Point point)
            {
                Point = point;
            }

            public Point Point { get; private set; }

            public override int GripCount => 1;

            public override Point GetGripPoint(int index) => Point;

            public override void MoveGrip(int index, Point newPosition)
            {
                Point = newPosition;
                InvalidateEntityGeometry();
            }

            public override void Translate(Vector delta)
            {
                Point += delta;
                InvalidateEntityGeometry();
            }

            protected override CustomEntityBase CreateInstanceCore()
            {
                return new TestCustomEntity(default);
            }

            protected override void CopyGeometryTo(CustomEntityBase target)
            {
                if (target is TestCustomEntity entity)
                    entity.Point = Point;
            }

            protected override void CopyGeometryFrom(CustomEntityBase source)
            {
                if (source is TestCustomEntity entity)
                    Point = entity.Point;
            }
        }
    }
}
