using System;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class OffsetInteractiveShapeSession
    {
        public Entity SourceEntity { get; private set; }
        public Guid SourceLayerId { get; private set; }
        public double? FixedDistance { get; private set; }

        public void Reset()
        {
            SourceEntity = null;
            SourceLayerId = Guid.Empty;
            FixedDistance = null;
        }

        public void ResetSelection()
        {
            SourceEntity = null;
            SourceLayerId = Guid.Empty;
        }

        public void BeginSelection(Entity entity, Guid layerId)
        {
            SourceEntity = entity;
            SourceLayerId = layerId;
        }

        public void SetFixedDistance(double distance)
        {
            FixedDistance = Math.Abs(distance);
        }

        public bool HasSelectedEntity => SourceEntity != null;
        public bool IsReady => SourceEntity != null && SourceLayerId != Guid.Empty;

        public GripPreview BuildPreview(ITransientEntityPreviewService previewService, Entity previewEntity, Color sourceColor)
        {
            return previewService?.CreatePreview(previewEntity, sourceColor) ?? GripPreview.Empty;
        }
    }
}
