using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class CommandStepChangedEventArgs : EventArgs
    {
        public CommandStepChangedEventArgs(CommandStep step)
        {
            Step = step;
        }

        public CommandStep Step { get; }
    }
}

