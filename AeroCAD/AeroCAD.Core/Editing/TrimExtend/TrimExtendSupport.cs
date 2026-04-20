using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    internal static class TrimExtendSupport
    {
        public static bool IsSupportedBoundary(Entity boundary)
        {
            return boundary is Line || boundary is Circle || boundary is Polyline || boundary is Arc || boundary is Rectangle;
        }

        public static IEnumerable<Entity> GetSupportedBoundaries(IReadOnlyList<Entity> boundaries)
        {
            return boundaries.Where(IsSupportedBoundary);
        }
    }
}
