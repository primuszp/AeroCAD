using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveShapePipeline : IInteractiveShapePipeline
    {
        private readonly IReadOnlyList<CommandStep> steps;

        public InteractiveShapePipeline(
            string name,
            string commandName,
            Func<Func<Layer>, IInteractiveCommandController> controllerFactory,
            IEnumerable<CommandStep> steps,
            string[] aliases = null,
            string description = null,
            bool assignActiveLayer = true,
            string menuGroup = null,
            string menuLabel = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Shape name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name is required.", nameof(commandName));

            Name = name.Trim();
            CommandName = commandName.Trim().ToUpperInvariant();
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            this.steps = (steps ?? Enumerable.Empty<CommandStep>()).Where(step => step != null).ToList().AsReadOnly();
            Aliases = aliases;
            Description = description;
            AssignActiveLayer = assignActiveLayer;
            MenuGroup = menuGroup;
            MenuLabel = menuLabel;
        }

        public string Name { get; }
        public string CommandName { get; }
        public IReadOnlyList<CommandStep> Steps => steps;
        public string[] Aliases { get; }
        public string Description { get; }
        public bool AssignActiveLayer { get; }
        public string MenuGroup { get; }
        public string MenuLabel { get; }
        public CommandStep InitialStep => steps.FirstOrDefault();
        public Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return new InteractiveCommandRegistration(
                CommandName,
                ControllerFactory,
                aliases: Aliases,
                description: Description,
                assignActiveLayer: AssignActiveLayer,
                menuGroup: MenuGroup,
                menuLabel: MenuLabel);
        }

        public IInteractiveCommandController CreateController(System.Func<Layer> activeLayerResolver)
        {
            return ControllerFactory(activeLayerResolver);
        }
    }
}
