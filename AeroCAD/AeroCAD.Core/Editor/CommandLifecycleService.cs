using System;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandLifecycleService
    {
        private readonly CommandRepeatCoordinator repeatCoordinator;

        public CommandLifecycleService()
            : this(new CommandRepeatCoordinator())
        {
        }

        public CommandLifecycleService(CommandRepeatCoordinator repeatCoordinator)
        {
            this.repeatCoordinator = repeatCoordinator ?? throw new ArgumentNullException(nameof(repeatCoordinator));
        }

        public bool TryHandleCommandLineInput(
            string input,
            ICommandInteractiveTool activeInteractiveTool,
            Func<string, CommandInputToken> parseInput,
            Func<CommandInputToken, bool> submitToActiveTool,
            Func<string, bool> executeCommand,
            Func<string> activeCommandNameProvider,
            Action refreshViewport,
            Action<string> logMessage)
        {
            var trimmedInput = (input ?? string.Empty).Trim();
            var normalized = trimmedInput.ToUpperInvariant();
            var token = parseInput != null ? parseInput(trimmedInput) : CommandInputToken.Text(trimmedInput, trimmedInput);

            if (trimmedInput.Length == 0)
            {
                repeatCoordinator.HandleBlankSubmit(
                    activeInteractiveTool,
                    executeCommand,
                    activeCommandNameProvider,
                    refreshViewport);
                return true;
            }

            if (activeInteractiveTool != null && submitToActiveTool != null && submitToActiveTool(token))
            {
                refreshViewport?.Invoke();
                var activeCommandName = activeCommandNameProvider?.Invoke();
                if (!string.IsNullOrWhiteSpace(activeCommandName))
                    repeatCoordinator.RememberExecutedCommand(activeCommandName);
                return true;
            }

            if (executeCommand != null && executeCommand(normalized))
            {
                repeatCoordinator.RememberExecutedCommand(normalized);
                refreshViewport?.Invoke();
                return true;
            }

            if (activeInteractiveTool != null)
                logMessage?.Invoke($"Invalid input for active command: {input}");
            else
                logMessage?.Invoke($"Unknown command: {input}");

            return false;
        }
    }
}
