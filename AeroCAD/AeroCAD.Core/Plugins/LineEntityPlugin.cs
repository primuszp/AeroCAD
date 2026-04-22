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
    public sealed class LineEntityPlugin : EntityPluginBase
    {
        protected override string PluginName => "AeroCAD.Line";
        protected override EntityPluginCapability Capabilities => EntityPluginCapability.Render | EntityPluginCapability.Bounds | EntityPluginCapability.GripPreview | EntityPluginCapability.SelectionMovePreview | EntityPluginCapability.TransientPreview | EntityPluginCapability.Offset | EntityPluginCapability.TrimExtend | EntityPluginCapability.InteractiveCommand;
        protected override IEntityRenderStrategy RenderStrategy => new LineEntityRenderStrategy();
        protected override IEntityBoundsStrategy BoundsStrategy => new LineBoundsStrategy();
        protected override IGripPreviewStrategy GripPreviewStrategy => new LineGripPreviewStrategy();
        protected override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new LineSelectionMovePreviewStrategy();
        protected override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new LineTransientEntityPreviewStrategy();
        protected override IEntityOffsetStrategy OffsetStrategy => new LineOffsetStrategy();
        protected override IEntityTrimExtendStrategy TrimExtendStrategy => new LineTrimExtendStrategy();

        protected override IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands()
        {
            yield return new InteractiveCommandRegistration(
                "LINE",
                () => new Tools.LineCommandController(),
                aliases: new[] { "L" },
                description: "Draw line segments.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Line");
        }
    }
}
