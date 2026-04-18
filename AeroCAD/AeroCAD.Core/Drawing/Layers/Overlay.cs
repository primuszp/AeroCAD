using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Drawing.Markers;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class Overlay : VisualHost, IViewportSpaceElement, IViewportHostedElement
    {
        private const double GripHitPadding = 3.0d;
        private readonly IMarkerAppearanceService appearanceService;
        private readonly IGripService gripService;

        public ViewportCoordinateSpace CoordinateSpace => ViewportCoordinateSpace.Screen;

        #region Constructors

        public Overlay(Viewport viewport, IMarkerAppearanceService appearanceService, IGripService gripService)
        {
            this.appearanceService = appearanceService;
            this.gripService = gripService;
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

            var selectedGrips = gripService?.GetSelectedGrips() ?? new List<GripDescriptor>();

            // Remove grips whose owners are no longer selected
            var gripsToRemove = Visuals.OfType<Grip>()
                .Where(g => !selectedGrips.Any(descriptor => Equals(descriptor.Owner, g.Owner) && descriptor.Index == g.Index))
                .ToList();
            foreach (var g in gripsToRemove)
                Visuals.Remove(g);

            // Add grips for newly selected entities
            foreach (var gripDescriptor in selectedGrips)
            {
                bool hasGrip = Visuals.OfType<Grip>().Any(g => Equals(g.Owner, gripDescriptor.Owner) && g.Index == gripDescriptor.Index);
                if (!hasGrip)
                    Visuals.Add(new Grip(gripDescriptor.Owner, gripDescriptor.Index, appearanceService));
            }
            foreach (var grip in Visuals.OfType<Grip>())
                grip.Render();
        }

        /// <summary>
        /// Returns the grip under the given world-space point, or null if none.
        /// </summary>
        public Grip HitTestGrip(Point worldPoint)
        {
            var vp = this.Viewport;
            if (vp == null || gripService == null) return null;

            Point screenPoint = vp.Project(worldPoint);
            var hitDescriptor = gripService.GetSelectedGrips().FirstOrDefault(grip =>
            {
                Point gp = vp.Project(grip.GetPoint());
                double halfSize = (appearanceService?.MarkerSize ?? 10.0d) / 2.0d + GripHitPadding;
                return System.Math.Abs(gp.X - screenPoint.X) <= halfSize
                    && System.Math.Abs(gp.Y - screenPoint.Y) <= halfSize;
            });

            if (hitDescriptor == null)
                return null;

            return Visuals.OfType<Grip>()
                .FirstOrDefault(g => Equals(g.Owner, hitDescriptor.Owner) && g.Index == hitDescriptor.Index);
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

