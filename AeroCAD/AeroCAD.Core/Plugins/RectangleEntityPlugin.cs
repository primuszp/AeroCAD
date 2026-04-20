using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Plugins
{
    public class RectangleEntityPlugin : EntityPluginBase
    {
        protected override string PluginName => "AeroCAD.Rectangle";
        protected override EntityPluginCapability Capabilities => EntityPluginCapability.Render | EntityPluginCapability.Bounds | EntityPluginCapability.GripPreview | EntityPluginCapability.SelectionMovePreview | EntityPluginCapability.TransientPreview | EntityPluginCapability.Offset | EntityPluginCapability.TrimExtend | EntityPluginCapability.InteractiveCommand;
        protected override IEntityRenderStrategy RenderStrategy => new RectangleEntityRenderStrategy();
        protected override IEntityBoundsStrategy BoundsStrategy => new RectangleBoundsStrategy();
        protected override IGripPreviewStrategy GripPreviewStrategy => new RectangleGripPreviewStrategy();
        protected override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new RectangleSelectionMovePreviewStrategy();
        protected override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new RectangleTransientEntityPreviewStrategy();
        protected override IEntityOffsetStrategy OffsetStrategy => new RectangleOffsetStrategy();
        protected override IEntityTrimExtendStrategy TrimExtendStrategy => new RectangleTrimExtendStrategy();

        protected override IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands()
        {
            yield return new InteractiveCommandRegistration(
                "RECTANGLE",
                layerProvider => new Tools.RectangleCommandController(layerProvider),
                aliases: new[] { "REC", "RECT" },
                description: "Draw an axis-aligned rectangle.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Rectangle");
        }
    }
}
