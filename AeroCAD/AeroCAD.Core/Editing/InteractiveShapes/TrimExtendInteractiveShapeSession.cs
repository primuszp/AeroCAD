using System.Collections.Generic;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class TrimExtendInteractiveShapeSession
    {
        public List<Entity> BoundaryEntities { get; } = new List<Entity>();
        public List<Entity> HighlightedBoundaries { get; } = new List<Entity>();
        public Entity HighlightedTargetEntity { get; private set; }

        public void Reset()
        {
            BoundaryEntities.Clear();
            HighlightedBoundaries.Clear();
            HighlightedTargetEntity = null;
        }

        public bool HasBoundaries => BoundaryEntities.Count > 0;

        public bool AddBoundary(Entity entity)
        {
            if (entity == null || BoundaryEntities.Contains(entity))
                return false;

            BoundaryEntities.Add(entity);
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
