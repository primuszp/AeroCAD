using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface ICommandFeedbackService
    {
        string Prompt { get; }

        string ActiveCommandName { get; }

        bool HasActiveCommand { get; }

        CommandPrompt ActivePrompt { get; }

        CommandSession ActiveSession { get; }

        event EventHandler StateChanged;

        event EventHandler<CommandFeedbackMessageEventArgs> MessageLogged;

        CommandInputToken ParseInput(string rawInput);

        void BeginCommand(CommandSession session);

        void BeginCommand(string commandName, string prompt);

        void SetPrompt(CommandPrompt prompt);

        void SetPrompt(string prompt);

        void LogInput(CommandInputToken token);

        void LogInput(string input);

        void LogMessage(string message);

        void EndCommand(string closingMessage = null);
    }
}

