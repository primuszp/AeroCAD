using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public interface ISpatialQueryService
    {
        IReadOnlyCollection<Entity> QueryNearby(Point point, double radius);

        IReadOnlyCollection<Entity> QueryIntersecting(Rect rect);
    }
}

