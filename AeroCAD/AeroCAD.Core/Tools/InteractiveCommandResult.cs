using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public sealed class InteractiveCommandResult
    {
        private InteractiveCommandResult(bool handled)
        {
            Handled = handled;
        }

        public bool Handled { get; private set; }

        public CommandStep NextStep { get; private set; }

        public bool EndSession { get; private set; }

        public string ClosingMessage { get; private set; }

        public bool DeactivateTool { get; private set; }

        public bool ReturnToSelectionMode { get; private set; }

        public static InteractiveCommandResult Unhandled()
        {
            return new InteractiveCommandResult(false);
        }

        public static InteractiveCommandResult HandledOnly()
        {
            return new InteractiveCommandResult(true);
        }

        public static InteractiveCommandResult MoveToStep(CommandStep step)
        {
            return new InteractiveCommandResult(true)
            {
                NextStep = step
            };
        }

        public static InteractiveCommandResult End(string closingMessage = null, bool deactivateTool = false, bool returnToSelectionMode = false)
        {
            return new InteractiveCommandResult(true)
            {
                EndSession = true,
                ClosingMessage = closingMessage,
                DeactivateTool = deactivateTool,
                ReturnToSelectionMode = returnToSelectionMode
            };
        }
    }
}

