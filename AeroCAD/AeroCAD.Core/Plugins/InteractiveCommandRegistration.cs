using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveCommandRegistration
    {
        public InteractiveCommandRegistration(
            string commandName,
            Func<IInteractiveCommandController> controllerFactory,
            string toolName = null,
            IEnumerable<string> aliases = null,
            string description = null,
            EditorCommandPolicy policy = null,
            bool assignActiveLayer = false,
            string menuGroup = null,
            string menuLabel = null,
            bool replaceExistingCommand = false)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name is required.", nameof(commandName));

            CommandName = commandName.Trim().ToUpperInvariant();
            ControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            ToolName = string.IsNullOrWhiteSpace(toolName) ? $"{CommandName}Tool" : toolName.Trim();
            Aliases = aliases;
            Description = description;
            Policy = policy;
            AssignActiveLayer = assignActiveLayer;
            MenuGroup = menuGroup;
            MenuLabel = menuLabel;
            ReplaceExistingCommand = replaceExistingCommand;
        }

        public string CommandName { get; }

        public string ToolName { get; }

        public IEnumerable<string> Aliases { get; }

        public string Description { get; }

        public EditorCommandPolicy Policy { get; }

        public bool AssignActiveLayer { get; }

        public string MenuGroup { get; }

        public string MenuLabel { get; }

        public bool ReplaceExistingCommand { get; }

        public Func<IInteractiveCommandController> ControllerFactory { get; }

        public InteractiveCommandRegistration(
            string commandName,
            Func<Func<Drawing.Layers.Layer>, IInteractiveCommandController> controllerFactory,
            string toolName = null,
            IEnumerable<string> aliases = null,
            string description = null,
            EditorCommandPolicy policy = null,
            bool assignActiveLayer = false,
            string menuGroup = null,
            string menuLabel = null,
            bool replaceExistingCommand = false)
            : this(commandName, () => controllerFactory?.Invoke(null), toolName, aliases, description, policy, assignActiveLayer, menuGroup, menuLabel, replaceExistingCommand)
        {
        }

        public EditorCommandDefinition CreateCommandDefinition()
        {
            return new EditorCommandDefinition(
                CommandName,
                Aliases,
                Description,
                Policy,
                modalToolType: null,
                assignActiveLayer: AssignActiveLayer,
                menuGroup: MenuGroup,
                menuLabel: MenuLabel,
                modalToolName: ToolName,
                replaceExistingCommand: ReplaceExistingCommand);
        }
    }
}
