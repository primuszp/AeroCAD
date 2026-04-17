using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public interface ITransientEntityPreviewStrategy
    {
        bool CanHandle(Entity entity);

        GripPreview CreatePreview(Entity entity, Color color);
    }
}
