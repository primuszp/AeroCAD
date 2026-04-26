using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class ExternalBoundaryGeometryTests
    {
        [Fact]
        public void LineTrim_CanUseExternalBoundaryGeometry()
        {
            var boundary = new ExternalVerticalBoundary(5d, -10d, 10d);
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var strategy = new LineTrimExtendStrategy();

            Assert.True(strategy.CanTrim(new Entity[] { boundary }, target));

            var result = strategy.CreateTrimmed(new Entity[] { boundary }, target, new Point(8, 0));

            var line = Assert.IsType<Line>(Assert.Single(result));
            Assert.Equal(new Point(0, 0), line.StartPoint);
            Assert.Equal(new Point(5, 0), line.EndPoint);
        }

        private sealed class ExternalVerticalBoundary : Entity, ITrimExtendBoundaryGeometry
        {
            private readonly double x;
            private readonly double minY;
            private readonly double maxY;

            public ExternalVerticalBoundary(double x, double minY, double maxY)
            {
                this.x = x;
                this.minY = minY;
                this.maxY = maxY;
            }

            public override int GripCount => 0;
            public override Point GetGripPoint(int index) => default;
            public override void MoveGrip(int index, Point newPosition) { }
            public override Entity Clone() => new ExternalVerticalBoundary(x, minY, maxY);
            public override Entity Duplicate() => new ExternalVerticalBoundary(x, minY, maxY);
            public override void RestoreState(Entity sourceState) { }
            public override void Translate(Vector delta) { }

            public IReadOnlyList<LineIntersectionPoint> GetLineIntersections(Line target, bool restrictTargetToSegment)
            {
                double dx = target.EndPoint.X - target.StartPoint.X;
                if (Math.Abs(dx) <= 1e-9)
                    return Array.Empty<LineIntersectionPoint>();

                double parameter = (x - target.StartPoint.X) / dx;
                if (restrictTargetToSegment && (parameter < 0d || parameter > 1d))
                    return Array.Empty<LineIntersectionPoint>();

                var point = target.StartPoint + ((target.EndPoint - target.StartPoint) * parameter);
                return point.Y >= minY && point.Y <= maxY
                    ? new[] { new LineIntersectionPoint(point, parameter) }
                    : Array.Empty<LineIntersectionPoint>();
            }

            public IReadOnlyList<CircularIntersectionPoint> GetCircularIntersections(Point center, double radius)
            {
                return Array.Empty<CircularIntersectionPoint>();
            }
        }
    }
}
