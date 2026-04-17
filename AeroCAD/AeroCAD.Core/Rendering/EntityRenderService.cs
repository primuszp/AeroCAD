using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class EntityRenderService : IEntityRenderService
    {
        private readonly IReadOnlyList<IEntityRenderStrategy> strategies;

        public EntityRenderService(IEnumerable<IEntityRenderStrategy> strategies)
        {
            this.strategies = strategies?.ToList() ?? throw new ArgumentNullException(nameof(strategies));
        }

        public void Render(Entity entity, Layer layer, EntityVisual visual)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));

            var strategy = strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
            if (strategy == null)
            {
                visual.Redraw(_ => { });
                return;
            }

            var context = new EntityRenderContext(
                layer,
                CreatePen(entity, layer),
                CreateHighlightPen(entity, layer),
                CreateHighlightGlowPen(entity, layer));
            visual.Redraw(drawingContext => strategy.Render(entity, drawingContext, context));
        }

        private static Pen CreatePen(Entity entity, Layer layer)
        {
            var brush = new SolidColorBrush(layer.Color);
            if (brush.CanFreeze)
                brush.Freeze();

            var pen = new Pen(brush, entity.Thickness * entity.Scale)
            {
                DashStyle = DashStyles.Solid,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };

            if (pen.CanFreeze)
                pen.Freeze();

            return pen;
        }

        private static Pen CreateHighlightPen(Entity entity, Layer layer)
        {
            if (!entity.HasVisualHighlight)
                return null;

            var brush = new SolidColorBrush(layer.Color)
            {
                Opacity = entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? 0.65d : 0.9d
            };
            if (brush.CanFreeze)
                brush.Freeze();

            double outlineThickness = entity.Thickness * entity.Scale +
                ((entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? 1.25d : 2.0d) * entity.Scale);
            var pen = new Pen(brush, outlineThickness)
            {
                DashStyle = DashStyles.Solid,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };

            if (pen.CanFreeze)
                pen.Freeze();

            return pen;
        }

        private static Pen CreateHighlightGlowPen(Entity entity, Layer layer)
        {
            if (!entity.HasVisualHighlight)
                return null;

            Color baseColor = layer.Color;
            var glowColor = Color.FromArgb(
                entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? (byte)170 : (byte)220,
                (byte)(baseColor.R + ((255 - baseColor.R) * 0.65)),
                (byte)(baseColor.G + ((255 - baseColor.G) * 0.65)),
                (byte)(baseColor.B + ((255 - baseColor.B) * 0.65)));

            var brush = new SolidColorBrush(glowColor);
            if (brush.CanFreeze)
                brush.Freeze();

            double glowThickness = entity.Thickness * entity.Scale +
                ((entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? 3.25d : 5.0d) * entity.Scale);
            var pen = new Pen(brush, glowThickness)
            {
                DashStyle = DashStyles.Solid,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };

            if (pen.CanFreeze)
                pen.Freeze();

            return pen;
        }
    }
}

