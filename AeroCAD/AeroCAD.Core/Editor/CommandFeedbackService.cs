using System;
using System.Globalization;
using System.Windows;

namespace Primusz.AeroCAD.Core.Editor
{
    public class CommandFeedbackService : ICommandFeedbackService
    {
        private CommandPrompt activePrompt = CommandPrompt.Default;
        private CommandSession activeSession;

        public string Prompt => activePrompt.ToDisplayString();

        public string ActiveCommandName => activeSession?.CommandName;

        public bool HasActiveCommand => activeSession != null;

        public CommandPrompt ActivePrompt => activePrompt;

        public CommandSession ActiveSession => activeSession;

        public event EventHandler StateChanged;

        public event EventHandler<CommandFeedbackMessageEventArgs> MessageLogged;

        public CommandInputToken ParseInput(string rawInput)
        {
            var normalized = (rawInput ?? string.Empty).Trim();
            if (normalized.Length == 0)
                return CommandInputToken.Empty();

            Point point;
            if (TryParsePoint(normalized, out point))
                return CommandInputToken.Point(normalized, point);

            double scalar;
            if (TryParseScalar(normalized, out scalar))
                return CommandInputToken.Scalar(normalized, scalar);

            if (IsKeyword(normalized))
                return CommandInputToken.Keyword(normalized, normalized.ToUpperInvariant());

            return CommandInputToken.Text(normalized, normalized);
        }

        public void BeginCommand(CommandSession session)
        {
            activeSession = session == null ? null : new CommandSession(session.CommandName, session.Prompt);
            activePrompt = activeSession?.Prompt ?? CommandPrompt.Default;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void BeginCommand(string commandName, string prompt)
        {
            BeginCommand(new CommandSession(commandName, new CommandPrompt(prompt)));
        }

        public void SetPrompt(CommandPrompt prompt)
        {
            activePrompt = prompt ?? CommandPrompt.Default;
            if (activeSession != null)
                activeSession = new CommandSession(activeSession.CommandName, activePrompt);

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetPrompt(string prompt)
        {
            SetPrompt(new CommandPrompt(prompt));
        }

        public void LogInput(CommandInputToken token)
        {
            if (token == null || token.IsEmpty)
                return;

            LogInput(token.FormatForDisplay());
        }

        public void LogInput(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
                MessageLogged?.Invoke(this, new CommandFeedbackMessageEventArgs($"{Prompt} {input}"));
        }

        public void LogMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                MessageLogged?.Invoke(this, new CommandFeedbackMessageEventArgs(message));
        }

        public void EndCommand(string closingMessage = null)
        {
            activeSession = null;
            activePrompt = CommandPrompt.Default;
            StateChanged?.Invoke(this, EventArgs.Empty);

            if (!string.IsNullOrWhiteSpace(closingMessage))
                LogMessage(closingMessage);
        }

        private static bool TryParsePoint(string input, out Point point)
        {
            point = new Point();
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var normalized = input.Trim().Replace(';', ',');
            string[] parts = normalized.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                parts = normalized.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    return false;
            }

            double x;
            double y;
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                !double.TryParse(parts[0], out x))
                return false;

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                !double.TryParse(parts[1], out y))
                return false;

            point = new Point(x, y);
            return true;
        }

        private static bool TryParseScalar(string input, out double scalar)
        {
            scalar = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var normalized = input.Trim().Replace(';', ',');
            if (normalized.Contains(",") || normalized.Contains(" "))
                return false;

            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out scalar) ||
                   double.TryParse(normalized, out scalar);
        }

        private static bool IsKeyword(string input)
        {
            foreach (char character in input)
            {
                if (char.IsLetter(character))
                    continue;

                if (character == '_' || character == '-')
                    continue;

                return false;
            }

            return input.Length > 0;
        }
    }
}

