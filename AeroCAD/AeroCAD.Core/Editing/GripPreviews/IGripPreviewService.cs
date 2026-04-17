using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public interface IGripPreviewService
    {
        GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition);
    }
}

