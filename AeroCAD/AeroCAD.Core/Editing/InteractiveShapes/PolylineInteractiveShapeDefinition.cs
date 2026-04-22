using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class PolylineInteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        private static readonly IReadOnlyList<CommandStep> DefaultSteps = new[]
        {
            new CommandStep("FirstPoint", "Specify start point:"),
            new CommandStep("NextPoint", "Specify next point:", keywords: new[]
            {
                new CommandKeywordOption("CLOSE", new[] { "C" }, "Close the polyline."),
                new CommandKeywordOption("UNDO", new[] { "U" }, "Undo last point.")
            })
        };

        public PolylineInteractiveShapeDefinition(Func<Func<Layer>, IInteractiveCommandController> controllerFactory)
        {
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public string Name => "AeroCAD.Polyline";
        public string CommandName => "PLINE";
        public CommandStep InitialStep => DefaultSteps[0];
        public IReadOnlyList<CommandStep> Steps => DefaultSteps;
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Draw a polyline.";
        public bool AssignActiveLayer => true;
        public string MenuGroup => "Draw";
        public string MenuLabel => "_Polyline";
        public Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return new InteractiveCommandRegistration(CommandName, ControllerFactory, aliases: Aliases, description: Description, assignActiveLayer: AssignActiveLayer, menuGroup: MenuGroup, menuLabel: MenuLabel);
        }
    }
}
