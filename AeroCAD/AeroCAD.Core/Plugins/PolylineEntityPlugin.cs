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
    public sealed class PolylineEntityPlugin : EntityPluginBase
    {
        protected override string PluginName => "AeroCAD.Polyline";
        protected override EntityPluginCapability Capabilities => EntityPluginCapability.Render | EntityPluginCapability.Bounds | EntityPluginCapability.GripPreview | EntityPluginCapability.SelectionMovePreview | EntityPluginCapability.TransientPreview | EntityPluginCapability.Offset | EntityPluginCapability.TrimExtend | EntityPluginCapability.InteractiveCommand;
        protected override IEntityRenderStrategy RenderStrategy => new PolylineEntityRenderStrategy();
        protected override IEntityBoundsStrategy BoundsStrategy => new PolylineBoundsStrategy();
        protected override IGripPreviewStrategy GripPreviewStrategy => new PolylineGripPreviewStrategy();
        protected override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new PolylineSelectionMovePreviewStrategy();
        protected override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new PolylineTransientEntityPreviewStrategy();
        protected override IEntityOffsetStrategy OffsetStrategy => new PolylineOffsetStrategy();
        protected override IEntityTrimExtendStrategy TrimExtendStrategy => new PolylineTrimExtendStrategy();

        protected override IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands()
        {
            yield return new InteractiveCommandRegistration(
                "PLINE",
                () => new Tools.PolylineCommandController(),
                aliases: new[] { "PL", "P" },
                description: "Draw polyline.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Polyline");
        }
    }
}
