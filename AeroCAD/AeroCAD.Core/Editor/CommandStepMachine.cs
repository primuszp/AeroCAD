using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandStepMachine
    {
        private CommandStep currentStep;

        public CommandStep CurrentStep => currentStep;

        public event EventHandler<CommandStepChangedEventArgs> StepChanged;

        public void MoveTo(CommandStep step)
        {
            currentStep = step;
            StepChanged?.Invoke(this, new CommandStepChangedEventArgs(step));
        }

        public void Reset()
        {
            MoveTo(null);
        }
    }
}

