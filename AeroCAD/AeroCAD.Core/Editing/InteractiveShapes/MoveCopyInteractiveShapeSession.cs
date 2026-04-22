using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class MoveCopyInteractiveShapeSession
    {
        public IReadOnlyList<Entity> SelectedEntities { get; private set; } = System.Array.Empty<Entity>();
        public IReadOnlyList<ModifyEntitiesCommand.EntityStateRecord> StateRecords { get; private set; } = System.Array.Empty<ModifyEntitiesCommand.EntityStateRecord>();
        public Point BasePoint { get; private set; }
        public bool HasBasePoint { get; private set; }

        public void Reset()
        {
            SelectedEntities = System.Array.Empty<Entity>();
            StateRecords = System.Array.Empty<ModifyEntitiesCommand.EntityStateRecord>();
            BasePoint = default(Point);
            HasBasePoint = false;
        }

        public void InitializeSelection(ISelectionManager selectionManager)
        {
            SelectedEntities = selectionManager != null
                ? selectionManager.SelectedEntities.ToList().AsReadOnly()
                : new List<Entity>().AsReadOnly();

            StateRecords = SelectedEntities
                .Select(entity => new ModifyEntitiesCommand.EntityStateRecord(entity, entity.Clone(), entity.Clone()))
                .ToList()
                .AsReadOnly();
        }

        public void BeginBasePoint(Point point)
        {
            BasePoint = point;
            HasBasePoint = true;
        }

        public Vector GetDisplacement(Point currentPoint)
        {
            return currentPoint - BasePoint;
        }

        public void Finish()
        {
            Reset();
        }

        public GripPreview BuildPreview(ISelectionMovePreviewService movePreviewService, Point currentPoint, bool includeEntityPreview = true)
        {
            if (!HasBasePoint)
                return GripPreview.Empty;

            var displacement = GetDisplacement(currentPoint);
            var strokes = new List<GripPreviewStroke>();
            if (includeEntityPreview)
            {
                var preview = movePreviewService?.CreatePreview(SelectedEntities, displacement) ?? GripPreview.Empty;
                strokes.AddRange(preview.Strokes);
            }

            strokes.Add(GripPreviewStroke.CreateScreenConstant(new LineGeometry(BasePoint, currentPoint), Colors.Orange, 1.5d, DashStyles.Dash));
            return new GripPreview(strokes);
        }
    }
}
