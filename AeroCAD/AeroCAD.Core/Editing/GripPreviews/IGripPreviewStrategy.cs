using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public interface IGripPreviewStrategy
    {
        bool CanHandle(Entity entity);

        GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition);
    }
}

