using System;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandRepeatCoordinator
    {
        private string lastExecutedCommand = string.Empty;

        public string LastExecutedCommand => lastExecutedCommand;

        public void RememberExecutedCommand(string commandName)
        {
            if (!string.IsNullOrWhiteSpace(commandName))
                lastExecutedCommand = commandName.Trim();
        }

        public bool HandleBlankSubmit(ICommandInteractiveTool activeInteractiveTool, Func<string, bool> executeCommand, Func<string> activeCommandNameProvider, Action refreshViewport)
        {
            if (activeInteractiveTool != null)
            {
                var activeCommandName = activeCommandNameProvider?.Invoke();
                activeInteractiveTool.TryComplete();
                refreshViewport?.Invoke();

                if (!string.IsNullOrWhiteSpace(activeCommandName))
                    lastExecutedCommand = activeCommandName;

                return true;
            }

            if (string.IsNullOrWhiteSpace(lastExecutedCommand) || executeCommand == null)
                return false;

            var executed = executeCommand(lastExecutedCommand);
            if (executed)
                refreshViewport?.Invoke();

            return executed;
        }
    }
}
