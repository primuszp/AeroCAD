using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public interface ISnapEngine
    {
        /// <summary>Snap tolerance in world units (updated when zoom changes).</summary>
        double ToleranceWorld { get; set; }

        ISnapModePolicy ModePolicy { get; }

        /// <summary>The last computed snap result, or null if no snap found.</summary>
        SnapResult CurrentSnap { get; }

        /// <summary>Updates the snap state for the given world position and candidate entities.</summary>
        void Update(Point worldPos, IEnumerable<Entity> candidates);

        /// <summary>Updates the snap state for the given world position and explicit descriptors.</summary>
        void Update(Point worldPos, IEnumerable<ISnapDescriptor> descriptors);

        /// <summary>Returns the snapped point, or rawPos if no snap found.</summary>
        Point Snap(Point rawPos);
    }
}

