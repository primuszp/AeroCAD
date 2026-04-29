namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    /// <summary>
    /// SDK-facing base class for custom entities that standardizes the entity lifecycle.
    /// It preserves the engine contract that Clone keeps identity, Duplicate creates
    /// a new identity, and RestoreState restores geometry plus base visual properties.
    /// </summary>
    public abstract class CustomEntityBase : Entity
    {
        public sealed override Entity Clone()
        {
            var clone = CreateInstanceCore();
            CopyGeometryTo(clone);
            CopyIdentityTo(clone);
            CopyStyleTo(clone);
            return clone;
        }

        public sealed override Entity Duplicate()
        {
            var duplicate = CreateInstanceCore();
            CopyGeometryTo(duplicate);
            CopyStyleTo(duplicate);
            return duplicate;
        }

        public sealed override void RestoreState(Entity sourceState)
        {
            if (sourceState == null || sourceState.GetType() != GetType() || sourceState is not CustomEntityBase source)
                return;

            CopyGeometryFrom(source);
            RestoreBaseFrom(source);
            InvalidateEntityGeometry();
        }

        /// <summary>
        /// Invalidates derived geometry, cached preview data, and the rendered visual.
        /// Custom entities should call this after geometry state changes.
        /// </summary>
        protected void InvalidateEntityGeometry()
        {
            OnGeometryInvalidated();
            InvalidateGeometry();
        }

        /// <summary>
        /// Creates an empty instance of the same concrete entity type.
        /// </summary>
        protected abstract CustomEntityBase CreateInstanceCore();

        /// <summary>
        /// Copies only geometry/domain state into the target entity. Base visual
        /// properties and identity are handled by the lifecycle base class.
        /// </summary>
        protected abstract void CopyGeometryTo(CustomEntityBase target);

        /// <summary>
        /// Restores only geometry/domain state from the source entity.
        /// </summary>
        protected abstract void CopyGeometryFrom(CustomEntityBase source);

        /// <summary>
        /// Hook for clearing entity-specific cached geometry before the engine redraws.
        /// </summary>
        protected virtual void OnGeometryInvalidated()
        {
        }

        protected virtual void CopyStyleTo(CustomEntityBase target)
        {
            target.Thickness = Thickness;
            target.Color = Color;
        }
    }
}
