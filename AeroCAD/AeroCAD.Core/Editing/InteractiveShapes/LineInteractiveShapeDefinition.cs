using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class LineInteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        private static readonly IReadOnlyList<CommandStep> DefaultSteps = new[]
        {
            new CommandStep("FirstPoint", "Specify first point:"),
            new CommandStep("NextPoint", "Specify next point:")
        };

        public LineInteractiveShapeDefinition(Func<Func<Layer>, IInteractiveCommandController> controllerFactory)
        {
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Line",
                commandName: "LINE",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                description: "Draw a line.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Line");
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;
        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
