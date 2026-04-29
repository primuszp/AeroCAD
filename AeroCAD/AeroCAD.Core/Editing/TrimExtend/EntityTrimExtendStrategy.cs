using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    /// <summary>
    /// Type-safe base class for trim/extend strategies where the edited target has
    /// a known entity type. Boundaries remain untyped so built-in and external
    /// boundary entities can participate together.
    /// </summary>
    public abstract class EntityTrimExtendStrategy<TEntity> : IEntityTrimExtendStrategy
        where TEntity : Entity
    {
        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is TEntity typedTarget && CanTrim(boundaries, typedTarget);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is TEntity typedTarget && CanExtend(boundaries, typedTarget);
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            if (target is not TEntity typedTarget)
                throw new ArgumentException($"Strategy {GetType().Name} cannot trim entity type {target?.GetType().Name ?? "<null>"}.", nameof(target));

            return CreateTrimmed(boundaries, typedTarget, pickPoint);
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            if (target is not TEntity typedTarget)
                throw new ArgumentException($"Strategy {GetType().Name} cannot extend entity type {target?.GetType().Name ?? "<null>"}.", nameof(target));

            return CreateExtended(boundaries, typedTarget, pickPoint);
        }

        protected abstract bool CanTrim(IReadOnlyList<Entity> boundaries, TEntity target);

        protected abstract bool CanExtend(IReadOnlyList<Entity> boundaries, TEntity target);

        protected abstract IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, TEntity target, Point pickPoint);

        protected abstract IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, TEntity target, Point pickPoint);
    }
}
