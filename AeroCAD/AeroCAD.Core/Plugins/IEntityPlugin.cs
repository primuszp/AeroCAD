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
    /// Bundles all strategies needed to integrate a new entity type into the CAD system.
    /// Implement this interface and call ModelSpaceComposition.RegisterPlugin() to add a new entity.
    /// </summary>
    public interface IEntityPlugin
    {
        IEntityRenderStrategy RenderStrategy { get; }
        IEntityBoundsStrategy BoundsStrategy { get; }
        IGripPreviewStrategy GripPreviewStrategy { get; }
        ISelectionMovePreviewStrategy SelectionMovePreviewStrategy { get; }
        ITransientEntityPreviewStrategy TransientEntityPreviewStrategy { get; }
        IEntityOffsetStrategy OffsetStrategy { get; }
        IEntityTrimExtendStrategy TrimExtendStrategy { get; }
    }
}
