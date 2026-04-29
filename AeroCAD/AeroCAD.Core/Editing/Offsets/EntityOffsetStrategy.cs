using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    /// <summary>
    /// Type-safe base class for entity offset strategies.
    /// </summary>
    public abstract class EntityOffsetStrategy<TEntity> : IEntityOffsetStrategy
        where TEntity : Entity
    {
        public bool CanHandle(Entity entity) => entity is TEntity;

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot offset entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            return CreateOffsetThroughPoint(typedEntity, throughPoint);
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot offset entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            return CreateOffsetByDistance(typedEntity, distance, sidePoint);
        }

        protected abstract Entity CreateOffsetThroughPoint(TEntity entity, Point throughPoint);

        protected abstract Entity CreateOffsetByDistance(TEntity entity, double distance, Point sidePoint);
    }
}
