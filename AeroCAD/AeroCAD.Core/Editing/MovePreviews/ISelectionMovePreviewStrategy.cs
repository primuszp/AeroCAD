using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public interface ISelectionMovePreviewStrategy
    {
        bool CanHandle(Entity entity);

        GripPreview CreatePreview(Entity entity, Vector displacement);
    }
}

