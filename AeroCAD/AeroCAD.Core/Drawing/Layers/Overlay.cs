using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Drawing.Markers;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class Overlay : VisualHost, IViewportSpaceElement, IViewportHostedElement
    {
        private const double GripHitPadding = 3.0d;
        private readonly IMarkerAppearanceService appearanceService;

        public ViewportCoordinateSpace CoordinateSpace => ViewportCoordinateSpace.Screen;

        #region Constructors

        public Overlay(Viewport viewport, IMarkerAppearanceService appearanceService)
        {
            this.appearanceService = appearanceService;
            IsHitTestVisible = false;
            viewport.Children.Add(this);
            Panel.SetZIndex(this, 1001);
            if (this.appearanceService != null)
                this.appearanceService.AppearanceChanged += (s, e) => RefreshGripAppearance();
        }

        #endregion

        /// <summary>
        /// Synchronizes grips with the current selection state.
        /// Removes grips for deselected entities, adds grips for newly selected entities.
        /// </summary>
        public void Update()
        {
            var vp = this.Viewport; // VisualHost.Viewport = Parent as Viewport
            if (vp == null) return;

            var selectedEntities = vp.GetLayers()
                .SelectMany(l => l.Entities)
                .Where(e => e.IsSelected)
                .ToList();

            // Remove grips whose owners are no longer selected
            var gripsToRemove = Visuals.OfType<Grip>()
                .Where(g => !selectedEntities.Contains(g.Owner))
                .ToList();
            foreach (var g in gripsToRemove)
                Visuals.Remove(g);

            // Add grips for newly selected entities
            foreach (var entity in selectedEntities)
            {
                bool hasGrips = Visuals.OfType<Grip>().Any(g => Equals(g.Owner, entity));
                if (!hasGrips)
                {
                    for (int i = 0; i < entity.GripCount; i++)
                        Visuals.Add(new Grip(entity, i, appearanceService));
                }
                else
                {
                    // Re-render existing grips (e.g. on zoom/pan or entity geometry change)
                    foreach (var grip in Visuals.OfType<Grip>().Where(g => Equals(g.Owner, entity)))
                        grip.Render();
                }
            }
        }

        /// <summary>
        /// Returns the grip under the given world-space point, or null if none.
        /// </summary>
        public Grip HitTestGrip(Point worldPoint)
        {
            var vp = this.Viewport;
            if (vp == null) return null;

            Point screenPoint = vp.Project(worldPoint);

            return Visuals.OfType<Grip>().FirstOrDefault(g =>
            {
                Point gp = vp.Project(g.Owner.GetGripPoint(g.Index));
                double halfSize = g.Size / 2.0d + GripHitPadding;
                return System.Math.Abs(gp.X - screenPoint.X) <= halfSize
                    && System.Math.Abs(gp.Y - screenPoint.Y) <= halfSize;
            });
        }

        private void RefreshGripAppearance()
        {
            foreach (var grip in Visuals.OfType<Grip>())
                grip.Render();
        }

        public void UpdateViewportBounds(Size viewportSize)
        {
            Width = viewportSize.Width;
            Height = viewportSize.Height;
        }
    }
}

