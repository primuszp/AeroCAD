using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public abstract class Entity : ISelectable, ISnappable
    {
        #region Members

        private double scale = 1.0;
        private double thickness = 1.0;

        #endregion

        #region Properties

        /// <summary>
        /// Viewport zoom-corrected scale factor, propagated by the Layer host.
        /// Used by the rendering system to compute screen-constant pen widths.
        /// </summary>
        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                Render();
            }
        }

        public double Thickness
        {
            get { return thickness; }
            set
            {
                thickness = value;
                Render();
            }
        }

        public bool IsSelected { get; private set; }

        public EntityCommandHighlightKind CommandHighlight { get; private set; }

        public bool HasVisualHighlight => IsSelected || CommandHighlight != EntityCommandHighlightKind.None;

        public Guid Id { get; protected set; }

        internal IEntityRenderHost RenderHost { get; set; }

        public event EventHandler GeometryChanged;

        /// <summary>
        /// Number of grip points on this entity.
        /// </summary>
        public abstract int GripCount { get; }

        #endregion

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Requests a redraw of the entity through the active render host.
        /// </summary>
        public void Render()
        {
            RenderHost?.RenderEntity(this);
        }

        protected void InvalidateGeometry()
        {
            GeometryChanged?.Invoke(this, EventArgs.Empty);
            Render();
        }

        /// <summary>
        /// Get grip point by index.
        /// </summary>
        public abstract Point GetGripPoint(int index);

        /// <summary>
        /// Moves the grip point at the specified index to a new position.
        /// </summary>
        public abstract void MoveGrip(int index, Point newPosition);

        /// <summary>
        /// Returns the semantic type of the grip at the specified index.
        /// </summary>
        public virtual GripKind GetGripKind(int index)
        {
            return GripKind.Endpoint;
        }

        protected void CopyIdentityTo(Entity clone)
        {
            clone.Id = Id;
        }

        /// <summary>
        /// Creates a deep copy of the entity for Undo/Redo purposes.
        /// </summary>
        public abstract Entity Clone();

        /// <summary>
        /// Creates a new entity instance with duplicated geometry and style, but a new identity.
        /// </summary>
        public abstract Entity Duplicate();

        /// <summary>
        /// Restores the properties of this entity from a previously cloned state.
        /// </summary>
        public abstract void RestoreState(Entity sourceState);

        /// <summary>
        /// Translates the entity by the specified world-space delta.
        /// </summary>
        public abstract void Translate(Vector delta);

        /// <summary>
        /// Gets the snap descriptors exposed by this entity.
        /// </summary>
        public abstract IEnumerable<ISnapDescriptor> GetSnapDescriptors();

        #region ISelectable

        public void Select()
        {
            IsSelected = true;
            Render();
        }

        public void Unselect()
        {
            IsSelected = false;
            Render();
        }

        public void SetCommandHighlight(EntityCommandHighlightKind highlightKind)
        {
            if (CommandHighlight == highlightKind)
                return;

            CommandHighlight = highlightKind;
            Render();
        }

        public void ClearCommandHighlight()
        {
            SetCommandHighlight(EntityCommandHighlightKind.None);
        }

        #endregion
    }
}

