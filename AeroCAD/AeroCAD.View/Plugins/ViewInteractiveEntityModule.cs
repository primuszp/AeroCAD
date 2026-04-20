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
                    "DIAMOND",
                    layerProvider => new DiamondCommandController(layerProvider),
                    aliases: new[] { "DM" },
                    description: "Draw a diamond centered on a picked point.",
                    policy: new EditorCommandPolicy(CommandSelectionRequirement.None),
                    assignActiveLayer: true,
                    menuGroup: "Draw",
                    menuLabel: "_Diamond");
            }
        }
    }
}
