namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandSession
    {
        public CommandSession(string commandName, CommandPrompt prompt)
        {
            CommandName = commandName ?? string.Empty;
            Prompt = prompt ?? CommandPrompt.Default;
        }

        public string CommandName { get; }

        public CommandPrompt Prompt { get; }
    }
}

