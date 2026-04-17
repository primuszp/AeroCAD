using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Selection
{
    public interface IPickResolutionService
    {
        Entity ResolvePrimary(IEnumerable<Entity> hits);

        Entity ResolvePrimary(IEnumerable<Entity> hits, Func<Entity, bool> predicate);
    }
}
