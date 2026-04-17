using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public class EntityOffsetService : IEntityOffsetService
    {
        private readonly IReadOnlyList<IEntityOffsetStrategy> strategies;

        public EntityOffsetService(IEnumerable<IEntityOffsetStrategy> strategies)
        {
            this.strategies = (strategies ?? Enumerable.Empty<IEntityOffsetStrategy>())
                .ToList()
                .AsReadOnly();
        }

        public bool CanOffset(Entity entity)
        {
            return ResolveStrategy(entity) != null;
        }

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            var strategy = ResolveStrategy(entity);
            if (strategy == null)
                throw new InvalidOperationException("Offset is not supported for the specified entity.");

            return strategy.CreateOffsetThroughPoint(entity, throughPoint);
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            var strategy = ResolveStrategy(entity);
            if (strategy == null)
                throw new InvalidOperationException("Offset is not supported for the specified entity.");

            return strategy.CreateOffsetByDistance(entity, distance, sidePoint);
        }

        private IEntityOffsetStrategy ResolveStrategy(Entity entity)
        {
            return entity == null ? null : strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
        }
    }
}
