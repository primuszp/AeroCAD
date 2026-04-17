using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public interface IEntityOffsetStrategy
    {
        bool CanHandle(Entity entity);

        Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint);

        Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint);
    }
}
