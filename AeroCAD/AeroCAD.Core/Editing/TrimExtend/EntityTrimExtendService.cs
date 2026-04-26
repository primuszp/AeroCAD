using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class EntityTrimExtendService : IEntityTrimExtendService
    {
        private readonly IReadOnlyList<IEntityTrimExtendStrategy> strategies;
        private readonly IReadOnlyList<EntityPluginDescriptor> descriptors;

        public EntityTrimExtendService(IEnumerable<IEntityTrimExtendStrategy> strategies)
            : this(strategies, null)
        {
        }

        public EntityTrimExtendService(IEnumerable<IEntityTrimExtendStrategy> strategies, IEnumerable<EntityPluginDescriptor> descriptors)
        {
            this.strategies = (strategies ?? Enumerable.Empty<IEntityTrimExtendStrategy>())
                .ToList()
                .AsReadOnly();
            this.descriptors = (descriptors ?? Enumerable.Empty<EntityPluginDescriptor>())
                .Where(descriptor => descriptor?.TrimExtendStrategy != null)
                .ToList()
                .AsReadOnly();
        }

        public bool CanUseAsBoundary(Entity entity)
        {
            return entity != null && descriptors.Any(descriptor =>
                (descriptor.BoundsStrategy?.CanHandle(entity) ?? false) ||
                (descriptor.RenderStrategy?.CanHandle(entity) ?? false));
        }

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return ResolveTrimStrategy(boundaries, target) != null;
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return ResolveExtendStrategy(boundaries, target) != null;
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var strategy = ResolveTrimStrategy(boundaries, target);
            if (strategy == null)
                throw new InvalidOperationException("Trim is not supported for the specified entities.");

            return strategy.CreateTrimmed(boundaries, target, pickPoint);
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var strategy = ResolveExtendStrategy(boundaries, target);
            if (strategy == null)
                throw new InvalidOperationException("Extend is not supported for the specified entities.");

            return strategy.CreateExtended(boundaries, target, pickPoint);
        }

        private IEntityTrimExtendStrategy ResolveTrimStrategy(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return boundaries == null || boundaries.Count == 0 || target == null
                ? null
                : strategies.FirstOrDefault(candidate => candidate.CanTrim(boundaries, target));
        }

        private IEntityTrimExtendStrategy ResolveExtendStrategy(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return boundaries == null || boundaries.Count == 0 || target == null
                ? null
                : strategies.FirstOrDefault(candidate => candidate.CanExtend(boundaries, target));
        }
    }
}
