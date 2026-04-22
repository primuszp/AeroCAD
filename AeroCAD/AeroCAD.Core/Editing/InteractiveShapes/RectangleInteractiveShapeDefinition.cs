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

        public RectangleInteractiveShapeDefinition(Func<IInteractiveCommandController> controllerFactory)
        {
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Rectangle",
                commandName: "RECTANGLE",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                description: "Draw a rectangle.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Rectangle");
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;
        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
