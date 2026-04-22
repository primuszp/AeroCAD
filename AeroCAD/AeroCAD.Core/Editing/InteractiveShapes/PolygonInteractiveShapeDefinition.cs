using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Layers;
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

        public PolygonInteractiveShapeDefinition(Func<Func<Layer>, IInteractiveCommandController> controllerFactory)
        {
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public string Name => "AeroCAD.Polygon";

        public string CommandName => "POLYGON";

        public CommandStep InitialStep => DefaultSteps.FirstOrDefault();

        public IReadOnlyList<CommandStep> Steps => DefaultSteps;

        public string[] Aliases => new[] { "POL" };

        public string Description => "Draw a regular polygon.";

        public bool AssignActiveLayer => true;

        public string MenuGroup => "Draw";

        public string MenuLabel => "_Polygon";

        public Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return new InteractiveCommandRegistration(
                CommandName,
                ControllerFactory,
                aliases: Aliases,
                description: Description,
                assignActiveLayer: AssignActiveLayer,
                menuGroup: MenuGroup,
                menuLabel: MenuLabel);
        }
    }
}
