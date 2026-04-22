using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class ArcInteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        private static readonly IReadOnlyList<CommandStep> DefaultSteps = new[]
        {
            new CommandStep("StartPoint", "Specify start point:"),
            new CommandStep("SecondPoint", "Specify second point:"),
            new CommandStep("EndPoint", "Specify end point:")
        };

        public ArcInteractiveShapeDefinition(Func<Func<Layer>, IInteractiveCommandController> controllerFactory)
        {
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public string Name => "AeroCAD.Arc";
        public string CommandName => "ARC";
        public CommandStep InitialStep => DefaultSteps[0];
        public IReadOnlyList<CommandStep> Steps => DefaultSteps;
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Draw an arc.";
        public bool AssignActiveLayer => true;
        public string MenuGroup => "Draw";
        public string MenuLabel => "_Arc";
        public Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return new InteractiveCommandRegistration(CommandName, ControllerFactory, aliases: Aliases, description: Description, assignActiveLayer: AssignActiveLayer, menuGroup: MenuGroup, menuLabel: MenuLabel);
        }
    }
}
