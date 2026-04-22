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

        public PolylineInteractiveShapeDefinition(Func<IInteractiveCommandController> controllerFactory)
        {
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Polyline",
                commandName: "PLINE",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                description: "Draw a polyline.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Polyline");
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;
        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
