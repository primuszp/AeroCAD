using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public interface IEntityBoundsStrategy
    {
        bool CanHandle(Entity entity);

        Rect GetBounds(Entity entity);
    }
}

