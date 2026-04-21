using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using static Primusz.AeroCAD.Core.Drawing.Layers.LineWeightPalette;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class EntityRenderService : IEntityRenderService
    {
        private readonly IReadOnlyList<IEntityRenderStrategy> strategies;
        private readonly Dictionary<PenCacheKey, Pen> penCache = new Dictionary<PenCacheKey, Pen>();

        public EntityRenderService(IEnumerable<IEntityRenderStrategy> strategies)
        {
            this.strategies = strategies?.ToList() ?? throw new ArgumentNullException(nameof(strategies));
        }

        public void InvalidateLayerCache(Layer layer)
        {
            penCache.Clear();
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
                GetOrCreatePen(CreateBaseKey(entity, layer)),
                GetOrCreatePen(CreateHighlightKey(entity, layer)),
                GetOrCreatePen(CreateGlowKey(entity, layer)));
            visual.Redraw(drawingContext => strategy.Render(entity, drawingContext, context));
        }

        private Pen GetOrCreatePen(PenCacheKey? key)
        {
            if (!key.HasValue)
                return null;

            if (penCache.TryGetValue(key.Value, out var cachedPen))
                return cachedPen;

            var brush = new SolidColorBrush(key.Value.Color) { Opacity = key.Value.Opacity };
            if (brush.CanFreeze)
                brush.Freeze();

            var pen = new Pen(brush, key.Value.Thickness)
            {
                DashStyle = GetDashStyle(key.Value.LineStyle),
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };

            if (pen.CanFreeze)
                pen.Freeze();

            penCache[key.Value] = pen;
            return pen;
        }

        private static Color ResolveColor(Entity entity, Layer layer)
        {
            return entity.Color.Resolve(layer.Color);
        }

        private static PenCacheKey CreateBaseKey(Entity entity, Layer layer)
        {
            return new PenCacheKey(ResolveColor(entity, layer), GetLineWeight(layer) * entity.Scale, 1.0d, GetLineStyle(layer));
        }

        private static PenCacheKey? CreateHighlightKey(Entity entity, Layer layer)
        {
            if (!entity.HasVisualHighlight)
                return null;

            double opacity = entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? 0.65d : 0.9d;
            double baseThickness = GetLineWeight(layer) * entity.Scale;
            double outlineThickness = baseThickness +
                ((entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? 1.25d : 2.0d) * entity.Scale);
            return new PenCacheKey(ResolveColor(entity, layer), outlineThickness, opacity, GetLineStyle(layer));
        }

        private static PenCacheKey? CreateGlowKey(Entity entity, Layer layer)
        {
            if (!entity.HasVisualHighlight)
                return null;

            Color baseColor = ResolveColor(entity, layer);
            var glowColor = Color.FromArgb(
                255,
                (byte)(baseColor.R + ((255 - baseColor.R) * 0.65)),
                (byte)(baseColor.G + ((255 - baseColor.G) * 0.65)),
                (byte)(baseColor.B + ((255 - baseColor.B) * 0.65)));

            double opacity = entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? (170d / 255d) : (220d / 255d);
            double baseThickness = GetLineWeight(layer) * entity.Scale;
            double glowThickness = baseThickness +
                ((entity.CommandHighlight == EntityCommandHighlightKind.Hover && !entity.IsSelected ? 3.25d : 5.0d) * entity.Scale);
            return new PenCacheKey(glowColor, glowThickness, opacity, GetLineStyle(layer));
        }

        private static double GetLineWeight(Layer layer)
        {
            var style = layer?.Style;
            double mm = style != null && style.LineWeight > 0d ? style.LineWeight : Default;
            // Convert mm → screen pixels: 0.25 mm = 1 px (zoom-independent via entity.Scale)
            return mm * PixelsPerMm;
        }

        private static LineStyle GetLineStyle(Layer layer)
        {
            return layer?.Style?.LineStyle ?? LineStyle.Solid;
        }

        private static DashStyle GetDashStyle(LineStyle lineStyle)
        {
            switch (lineStyle)
            {
                case LineStyle.Dashed:
                    return DashStyles.Dash;
                case LineStyle.DotDash:
                    return DashStyles.DashDot;
                case LineStyle.Dotted:
                    return DashStyles.Dot;
                default:
                    return DashStyles.Solid;
            }
        }

        private readonly struct PenCacheKey : IEquatable<PenCacheKey>
        {
            public PenCacheKey(Color color, double thickness, double opacity, LineStyle lineStyle)
            {
                Color = color;
                Thickness = thickness;
                Opacity = opacity;
                LineStyle = lineStyle;
            }

            public Color Color { get; }

            public double Thickness { get; }

            public double Opacity { get; }

            public LineStyle LineStyle { get; }

            public bool Equals(PenCacheKey other)
            {
                return Color.Equals(other.Color)
                    && Thickness.Equals(other.Thickness)
                    && Opacity.Equals(other.Opacity)
                    && LineStyle == other.LineStyle;
            }

            public override bool Equals(object obj)
            {
                return obj is PenCacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Color.GetHashCode();
                    hashCode = (hashCode * 397) ^ Thickness.GetHashCode();
                    hashCode = (hashCode * 397) ^ Opacity.GetHashCode();
                    hashCode = (hashCode * 397) ^ LineStyle.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}

