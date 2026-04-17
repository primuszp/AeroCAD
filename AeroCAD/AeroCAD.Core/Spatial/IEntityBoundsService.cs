using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public interface IEntityBoundsService
    {
        bool TryGetBounds(Entity entity, out Rect bounds);
    }
}

