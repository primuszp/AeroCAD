using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public interface ITransientEntityPreviewService
    {
        GripPreview CreatePreview(Entity entity, Color color);
    }
}
