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
    public sealed class CircleEntityPlugin : EntityPluginBase
    {
        protected override string PluginName => "AeroCAD.Circle";
        protected override EntityPluginCapability Capabilities => EntityPluginCapability.Render | EntityPluginCapability.Bounds | EntityPluginCapability.GripPreview | EntityPluginCapability.SelectionMovePreview | EntityPluginCapability.TransientPreview | EntityPluginCapability.Offset | EntityPluginCapability.TrimExtend | EntityPluginCapability.InteractiveCommand;
        protected override IEntityRenderStrategy RenderStrategy => new CircleEntityRenderStrategy();
        protected override IEntityBoundsStrategy BoundsStrategy => new CircleBoundsStrategy();
        protected override IGripPreviewStrategy GripPreviewStrategy => new CircleGripPreviewStrategy();
        protected override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new CircleSelectionMovePreviewStrategy();
        protected override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new CircleTransientEntityPreviewStrategy();
        protected override IEntityOffsetStrategy OffsetStrategy => new CircleOffsetStrategy();
        protected override IEntityTrimExtendStrategy TrimExtendStrategy => new CircleTrimExtendStrategy();

        protected override IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands()
        {
            yield return new InteractiveCommandRegistration(
                "CIRCLE",
                layerProvider => new Tools.CircleCommandController(layerProvider),
                aliases: new[] { "C", "CI", "CIR" },
                description: "Draw circles.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Circle");
        }
    }
}
