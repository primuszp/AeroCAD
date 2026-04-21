using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class HoverFeedbackService : IHoverFeedbackService
    {
        public bool CanUpdateSnap(EditorMode mode, bool hasSelectedGrips)
        {
            return hasSelectedGrips || mode == EditorMode.CommandInput || mode == EditorMode.GripEditing;
        }

        public Point? ResolveStatusPoint(EditorMode mode, bool hasSelectedGrips, SnapResult snapResult)
        {
            if (snapResult == null || !CanUpdateSnap(mode, hasSelectedGrips))
                return null;

            if (snapResult.SourceEntity != null && snapResult.SourceGripIndex.HasValue)
                return snapResult.SourceEntity.GetGripPoint(snapResult.SourceGripIndex.Value);

            return snapResult.SourcePoint ?? snapResult.Point;
        }

        public Point ResolveGripPoint(Entity entity, int gripIndex, Point fallbackPoint)
        {
            if (entity != null)
                return entity.GetGripPoint(gripIndex);

            return fallbackPoint;
        }
    }
}
