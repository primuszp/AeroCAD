using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.View.Tools;

namespace Primusz.AeroCAD.View.Plugins
{
    public sealed class ViewInteractiveEntityModule : CadModuleBase
    {
        public override string Name => "AeroCAD.ViewInteractiveEntity";
        public override string Version => "1.0.0";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get { yield break; }
        }

        public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
        {
            get
            {
                yield return new InteractiveCommandRegistration(
                    "POLYGON",
                    layerProvider => new PolygonCommandController(layerProvider),
                    aliases: new[] { "POL" },
                    description: "Draw a regular polygon.",
                    policy: new EditorCommandPolicy(CommandSelectionRequirement.None),
                    assignActiveLayer: true,
                    menuGroup: "Draw",
                    menuLabel: "_Polygon");
            }
        }
    }
}
