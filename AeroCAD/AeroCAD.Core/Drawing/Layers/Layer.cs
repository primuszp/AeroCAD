using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.GeometryMath;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class Layer : VisualHost, IViewportSpaceElement, IEntityRenderHost
    {
        #region Members

        private readonly Dictionary<Guid, EntityVisual> entityVisuals = new Dictionary<Guid, EntityVisual>();
        private LayerStyle style;

        #endregion

        #region Properties

        public Guid Id { get; private set; }

        public string LayerName { get; set; }

        public Color Color
        {
            get { return Style.Color; }
            set
            {
                Style.Color = value;
            }
        }

        public new LayerStyle Style
        {
            get { return style; }
            set
            {
                if (ReferenceEquals(style, value))
                    return;

                if (style != null)
                    style.PropertyChanged -= OnStylePropertyChanged;

                style = value ?? new LayerStyle();
                style.PropertyChanged += OnStylePropertyChanged;
                UpdateLayerPresentation();
                RefreshEntityVisuals();
            }
        }

        public bool IsFrozen => Style.IsFrozen;

        public bool IsLocked => Style.IsLocked;

        public bool IsRenderable => Style.IsVisible && !Style.IsFrozen;

        public bool IsQueryable => IsRenderable;

        public bool IsEditable => IsRenderable && !IsLocked;

        public IEntityRenderService RenderService { get; set; }

        public IList<Entity> Entities
        {
            get { return entityVisuals.Values.Select(visual => visual.Entity).ToList(); }
        }

        public ViewportCoordinateSpace CoordinateSpace => ViewportCoordinateSpace.World;

        #endregion

        #region Constructors

        public Layer()
        {
            Id = Guid.NewGuid();
            Style = new LayerStyle();
        }

        #endregion

        public void Add(Entity entity)
        {
            if (entity == null || entityVisuals.ContainsKey(entity.Id))
                return;

            var visual = new EntityVisual(entity);
            entityVisuals.Add(entity.Id, visual);
            entity.RenderHost = this;
            entity.Scale = Scale; // inherit current zoom-corrected scale before first render
            Visuals.Add(visual);

            if (IsRenderable)
                RenderEntity(entity);
        }

        public void Remove(Entity entity)
        {
            if (entity == null)
                return;

            EntityVisual visual;
            if (!entityVisuals.TryGetValue(entity.Id, out visual))
                return;

            entityVisuals.Remove(entity.Id);
            entity.RenderHost = null;
            Visuals.Remove(visual);
        }

        public void Clear()
        {
            foreach (var entity in entityVisuals.Values.Select(value => value.Entity))
                entity.RenderHost = null;

            entityVisuals.Clear();
            Visuals.Clear();
        }

        private void OnStylePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LayerStyle.Color):
                case nameof(LayerStyle.LineStyle):
                case nameof(LayerStyle.LineWeight):
                    RenderService?.InvalidateLayerCache(this);
                    RefreshEntityVisuals();
                    break;
                case nameof(LayerStyle.IsVisible):
                case nameof(LayerStyle.IsFrozen):
                    UpdateLayerPresentation();
                    if (IsRenderable)
                        RefreshEntityVisuals();
                    break;
                case nameof(LayerStyle.IsLocked):
                    break;
                default:
                    RefreshEntityVisuals();
                    break;
            }
        }

        // Layer.OnRender is intentionally NOT overriding entity rendering.
        // Entities draw in world-space (Layer.RenderTransform = ViewTransform handles zoom/pan via GPU).
        // Entities render themselves via Render() when their geometry or visual state changes.

        protected override void ScaleUpdate()
        {
            foreach (Entity entity in entityVisuals.Values.Select(value => value.Entity))
                entity.Scale = Scale;
        }

        public void RenderEntity(Entity entity)
        {
            if (entity == null || !IsRenderable)
                return;

            EntityVisual visual;
            if (!entityVisuals.TryGetValue(entity.Id, out visual))
                return;

            RenderService?.Render(entity, this, visual);
        }

        #region HitTest

        /// <summary>
        /// Returns entities that intersect with the given point (with tolerance).
        /// Does NOT select them â€” caller is responsible for selection.
        /// </summary>
        public IList<Entity> QueryHitEntities(Point point)
        {
            return QueryHitEntities(point, 4d * Scale, null);
        }

        public IList<Entity> QueryHitEntities(Point point, double toleranceWorld)
        {
            return QueryHitEntities(point, toleranceWorld, null);
        }

        public IList<Entity> QueryHitEntities(Point point, IEnumerable<Entity> candidates)
        {
            return QueryHitEntities(point, 4d * Scale, candidates);
        }

        public IList<Entity> QueryHitEntities(Point point, double toleranceWorld, IEnumerable<Entity> candidates)
        {
            if (!IsQueryable)
                return new List<Entity>();

            double effectiveTolerance = toleranceWorld > 0 ? toleranceWorld : 4d * Scale;
            Rect rectangle = new Rect(
                point.X - effectiveTolerance,
                point.Y - effectiveTolerance,
                effectiveTolerance * 2,
                effectiveTolerance * 2);
            var hits = QueryHitEntities(rectangle, false, candidates);
            AddPointPickFallbackHits(point, effectiveTolerance, candidates, hits);
            return hits;
        }

        /// <summary>
        /// Returns entities that intersect with the given rectangle.
        /// Does NOT select them â€” caller is responsible for selection.
        /// </summary>
        public IList<Entity> QueryHitEntities(Rect rect, bool requireFullyInside = false)
        {
            return QueryHitEntities(rect, requireFullyInside, null);
        }

        public IList<Entity> QueryHitEntities(Rect rect, bool requireFullyInside, IEnumerable<Entity> candidates)
        {
            if (!IsQueryable)
                return new List<Entity>();

            var hits = new List<Entity>();
            var candidateIds = candidates != null
                ? new HashSet<Guid>(candidates.Select(entity => entity.Id))
                : null;

            Geometry rectangle = new RectangleGeometry(rect);
            VisualTreeHelper.HitTest(this, dobj =>
            {
                var visual = dobj as EntityVisual;
                if (visual != null && candidateIds != null && !candidateIds.Contains(visual.Entity.Id))
                    return HitTestFilterBehavior.ContinueSkipSelf;

                return HitTestFilter(dobj);
            }, result =>
            {
                var geometryResult = result as GeometryHitTestResult;
                if (geometryResult != null)
                {
                    Entity entity = (geometryResult.VisualHit as EntityVisual)?.Entity;
                    if (entity != null && !hits.Contains(entity))
                    {
                        if (requireFullyInside)
                        {
                            if (geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
                                hits.Add(entity);
                        }
                        else
                        {
                            if (geometryResult.IntersectionDetail != IntersectionDetail.Empty)
                                hits.Add(entity);
                        }
                    }
                }
                return HitTestResultBehavior.Continue;
            }, new GeometryHitTestParameters(rectangle));

            return hits;
        }

        public virtual HitTestFilterBehavior HitTestFilter(DependencyObject dobj)
        {
            return dobj.GetType() == typeof(Layer)
                ? HitTestFilterBehavior.ContinueSkipSelf
                : HitTestFilterBehavior.Continue;
        }

        private void AddPointPickFallbackHits(Point point, double tolerance, IEnumerable<Entity> candidates, IList<Entity> hits)
        {
            if (!IsQueryable)
                return;

            IEnumerable<Entity> source = candidates ?? Entities;

            foreach (var entity in source)
            {
                if (entity == null || hits.Contains(entity))
                    continue;

                Point? closest = GetClosestPointForPick(entity, point);
                if (!closest.HasValue)
                    continue;

                if ((closest.Value - point).Length <= tolerance)
                    hits.Add(entity);
            }
        }

        private static Point? GetClosestPointForPick(Entity entity, Point point)
        {
            var nearestDescriptor = entity.GetSnapDescriptors()
                .OfType<ComputedSnapDescriptor>()
                .FirstOrDefault(descriptor => descriptor.Type == SnapType.Nearest);

            return nearestDescriptor?.TrySnap(point, double.MaxValue)?.Point;
        }

        private void UpdateLayerPresentation()
        {
            Visibility = IsRenderable ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshEntityVisuals()
        {
            foreach (var entity in entityVisuals.Values.Select(visual => visual.Entity))
                RenderEntity(entity);
        }

        #endregion
    }
}

