using System;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    /// <summary>
    /// Type-safe base class for transient entity preview strategies.
    /// </summary>
    public abstract class TransientEntityPreviewStrategy<TEntity> : ITransientEntityPreviewStrategy
        where TEntity : Entity
    {
        public bool CanHandle(Entity entity) => entity is TEntity;

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            if (entity is not TEntity typedEntity)
                throw new ArgumentException($"Strategy {GetType().Name} cannot preview entity type {entity?.GetType().Name ?? "<null>"}.", nameof(entity));

            return CreatePreview(typedEntity, color) ?? GripPreview.Empty;
        }

        protected abstract GripPreview CreatePreview(TEntity entity, Color color);
    }
}
