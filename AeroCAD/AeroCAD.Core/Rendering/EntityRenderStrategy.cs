using System;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    /// <summary>
    /// Type-safe base class for entity render strategies.
    /// External plugins can override the typed Render overload and avoid repeated
    /// CanHandle/cast boilerplate in every strategy.
    /// </summary>
    public abstract class EntityRenderStrategy<TEntity> : IEntityRenderStrategy
        where TEntity : Entity
    {
        public bool CanHandle(Entity entity) => entity is TEntity;

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot render entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            Render(typedEntity, drawingContext, context);
        }

        protected abstract void Render(TEntity entity, DrawingContext drawingContext, EntityRenderContext context);
    }
}
