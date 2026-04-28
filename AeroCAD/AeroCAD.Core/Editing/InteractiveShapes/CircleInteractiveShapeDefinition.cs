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

        public CircleInteractiveShapeDefinition(Func<IInteractiveCommandController> controllerFactory, bool replaceExistingCommand = false)
        {
            Pipeline = new InteractiveShapePipeline(
                name: "AeroCAD.Circle",
                commandName: "CIRCLE",
                controllerFactory: controllerFactory,
                steps: DefaultSteps,
                aliases: new[] { "C" },
                description: "Draw a circle.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Circle",
                replaceExistingCommand: replaceExistingCommand);
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;
        public InteractiveCommandRegistration CreateCommandRegistration() => Pipeline.CreateCommandRegistration();
    }
}
