using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class CircleInteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        private static readonly IReadOnlyList<CommandStep> DefaultSteps = new[]
        {
            new CommandStep("CenterPoint", "Specify center point:"),
            new CommandStep("RadiusPoint", "Specify radius:", keywords: new[] { new CommandKeywordOption("DIAMETER", new[] { "D" }, "Switch to diameter input.") }),
            new CommandStep("DiameterPoint", "Specify diameter:")
        };

        public CircleInteractiveShapeDefinition(Func<Func<Layer>, IInteractiveCommandController> controllerFactory)
        {
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public string Name => "AeroCAD.Circle";
        public string CommandName => "CIRCLE";
        public CommandStep InitialStep => DefaultSteps[0];
        public IReadOnlyList<CommandStep> Steps => DefaultSteps;
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Draw a circle.";
        public bool AssignActiveLayer => true;
        public string MenuGroup => "Draw";
        public string MenuLabel => "_Circle";
        public Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return new InteractiveCommandRegistration(CommandName, ControllerFactory, aliases: Aliases, description: Description, assignActiveLayer: AssignActiveLayer, menuGroup: MenuGroup, menuLabel: MenuLabel);
        }
    }
}
