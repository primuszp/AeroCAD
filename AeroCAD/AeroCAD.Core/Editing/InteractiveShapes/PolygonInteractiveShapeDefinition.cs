using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class PolygonInteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        private static readonly IReadOnlyList<CommandStep> DefaultSteps = new[]
        {
            new CommandStep("Sides", "Enter number of sides [3-1024] <4>:"),
            new CommandStep("Placement", "Specify center point or [Edge]:"),
            new CommandStep("CenterMode", "Enter an option [Inscribed in circle/Circumscribed about circle] <Inscribed in circle>:"),
            new CommandStep("Radius", "Specify radius of circle:"),
            new CommandStep("FirstEdge", "Specify first endpoint of edge:"),
            new CommandStep("SecondEdge", "Specify second endpoint of edge:")
        };

        public PolygonInteractiveShapeDefinition(System.Func<IInteractiveCommandController> controllerFactory, bool replaceExistingCommand = false)
        {
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Polygon",
                commandName: "POLYGON",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                aliases: new[] { "POL" },
                description: "Draw a regular polygon.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Polygon",
                replaceExistingCommand: replaceExistingCommand);
        }

        public IInteractiveShapePipeline Pipeline { get; }

        public string Name => Pipeline.Name;

        public string CommandName => Pipeline.CommandName;

        public string Description => Pipeline.Description;

        public bool AssignActiveLayer => Pipeline.AssignActiveLayer;

        public string MenuGroup => Pipeline.MenuGroup;

        public string MenuLabel => Pipeline.MenuLabel;

        public CommandStep InitialStep => Pipeline.InitialStep;

        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;

        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
