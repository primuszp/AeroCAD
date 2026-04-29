using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    /// <summary>
    /// Type-safe base class for selection move preview strategies.
    /// </summary>
    public abstract class SelectionMovePreviewStrategy<TEntity> : ISelectionMovePreviewStrategy
        where TEntity : Entity
    {
        public bool CanHandle(Entity entity) => entity is TEntity;

        public GripPreview CreatePreview(Entity entity, Vector displacement)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot preview entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            return CreatePreview(typedEntity, displacement) ?? GripPreview.Empty;
        }

        protected abstract GripPreview CreatePreview(TEntity entity, Vector displacement);
    }
}
