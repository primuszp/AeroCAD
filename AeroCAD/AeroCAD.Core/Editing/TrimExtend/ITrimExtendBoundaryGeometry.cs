using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public interface ITrimExtendBoundaryGeometry
    {
        IReadOnlyList<LineIntersectionPoint> GetLineIntersections(Line target, bool restrictTargetToSegment);

        IReadOnlyList<CircularIntersectionPoint> GetCircularIntersections(Point center, double radius);
    }
}
