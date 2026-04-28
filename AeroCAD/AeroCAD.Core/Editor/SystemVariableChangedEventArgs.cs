using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class SystemVariableChangedEventArgs : EventArgs
    {
        public SystemVariableChangedEventArgs(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}
