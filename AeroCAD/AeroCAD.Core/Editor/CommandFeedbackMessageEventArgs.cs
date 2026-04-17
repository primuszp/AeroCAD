using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public class CommandFeedbackMessageEventArgs : EventArgs
    {
        public CommandFeedbackMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}

