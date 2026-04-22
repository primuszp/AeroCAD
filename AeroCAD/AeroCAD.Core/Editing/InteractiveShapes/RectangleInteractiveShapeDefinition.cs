using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class RectangleInteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        private static readonly IReadOnlyList<CommandStep> DefaultSteps = new[]
        {
            new CommandStep("FirstCorner", "Specify first corner:"),
            new CommandStep("OppositeCorner", "Specify opposite corner:")
        };

        public RectangleInteractiveShapeDefinition(Func<Func<Layer>, IInteractiveCommandController> controllerFactory)
        {
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public string Name => "AeroCAD.Rectangle";
        public string CommandName => "RECTANGLE";
        public CommandStep InitialStep => DefaultSteps[0];
        public IReadOnlyList<CommandStep> Steps => DefaultSteps;
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Draw a rectangle.";
        public bool AssignActiveLayer => true;
        public string MenuGroup => "Draw";
        public string MenuLabel => "_Rectangle";
        public Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return new InteractiveCommandRegistration(CommandName, ControllerFactory, aliases: Aliases, description: Description, assignActiveLayer: AssignActiveLayer, menuGroup: MenuGroup, menuLabel: MenuLabel);
        }
    }
}
