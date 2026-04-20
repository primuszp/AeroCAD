using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class PolylinePathOperationsTests
    {
        [Fact]
        public void IsClosed_ReturnsTrue_ForClosedPolyline()
        {
            var polyline = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(10, 10),
                new Point(0, 0)
            });

            Assert.True(PolylinePathOperations.IsClosed(polyline));
        }

        [Fact]
        public void BuildOpenPath_ReturnsPathWithIntermediatePoints()
        {
            var polyline = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(20, 0)
            })
            {
                Thickness = 2
            };

            var path = PolylinePathOperations.BuildOpenPath(polyline, 0.25, 1.75);

            Assert.NotNull(path);
            Assert.Equal(2, path.Thickness);
            Assert.Equal(3, path.Points.Count);
            Assert.Equal(new Point(2.5, 0), path.Points[0]);
            Assert.Equal(new Point(10, 0), path.Points[1]);
            Assert.Equal(new Point(17.5, 0), path.Points[2]);
        }

        [Fact]
        public void BuildClosedPath_ReturnsWrappedPath()
        {
            var polyline = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(10, 10),
                new Point(0, 10),
                new Point(0, 0)
            });

            var path = PolylinePathOperations.BuildClosedPath(polyline, 2.5, 0.5);

            Assert.NotNull(path);
            Assert.Equal(new Point(5, 10), path.Points[0]);
            Assert.Equal(new Point(5, 0), path.Points[path.Points.Count - 1]);
            Assert.Contains(new Point(0, 10), path.Points);
            Assert.Contains(new Point(0, 0), path.Points);
        }

        [Fact]
        public void ReplaceEndpoint_ChangesOnlySelectedEndpoint()
        {
            var polyline = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(20, 0)
            });

            var replaced = PolylinePathOperations.ReplaceEndpoint(polyline, replaceStart: true, new Point(-5, 0));

            Assert.NotNull(replaced);
            Assert.Equal(new Point(-5, 0), replaced.Points[0]);
            Assert.Equal(new Point(20, 0), replaced.Points[2]);
        }
    }
}
