using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class EntityBoundsService : IEntityBoundsService
    {
        private readonly IReadOnlyList<IEntityBoundsStrategy> strategies;

        public EntityBoundsService(IEnumerable<IEntityBoundsStrategy> strategies)
        {
            this.strategies = strategies?.ToList() ?? throw new ArgumentNullException(nameof(strategies));
        }

        public bool TryGetBounds(Entity entity, out Rect bounds)
        {
            var strategy = strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
            if (strategy == null)
            {
                bounds = Rect.Empty;
                return false;
            }

            bounds = strategy.GetBounds(entity);
            return !bounds.IsEmpty;
        }
    }
}

