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
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Circle",
                commandName: "CIRCLE",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                description: "Draw a circle.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Circle");
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;
        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
