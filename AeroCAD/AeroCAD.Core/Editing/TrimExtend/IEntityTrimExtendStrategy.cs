using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public interface IEntityTrimExtendStrategy
    {
        bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target);

        bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target);

        Entity CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint);

        Entity CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint);
    }
}
