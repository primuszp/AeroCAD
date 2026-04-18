using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Plugins
{
    public class RectangleEntityPlugin : EntityPluginBase
    {
        public override IEntityRenderStrategy RenderStrategy => new RectangleEntityRenderStrategy();
        public override IEntityBoundsStrategy BoundsStrategy => new RectangleBoundsStrategy();
        public override IGripPreviewStrategy GripPreviewStrategy => new RectangleGripPreviewStrategy();
        public override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new RectangleSelectionMovePreviewStrategy();
        public override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new RectangleTransientEntityPreviewStrategy();
    }
}
