using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public class LineTransientEntityPreviewStrategy : ITransientEntityPreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Line;
        }

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            var line = entity as Line;
            if (line == null)
                return GripPreview.Empty;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(line.StartPoint, line.EndPoint), Colors.White, line.Thickness)
            });
        }
    }
}
