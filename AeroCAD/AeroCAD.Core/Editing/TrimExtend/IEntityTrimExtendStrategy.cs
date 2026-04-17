using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public interface IEntityTrimExtendStrategy
    {
        bool CanTrim(Entity boundary, Entity target);

        bool CanExtend(Entity boundary, Entity target);

        Entity CreateTrimmed(Entity boundary, Entity target, Point pickPoint);

        Entity CreateExtended(Entity boundary, Entity target, Point pickPoint);
    }
}
