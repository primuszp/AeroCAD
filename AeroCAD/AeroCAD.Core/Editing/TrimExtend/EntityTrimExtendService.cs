using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class EntityTrimExtendService : IEntityTrimExtendService
    {
        private readonly IReadOnlyList<IEntityTrimExtendStrategy> strategies;

        public EntityTrimExtendService(IEnumerable<IEntityTrimExtendStrategy> strategies)
        {
            this.strategies = (strategies ?? Enumerable.Empty<IEntityTrimExtendStrategy>())
                .ToList()
                .AsReadOnly();
        }

        public bool CanTrim(Entity boundary, Entity target)
        {
            return ResolveTrimStrategy(boundary, target) != null;
        }

        public bool CanExtend(Entity boundary, Entity target)
        {
            return ResolveExtendStrategy(boundary, target) != null;
        }

        public Entity CreateTrimmed(Entity boundary, Entity target, Point pickPoint)
        {
            var strategy = ResolveTrimStrategy(boundary, target);
            if (strategy == null)
                throw new InvalidOperationException("Trim is not supported for the specified entities.");

            return strategy.CreateTrimmed(boundary, target, pickPoint);
        }

        public Entity CreateExtended(Entity boundary, Entity target, Point pickPoint)
        {
            var strategy = ResolveExtendStrategy(boundary, target);
            if (strategy == null)
                throw new InvalidOperationException("Extend is not supported for the specified entities.");

            return strategy.CreateExtended(boundary, target, pickPoint);
        }

        private IEntityTrimExtendStrategy ResolveTrimStrategy(Entity boundary, Entity target)
        {
            return boundary == null || target == null
                ? null
                : strategies.FirstOrDefault(candidate => candidate.CanTrim(boundary, target));
        }

        private IEntityTrimExtendStrategy ResolveExtendStrategy(Entity boundary, Entity target)
        {
            return boundary == null || target == null
                ? null
                : strategies.FirstOrDefault(candidate => candidate.CanExtend(boundary, target));
        }
    }
}
