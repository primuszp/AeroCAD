using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public class CircleTransientEntityPreviewStrategy : ITransientEntityPreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Circle;
        }

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            var circle = entity as Circle;
            if (circle == null || circle.Radius <= 0d)
                return GripPreview.Empty;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(Circle.BuildGeometry(circle.Center, circle.Radius), color, circle.Thickness)
            });
        }
    }
}
