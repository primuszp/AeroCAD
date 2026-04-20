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
    public sealed class ArcEntityPlugin : EntityPluginBase
    {
        protected override string PluginName => "AeroCAD.Arc";
        protected override EntityPluginCapability Capabilities => EntityPluginCapability.Render | EntityPluginCapability.Bounds | EntityPluginCapability.GripPreview | EntityPluginCapability.SelectionMovePreview | EntityPluginCapability.TransientPreview | EntityPluginCapability.Offset | EntityPluginCapability.TrimExtend | EntityPluginCapability.InteractiveCommand;
        protected override IEntityRenderStrategy RenderStrategy => new ArcEntityRenderStrategy();
        protected override IEntityBoundsStrategy BoundsStrategy => new ArcBoundsStrategy();
        protected override IGripPreviewStrategy GripPreviewStrategy => new ArcGripPreviewStrategy();
        protected override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new ArcSelectionMovePreviewStrategy();
        protected override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new ArcTransientEntityPreviewStrategy();
        protected override IEntityOffsetStrategy OffsetStrategy => new ArcOffsetStrategy();
        protected override IEntityTrimExtendStrategy TrimExtendStrategy => new ArcTrimExtendStrategy();

        protected override IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands()
        {
            yield return new InteractiveCommandRegistration(
                "ARC",
                layerProvider => new Tools.ArcCommandController(layerProvider),
                aliases: new[] { "A", "AR" },
                description: "Draw a 3-point arc.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Arc");
        }
    }
}
