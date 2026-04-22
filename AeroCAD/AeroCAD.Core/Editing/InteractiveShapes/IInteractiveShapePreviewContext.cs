using System.Windows;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public interface IInteractiveShapePreviewContext
    {
        Point? Center { get; }
        Point Cursor { get; }
        int Sides { get; }
        bool UseInscribed { get; }
        bool UseEdgeMode { get; }
        Point? EdgeStart { get; }
    }
}
