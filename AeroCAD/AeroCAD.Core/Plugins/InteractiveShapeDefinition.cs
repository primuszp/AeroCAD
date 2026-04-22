using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveShapeDefinition : IInteractiveShapeDefinition
    {
        public InteractiveShapeDefinition(
            string name,
            string commandName,
            System.Func<System.Func<Layer>, IInteractiveCommandController> controllerFactory,
            IEnumerable<CommandStep> steps,
            string[] aliases = null,
            string description = null,
            bool assignActiveLayer = true,
            string menuGroup = null,
            string menuLabel = null)
            : this(new InteractiveShapePipeline(name, commandName, controllerFactory, steps, aliases, description, assignActiveLayer, menuGroup, menuLabel))
        {
        }

        public InteractiveShapeDefinition(IInteractiveShapePipeline pipeline)
        {
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        public IInteractiveShapePipeline Pipeline { get; }
        public string Name => Pipeline.Name;
        public string CommandName => Pipeline.CommandName;
        public CommandStep InitialStep => Pipeline.InitialStep;
        public IReadOnlyList<CommandStep> Steps => Pipeline.Steps;

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return Pipeline.CreateCommandRegistration();
        }
    }
}
