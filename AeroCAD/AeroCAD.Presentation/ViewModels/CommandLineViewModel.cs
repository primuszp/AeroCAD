using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Primusz.AeroCAD.Presentation.ViewModels
{
    public class CommandLineViewModel : ViewModelBase
    {
        private readonly Action<string> submitAction;
        private readonly Action cancelAction;
        private readonly List<string> commandHistory = new List<string>();
        private int historyIndex;
        private string currentInput = string.Empty;
        private string prompt = "Parancs:";

        public CommandLineViewModel(Action<string> submitAction, Action cancelAction)
        {
            this.submitAction = submitAction ?? throw new ArgumentNullException(nameof(submitAction));
            this.cancelAction = cancelAction ?? throw new ArgumentNullException(nameof(cancelAction));
        }

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public string CurrentInput
        {
            get => currentInput;
            set
            {
                if (currentInput == value)
                    return;

                currentInput = value;
                OnPropertyChanged();
            }
        }

        public string Prompt
        {
            get => prompt;
            set
            {
                if (prompt == value)
                    return;

                prompt = value;
                OnPropertyChanged();
            }
        }

        public void SubmitCurrentInput()
        {
            var input = (CurrentInput ?? string.Empty).Trim();
            if (input.Length > 0)
            {
                Messages.Add($"{Prompt} {input}");

                if (commandHistory.Count == 0 || !string.Equals(commandHistory[commandHistory.Count - 1], input, StringComparison.OrdinalIgnoreCase))
                    commandHistory.Add(input);

                historyIndex = commandHistory.Count;
            }

            CurrentInput = string.Empty;
            submitAction(input);
        }

        public void WriteMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Messages.Add(message);
        }

        public string RecallPrevious()
        {
            if (commandHistory.Count == 0)
                return CurrentInput;

            if (historyIndex > 0)
                historyIndex--;

            CurrentInput = commandHistory[historyIndex];
            return CurrentInput;
        }

        public string RecallNext()
        {
            if (commandHistory.Count == 0)
                return CurrentInput;

            if (historyIndex < commandHistory.Count - 1)
            {
                historyIndex++;
                CurrentInput = commandHistory[historyIndex];
            }
            else
            {
                historyIndex = commandHistory.Count;
                CurrentInput = string.Empty;
            }

            return CurrentInput;
        }

        public string AutocompleteFromLast()
        {
            if (commandHistory.Count == 0)
                return CurrentInput;

            var last = commandHistory[commandHistory.Count - 1];
            if (CurrentInput.Length < last.Length)
                CurrentInput = last.Substring(0, CurrentInput.Length + 1);

            return CurrentInput;
        }

        public void RequestCancel()
        {
            CurrentInput = string.Empty;
            cancelAction();
        }
    }
}

