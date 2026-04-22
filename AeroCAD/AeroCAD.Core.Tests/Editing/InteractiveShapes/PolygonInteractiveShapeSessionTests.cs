using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class PolygonInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsAllState()
        {
            var session = new PolygonInteractiveShapeSession();
            session.TrySetSides(6);
            session.BeginEdgeMode();
            session.SetCenter(new Point(1, 2));
            session.ChooseCenterMode(false);
            session.SetFirstEdgePoint(new Point(3, 4));

            session.Reset();

            Assert.False(session.HasSides);
            Assert.False(session.UseEdgeMode);
            Assert.False(session.HasCenter);
            Assert.False(session.HasCenterModeChoice);
            Assert.True(session.UseInscribed);
            Assert.False(session.HasFirstEdgePoint);
        }

        [Fact]
        public void BuildPreview_CenterMode_ReturnsPolygonPreview()
        {
            var session = new PolygonInteractiveShapeSession();
            session.TrySetSides(4);
            session.SetCenter(new Point(0, 0));
            session.ChooseCenterMode(true);

            var preview = session.BuildPreview(new Point(10, 0));

            Assert.NotNull(preview);
            Assert.Equal(5, preview.Points.Count);
        }

        [Fact]
        public void BuildPreview_EdgeMode_ReturnsPolygonPreview()
        {
            var session = new PolygonInteractiveShapeSession();
            session.TrySetSides(4);
            session.BeginEdgeMode();
            session.SetFirstEdgePoint(new Point(0, 0));

            var preview = session.BuildPreview(new Point(10, 0));

            Assert.NotNull(preview);
            Assert.Equal(5, preview.Points.Count);
        }

        [Fact]
        public void TryBuildCenterPolygon_UsesSelectedMode()
        {
            var session = new PolygonInteractiveShapeSession();
            session.TrySetSides(4);
            session.SetCenter(new Point(0, 0));
            session.ChooseCenterMode(false);

            var ok = session.TryBuildCenterPolygon(new Point(0, 10), out var points);

            Assert.True(ok);
            Assert.NotNull(points);
            Assert.Equal(5, points.Count);
        }

        [Fact]
        public void TryBuildEdgePolygon_UsesFirstEdgePoint()
        {
            var session = new PolygonInteractiveShapeSession();
            session.TrySetSides(4);
            session.BeginEdgeMode();
            session.SetFirstEdgePoint(new Point(0, 0));

            var ok = session.TryBuildEdgePolygon(new Point(10, 0), out var points);

            Assert.True(ok);
            Assert.NotNull(points);
            Assert.Equal(5, points.Count);
        }

        [Fact]
        public void BuildCenterPreview_ReturnsGuideCircleAndPolygon()
        {
            var session = new PolygonInteractiveShapeSession();
            session.TrySetSides(4);
            session.SetCenter(new Point(0, 0));
            session.ChooseCenterMode(true);

            var preview = session.BuildCenterPreview(new Point(10, 0));

            Assert.True(preview.HasContent);
            Assert.Equal(3, preview.Strokes.Count);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.Orange);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.LightGray);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.White);
        }
    }
}
