using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public interface ISelectionMovePreviewService
    {
        GripPreview CreatePreview(IEnumerable<Entity> entities, Vector displacement);
    }
}

