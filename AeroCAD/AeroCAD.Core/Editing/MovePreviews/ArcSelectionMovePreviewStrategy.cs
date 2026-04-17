using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public class ArcSelectionMovePreviewStrategy : ISelectionMovePreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Arc;
        }

        public GripPreview CreatePreview(Entity entity, Vector displacement)
        {
            var arc = entity as Arc;
            if (arc == null || arc.Radius <= 0d || System.Math.Abs(arc.SweepAngle) <= double.Epsilon)
                return GripPreview.Empty;

            var movedGeometry = Arc.BuildGeometry(arc.Center + displacement, arc.Radius, arc.StartAngle, arc.SweepAngle);
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(movedGeometry, GetEntityColor(entity), entity.Thickness)
            });
        }

        private static Color GetEntityColor(Entity entity)
        {
            var layer = entity.RenderHost as Layer;
            return layer?.Color ?? Colors.White;
        }
    }
}
