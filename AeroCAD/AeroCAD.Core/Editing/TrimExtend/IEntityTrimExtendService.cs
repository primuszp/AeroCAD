using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public interface IEntityTrimExtendService
    {
        bool CanUseAsBoundary(Entity entity) => false;

        bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target);

        bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target);

        IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint);

        IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint);
    }
}
