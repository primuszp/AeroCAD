using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Snapping;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Snapping
{
    public class SnapResultTests
    {
        [Fact]
        public void LineEndpointSnap_ExposesOriginalEndpointAsSourcePoint()
        {
            var line = new Line(new Point(0, 0), new Point(100, 100));
            var descriptors = line.GetSnapDescriptors();
            var engine = new SnapEngine(new SnapModePolicy(
                new[] { SnapType.Endpoint },
                new[] { SnapType.Endpoint }));

            engine.Update(new Point(0.2, 0.1), descriptors);

            var snap = engine.CurrentSnap;

            Assert.NotNull(snap);
            Assert.Equal(new Point(0, 0), snap.SourcePoint.Value);
            Assert.Same(line, snap.SourceEntity);
            Assert.Equal(0, snap.SourceGripIndex.Value);
            Assert.Equal(new Point(0, 0), snap.Point);
        }

        [Fact]
        public void LineOtherEndpointSnap_ExposesOriginalEndpointAsSourcePoint()
        {
            var line = new Line(new Point(0, 0), new Point(100, 100));
            var descriptors = line.GetSnapDescriptors();
            var engine = new SnapEngine(new SnapModePolicy(
                new[] { SnapType.Endpoint },
                new[] { SnapType.Endpoint }));

            engine.Update(new Point(99.9, 100.1), descriptors);

            var snap = engine.CurrentSnap;

            Assert.NotNull(snap);
            Assert.Equal(new Point(100, 100), snap.SourcePoint.Value);
            Assert.Same(line, snap.SourceEntity);
            Assert.Equal(1, snap.SourceGripIndex.Value);
            Assert.Equal(new Point(100, 100), snap.Point);
        }
    }
}
