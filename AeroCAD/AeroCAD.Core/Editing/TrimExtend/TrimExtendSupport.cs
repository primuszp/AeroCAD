using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    internal static class TrimExtendSupport
    {
        public static bool IsSupportedBoundary(Entity boundary)
        {
            return boundary is Line || boundary is Circle || boundary is Polyline || boundary is Arc || boundary is Rectangle;
        }
    }
}
