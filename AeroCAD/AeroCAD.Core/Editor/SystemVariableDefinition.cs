using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class SystemVariableDefinition
    {
        public SystemVariableDefinition(string name, Type valueType, object defaultValue, string description = null, Func<object, object> sanitize = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Variable name is required.", nameof(name)) : name.Trim().ToUpperInvariant();
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
            DefaultValue = defaultValue;
            Description = description ?? string.Empty;
            Sanitize = sanitize;
        }

        public string Name { get; }

        public Type ValueType { get; }

        public object DefaultValue { get; }

        public string Description { get; }

        public Func<object, object> Sanitize { get; }
    }
}
