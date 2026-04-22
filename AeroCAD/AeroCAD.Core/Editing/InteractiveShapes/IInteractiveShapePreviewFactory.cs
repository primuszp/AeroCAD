using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public interface IInteractiveShapePreviewFactory
    {
        GripPreview CreatePreview(IInteractiveShapePreviewContext context);
    }
}
