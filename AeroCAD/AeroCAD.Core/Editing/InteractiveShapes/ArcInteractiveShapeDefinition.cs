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

        public ArcInteractiveShapeDefinition(Func<IInteractiveCommandController> controllerFactory)
        {
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Arc",
                commandName: "ARC",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                description: "Draw an arc.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Arc");
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;
        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
