using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class TrimExtendCommandSession : IInteractiveShapeSession
    {
        public List<Entity> Boundaries { get; } = new List<Entity>();
        public List<Entity> HighlightedBoundaries { get; } = new List<Entity>();
        public Entity HighlightedTargetEntity { get; private set; }

        public void Reset()
        {
            Boundaries.Clear();
            HighlightedBoundaries.Clear();
            HighlightedTargetEntity = null;
        }

        public bool HasBoundaries => Boundaries.Count > 0;

        public bool AddBoundary(Entity entity)
        {
            if (entity == null || Boundaries.Contains(entity))
                return false;

            Boundaries.Add(entity);
            return true;
        }

        public void RemoveTargetHighlight()
        {
            HighlightedTargetEntity = null;
        }

        public void SetTargetHighlight(Entity entity)
        {
            HighlightedTargetEntity = entity;
        }

        public GripPreview BuildPreview(ITransientEntityPreviewService previewService, Entity previewEntity, Color color)
        {
            return previewService?.CreatePreview(previewEntity, color) ?? GripPreview.Empty;
        }
    }
}
