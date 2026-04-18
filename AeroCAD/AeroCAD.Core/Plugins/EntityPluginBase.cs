using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// Base class for entity plugins. Subclasses only need to override the strategies they support.
    /// RenderStrategy and BoundsStrategy are required; all others default to null (not registered).
    /// </summary>
    public abstract class EntityPluginBase : IEntityPlugin
    {
        public abstract IEntityRenderStrategy RenderStrategy { get; }
        public abstract IEntityBoundsStrategy BoundsStrategy { get; }
        public virtual IGripPreviewStrategy GripPreviewStrategy => null;
        public virtual ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => null;
        public virtual ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => null;
        public virtual IEntityOffsetStrategy OffsetStrategy => null;
        public virtual IEntityTrimExtendStrategy TrimExtendStrategy => null;
    }
}
