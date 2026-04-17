using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class ArcGripPreviewStrategy : IGripPreviewStrategy
    {
        private const double HelperStrokeThickness = 1.5d;

        public bool CanHandle(Entity entity)
        {
            return entity is Arc;
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var arc = entity as Arc;
            if (arc == null)
                return GripPreview.Empty;

            Color entityColor = GetEntityColor(entity);
            int dir = arc.SweepAngle >= 0d ? 1 : -1;

            switch (gripIndex)
            {
                case 0: // StartPoint — keep EndAngle fixed
                {
                    double endAngle = arc.EndAngle;
                    double newStart = CircularGeometry.GetAngle(arc.Center, newPosition);
                    double newSweep = CircularGeometry.GetDirectionalDistance(newStart, endAngle, dir);
                    if (dir < 0) newSweep = -newSweep;
                    var previewGeom = System.Math.Abs(newSweep) > 0.001
                        ? Arc.BuildGeometry(arc.Center, arc.Radius, newStart, newSweep)
                        : Geometry.Empty;
                    return Build(new LineGeometry(arc.StartPoint, newPosition), previewGeom, entityColor, entity.Thickness);
                }
                case 1: // EndPoint — keep StartAngle fixed
                {
                    double newEnd = CircularGeometry.GetAngle(arc.Center, newPosition);
                    double newSweep = CircularGeometry.GetDirectionalDistance(arc.StartAngle, newEnd, dir);
                    if (dir < 0) newSweep = -newSweep;
                    var previewGeom = System.Math.Abs(newSweep) > 0.001
                        ? Arc.BuildGeometry(arc.Center, arc.Radius, arc.StartAngle, newSweep)
                        : Geometry.Empty;
                    return Build(new LineGeometry(arc.EndPoint, newPosition), previewGeom, entityColor, entity.Thickness);
                }
                case 2: // MidPoint on arc — adjust radius
                {
                    double newRadius = (newPosition - arc.Center).Length;
                    var previewGeom = newRadius > 0.001d
                        ? Arc.BuildGeometry(arc.Center, newRadius, arc.StartAngle, arc.SweepAngle)
                        : Geometry.Empty;
                    return Build(new LineGeometry(arc.MidPoint, newPosition), previewGeom, entityColor, entity.Thickness);
                }
                case 3: // Center — translate the whole arc
                {
                    Vector delta = newPosition - arc.Center;
                    var previewGeom = Arc.BuildGeometry(arc.Center + delta, arc.Radius, arc.StartAngle, arc.SweepAngle);
                    return Build(new LineGeometry(arc.Center, newPosition), previewGeom, entityColor, entity.Thickness);
                }
                default:
                    return GripPreview.Empty;
            }
        }

        private static GripPreview Build(Geometry helperGeometry, Geometry entityGeometry, Color entityColor, double thickness)
        {
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(helperGeometry, Colors.Orange, HelperStrokeThickness, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(entityGeometry, entityColor, thickness)
            });
        }

        private static Color GetEntityColor(Entity entity)
        {
            var layer = entity.RenderHost as Layer;
            return layer?.Color ?? Colors.White;
        }
    }
}
