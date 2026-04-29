using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    /// <summary>
    /// Type-safe base class for entity bounds strategies.
    /// </summary>
    public abstract class EntityBoundsStrategy<TEntity> : IEntityBoundsStrategy
        where TEntity : Entity
    {
        public bool CanHandle(Entity entity) => entity is TEntity;

        public Rect GetBounds(Entity entity)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot calculate bounds for entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            return GetBounds(typedEntity);
        }

        protected abstract Rect GetBounds(TEntity entity);
    }
}
