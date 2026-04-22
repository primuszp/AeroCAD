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

        public override IEnumerable<IInteractiveShapeDefinition> Shapes
        {
            get
            {
                yield return new InteractiveShapeDefinition(
                    name: "AeroCAD.Polygon",
                    commandName: "POLYGON",
                    controllerFactory: layerProvider => new PolygonCommandController(layerProvider),
                    steps: new[]
                    {
                        new CommandStep("Sides", "Enter number of sides [3-1024] <4>:"),
                        new CommandStep("Placement", "Specify center point or [Edge]:"),
                        new CommandStep("CenterMode", "Enter an option [Inscribed in circle/Circumscribed about circle] <Inscribed in circle>:"),
                        new CommandStep("Radius", "Specify radius of circle:"),
                        new CommandStep("FirstEdge", "Specify first endpoint of edge:"),
                        new CommandStep("SecondEdge", "Specify second endpoint of edge:")
                    },
                    aliases: new[] { "POL" },
                    description: "Draw a regular polygon.",
                    assignActiveLayer: true,
                    menuGroup: "Draw",
                    menuLabel: "_Polygon");
            }
        }

        public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
        {
            get { yield break; }
        }
    }
}
