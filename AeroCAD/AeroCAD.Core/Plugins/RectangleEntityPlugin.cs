using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public class RectangleEntityPlugin : EntityPluginBase
    {
        public override IEntityRenderStrategy RenderStrategy => new RectangleEntityRenderStrategy();
        public override IEntityBoundsStrategy BoundsStrategy => new RectangleBoundsStrategy();
        public override IGripPreviewStrategy GripPreviewStrategy => new RectangleGripPreviewStrategy();
        public override ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => new RectangleSelectionMovePreviewStrategy();
        public override ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => new RectangleTransientEntityPreviewStrategy();

        public override IEnumerable<ITool> CreateTools()
        {
            yield return new RectangleTool();
        }

        public override IEnumerable<EditorCommandDefinition> CreateCommands()
        {
            yield return new EditorCommandDefinition(
                "RECTANGLE",
                new[] { "REC", "RECT" },
                "Draw an axis-aligned rectangle.",
                modalToolType: typeof(RectangleTool),
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Rectangle");
        }
    }
}
