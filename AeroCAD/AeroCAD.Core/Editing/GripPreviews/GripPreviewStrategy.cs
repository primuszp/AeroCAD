using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    /// <summary>
    /// Type-safe base class for custom grip preview strategies.
    /// </summary>
    public abstract class GripPreviewStrategy<TEntity> : IGripPreviewStrategy
        where TEntity : Entity
    {
        public bool CanHandle(Entity entity) => entity is TEntity;

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot preview entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            return CreatePreview(typedEntity, gripIndex, newPosition) ?? GripPreview.Empty;
        }

        protected abstract GripPreview CreatePreview(TEntity entity, int gripIndex, Point newPosition);
    }
}
