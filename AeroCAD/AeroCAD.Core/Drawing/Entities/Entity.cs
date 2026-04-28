using System;
using System.Collections.Generic;
using System.Linq;
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
        private EntityColor color = EntityColor.ByLayer;

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

        /// <summary>
        /// Entity color. Default is <see cref="EntityColor.ByLayer"/> — inherits the layer color at render time.
        /// Set to an ACI index or explicit RGB to override the layer color for this entity.
        /// </summary>
        public EntityColor Color
        {
            get => color;
            set
            {
                color = value;
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

        /// <summary>
        /// Returns grip descriptors for this entity. Override when grip semantics need
        /// stable, explicit descriptors beyond simple index-based defaults.
        /// </summary>
        public virtual IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            return Enumerable.Range(0, GripCount)
                .Select(index => new GripDescriptor(this, index, GetGripKind(index), () => GetGripPoint(index)));
        }

        /// <summary>
        /// Returns lightweight preview geometry for generic MOVE/COPY/GRIP feedback.
        /// Override this in external entities when grip-derived preview does not represent the entity shape well.
        /// </summary>
        public virtual System.Windows.Media.Geometry GetPreviewGeometry()
        {
            var points = GetGripDescriptors()
                .Select(grip => grip.GetPoint())
                .ToList();

            if (points.Count == 0)
                return null;

            var group = new System.Windows.Media.GeometryGroup();
            const double markerRadius = 1.75d;
            foreach (var point in points)
                group.Children.Add(new System.Windows.Media.EllipseGeometry(point, markerRadius, markerRadius));

            if (points.Count > 1)
            {
                for (int index = 0; index < points.Count - 1; index++)
                    group.Children.Add(new System.Windows.Media.LineGeometry(points[index], points[index + 1]));
            }

            if (group.CanFreeze)
                group.Freeze();

            return group;
        }

        protected void CopyIdentityTo(Entity clone)
        {
            clone.Id = Id;
            clone.color = color;
            clone.thickness = thickness;
        }

        /// <summary>
        /// Copies base visual properties (Color, Thickness) from <paramref name="source"/>.
        /// Call this in <see cref="RestoreState"/> implementations.
        /// </summary>
        protected void RestoreBaseFrom(Entity source)
        {
            color = source.color;
            thickness = source.thickness;
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
        /// Grip-backed snap points are derived from the grip descriptor model by default,
        /// while entity-specific non-grip snaps can be supplied by overrides.
        /// </summary>
        public virtual IEnumerable<ISnapDescriptor> GetSnapDescriptors()
        {
            foreach (var descriptor in GetGripSnapDescriptors())
                yield return descriptor;

            foreach (var descriptor in GetAdditionalSnapDescriptors())
                yield return descriptor;
        }

        protected virtual IEnumerable<ISnapDescriptor> GetGripSnapDescriptors()
        {
            foreach (var grip in GetGripDescriptors())
            {
                if (!TryMapGripToSnapType(grip.Kind, out var snapType))
                    continue;

                yield return new SnapPointDescriptor(snapType, grip.GetPoint, grip.Owner, grip.Index);
            }
        }

        protected virtual IEnumerable<ISnapDescriptor> GetAdditionalSnapDescriptors()
        {
            yield break;
        }

        protected virtual bool TryMapGripToSnapType(GripKind gripKind, out SnapType snapType)
        {
            switch (gripKind)
            {
                case GripKind.Center:
                    snapType = SnapType.Center;
                    return true;
                case GripKind.Midpoint:
                    snapType = SnapType.Midpoint;
                    return true;
                case GripKind.Quadrant:
                    snapType = SnapType.Quadrant;
                    return true;
                case GripKind.Endpoint:
                    snapType = SnapType.Endpoint;
                    return true;
                case GripKind.Node:
                    snapType = SnapType.Node;
                    return true;
                default:
                    snapType = default;
                    return false;
            }
        }

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

