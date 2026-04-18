using System.Collections.Generic;
using System.Windows;

namespace Primusz.AeroCAD.Core.Drawing.Handles
{
    public interface IGripService
    {
        IReadOnlyList<GripDescriptor> GetSelectedGrips();

        GripDescriptor FindSnapCandidate(Point worldPoint, double toleranceWorld);
    }
}
