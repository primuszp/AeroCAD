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
        private Color color;

        #endregion

        #region Properties

        public Guid Id { get; private set; }

        public string LayerName { get; set; }

        public Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    foreach (var entity in entityVisuals.Values.Select(visual => visual.Entity))
                        RenderEntity(entity);
                }
            }
        }

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
            Color = Colors.White;
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
            if (entity == null)
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
            Rect rectangle = new Rect(point.X - 4 * Scale, point.Y - 4 * Scale, 8 * Scale, 8 * Scale);
            var hits = QueryHitEntities(rectangle, false, null);
            AddPointPickFallbackHits(point, null, hits);
            return hits;
        }

        public IList<Entity> QueryHitEntities(Point point, IEnumerable<Entity> candidates)
        {
            Rect rectangle = new Rect(point.X - 4 * Scale, point.Y - 4 * Scale, 8 * Scale, 8 * Scale);
            var hits = QueryHitEntities(rectangle, false, candidates);
            AddPointPickFallbackHits(point, candidates, hits);
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

        private void AddPointPickFallbackHits(Point point, IEnumerable<Entity> candidates, IList<Entity> hits)
        {
            IEnumerable<Entity> source = candidates ?? Entities;
            double tolerance = 4d * Scale;

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

        #endregion
    }
}

