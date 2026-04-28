using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public class SelectionMovePreviewService : ISelectionMovePreviewService
    {
        private const double FallbackStrokeThickness = 1.5d;
        private static readonly Color FallbackPreviewColor = Colors.Orange;
        private readonly IReadOnlyList<ISelectionMovePreviewStrategy> strategies;

        public SelectionMovePreviewService(IEnumerable<ISelectionMovePreviewStrategy> strategies)
        {
            this.strategies = (strategies ?? Enumerable.Empty<ISelectionMovePreviewStrategy>())
                .ToList()
                .AsReadOnly();
        }

        public GripPreview CreatePreview(IEnumerable<Entity> entities, Vector displacement)
        {
            if (entities == null)
                return GripPreview.Empty;

            var strokes = new List<GripPreviewStroke>();
            foreach (var entity in entities.Where(item => item != null))
            {
                var strategy = strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
                var preview = strategy?.CreatePreview(entity, displacement) ?? CreateFallbackPreview(entity, displacement);
                if (preview?.HasContent == true)
                    strokes.AddRange(preview.Strokes);
            }

            return strokes.Count == 0 ? GripPreview.Empty : new GripPreview(strokes);
        }

        private static GripPreview CreateFallbackPreview(Entity entity, Vector displacement)
        {
            var previewEntity = entity.Duplicate();
            previewEntity.Translate(displacement);
            var geometry = previewEntity.GetPreviewGeometry();
            if (geometry == null || geometry.IsEmpty())
                return GripPreview.Empty;

            if (geometry.CanFreeze)
                geometry.Freeze();

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(geometry, FallbackPreviewColor, FallbackStrokeThickness, DashStyles.Dash)
            });
        }
    }
}
