using System;
using System.Windows;

namespace Primusz.AeroCAD.Core.Editor
{
    public class OrthoService : IOrthoService
    {
        public bool IsEnabled { get; private set; }

        public event EventHandler StateChanged;

        public void Toggle()
        {
            IsEnabled = !IsEnabled;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// If ortho is enabled, projects rawPoint onto the closest orthogonal axis
        /// (horizontal or vertical) relative to basePoint.
        /// If ortho is disabled, returns rawPoint unchanged.
        /// </summary>
        public Point Apply(Point basePoint, Point rawPoint)
        {
            if (!IsEnabled)
                return rawPoint;

            double dx = rawPoint.X - basePoint.X;
            double dy = rawPoint.Y - basePoint.Y;

            // Lock to whichever axis has the larger displacement
            if (Math.Abs(dx) >= Math.Abs(dy))
                return new Point(rawPoint.X, basePoint.Y); // horizontal
            else
                return new Point(basePoint.X, rawPoint.Y); // vertical
        }
    }
}

