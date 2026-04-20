using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    /// <summary>
    /// Converts Rectangle to a closed Polyline (4 sides) and delegates trim to PolylineTrimExtendStrategy.
    /// The result is always a Polyline since trimming a rectangle removes a corner or a side segment.
    /// </summary>
    public class RectangleTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private static readonly PolylineTrimExtendStrategy PolylineStrategy = new PolylineTrimExtendStrategy();

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Rectangle && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return false;
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var rect = target as Rectangle;
            if (rect == null)
                return Array.Empty<Entity>();

            var polyline = ToClosedPolyline(rect);
            return PolylineStrategy.CreateTrimmed(boundaries, polyline, pickPoint);
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            return Array.Empty<Entity>();
        }

        private static Polyline ToClosedPolyline(Rectangle rect)
        {
            var tl = rect.TopLeft;
            var br = rect.BottomRight;
            var tr = new Point(br.X, tl.Y);
            var bl = new Point(tl.X, br.Y);

            return new Polyline(new[] { tl, tr, br, bl, tl })
            {
                Thickness = rect.Thickness
            };
        }

    }
}
