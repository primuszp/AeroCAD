using System;
using System.Windows;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface IOrthoService
    {
        bool IsEnabled { get; }

        void Toggle();

        /// <summary>
        /// Projects rawPoint onto the closest orthogonal axis (horizontal or vertical) from basePoint.
        /// Returns rawPoint unchanged if ortho is disabled.
        /// </summary>
        Point Apply(Point basePoint, Point rawPoint);

        event EventHandler StateChanged;
    }
}

