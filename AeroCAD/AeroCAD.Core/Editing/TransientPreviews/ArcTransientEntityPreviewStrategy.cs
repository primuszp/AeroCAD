using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public class ArcTransientEntityPreviewStrategy : ITransientEntityPreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Arc;
        }

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            var arc = entity as Arc;
            if (arc == null || arc.Radius <= 0d || System.Math.Abs(arc.SweepAngle) <= double.Epsilon)
                return GripPreview.Empty;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(
                    Arc.BuildGeometry(arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle),
                    Colors.White,
                    arc.Thickness)
            });
        }
    }
}
