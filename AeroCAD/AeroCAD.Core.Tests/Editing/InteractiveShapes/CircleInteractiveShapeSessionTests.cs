using System.Windows;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class CircleInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsCircleState()
        {
            var session = new CircleInteractiveShapeSession();
            session.BeginCenter(new Point(1, 2));
            session.BeginDiameterInput();

            session.Reset();

            Assert.False(session.HasCenterPoint);
            Assert.Equal(new Point(), session.CenterPoint);
            Assert.False(session.UseDiameterInput);
        }

        [Fact]
        public void BeginCenter_StoresCenterPoint()
        {
            var session = new CircleInteractiveShapeSession();

            session.BeginCenter(new Point(4, 5));

            Assert.True(session.HasCenterPoint);
            Assert.Equal(new Point(4, 5), session.CenterPoint);
            Assert.False(session.UseDiameterInput);
        }

        [Fact]
        public void GetRadiusFromPoint_ReturnsDistanceFromCenter()
        {
            var session = new CircleInteractiveShapeSession();
            session.BeginCenter(new Point(0, 0));

            Assert.Equal(10, session.GetRadiusFromPoint(new Point(10, 0)), 6);
        }

        [Fact]
        public void GetDiameterFromPoint_ReturnsDistanceFromCenter()
        {
            var session = new CircleInteractiveShapeSession();
            session.BeginCenter(new Point(0, 0));

            Assert.Equal(10, session.GetDiameterFromPoint(new Point(10, 0)), 6);
        }
    }
}
