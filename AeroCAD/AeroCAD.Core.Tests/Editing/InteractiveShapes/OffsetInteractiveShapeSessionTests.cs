using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class OffsetInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsOffsetState()
        {
            var session = new OffsetInteractiveShapeSession();
            session.BeginSelection(new Line(new Point(0, 0), new Point(1, 1)), Guid.NewGuid());
            session.SetFixedDistance(-3);

            session.Reset();

            Assert.Null(session.SourceEntity);
            Assert.Equal(Guid.Empty, session.SourceLayerId);
            Assert.Null(session.FixedDistance);
        }

        [Fact]
        public void BeginSelection_StoresEntityAndLayer()
        {
            var session = new OffsetInteractiveShapeSession();
            var entity = new Line(new Point(0, 0), new Point(1, 1));

            var layerId = Guid.NewGuid();
            session.BeginSelection(entity, layerId);

            Assert.Same(entity, session.SourceEntity);
            Assert.Equal(layerId, session.SourceLayerId);
            Assert.True(session.HasSelectedEntity);
            Assert.True(session.IsReady);
        }

        [Fact]
        public void SetFixedDistance_StoresAbsoluteValue()
        {
            var session = new OffsetInteractiveShapeSession();

            session.SetFixedDistance(-12.5);

            Assert.Equal(12.5, session.FixedDistance.Value, 6);
        }
    }
}
