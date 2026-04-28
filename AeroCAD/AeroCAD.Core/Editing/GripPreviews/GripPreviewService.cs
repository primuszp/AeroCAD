using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class GripPreviewService : IGripPreviewService
    {
        private const double HelperStrokeThickness = 1.5d;
        private static readonly Color FallbackPreviewColor = Colors.Orange;
        private readonly IReadOnlyList<IGripPreviewStrategy> strategies;

        public GripPreviewService(IEnumerable<IGripPreviewStrategy> strategies)
        {
            this.strategies = (strategies ?? Enumerable.Empty<IGripPreviewStrategy>()).ToList();
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            if (entity == null)
                return GripPreview.Empty;

            var strategy = strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
            return strategy?.CreatePreview(entity, gripIndex, newPosition) ?? CreateFallbackPreview(entity, gripIndex, newPosition);
        }

        private static GripPreview CreateFallbackPreview(Entity entity, int gripIndex, Point newPosition)
        {
            if (gripIndex < 0 || gripIndex >= entity.GripCount)
                return GripPreview.Empty;

            var previewEntity = entity.Clone();
            previewEntity.MoveGrip(gripIndex, newPosition);
            var geometry = previewEntity.GetPreviewGeometry();
            if (geometry == null || geometry.IsEmpty())
                geometry = new LineGeometry(entity.GetGripPoint(gripIndex), newPosition);

            if (geometry.CanFreeze)
                geometry.Freeze();

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(geometry, FallbackPreviewColor, HelperStrokeThickness, DashStyles.Dash)
            });
        }
    }
}
