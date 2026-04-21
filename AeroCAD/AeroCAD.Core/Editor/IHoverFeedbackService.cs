using System.Windows;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface IHoverFeedbackService
    {
        bool CanUpdateSnap(EditorMode mode, bool hasSelectedGrips);

        Point? ResolveStatusPoint(EditorMode mode, bool hasSelectedGrips, SnapResult snapResult);

        Point ResolveGripPoint(Primusz.AeroCAD.Core.Drawing.Entities.Entity entity, int gripIndex, Point fallbackPoint);
    }
}
