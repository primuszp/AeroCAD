using System;
using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class SystemVariableService : ISystemVariableService
    {
        public const string PdMode = "PDMODE";
        public const string PdSize = "PDSIZE";

        private readonly Dictionary<string, SystemVariableDefinition> definitions = new Dictionary<string, SystemVariableDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public SystemVariableService()
        {
            Register(new SystemVariableDefinition(
                PdMode,
                typeof(int),
                0,
                "Controls point object display geometry.",
                value => Math.Max(0, Convert.ToInt32(value))));
            Register(new SystemVariableDefinition(
                PdSize,
                typeof(double),
                0d,
                "Controls point object display size.",
                value => Convert.ToDouble(value)));
        }

        public event EventHandler<SystemVariableChangedEventArgs> VariableChanged;

        public void Register(SystemVariableDefinition definition)
        {
            if (definition == null)
                return;

            definitions[definition.Name] = definition;
            if (!values.ContainsKey(definition.Name))
                values[definition.Name] = Sanitize(definition, definition.DefaultValue);
        }

        public bool TryGet<T>(string name, out T value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(name) || !values.TryGetValue(name, out var raw))
                return false;

            if (raw is T typed)
            {
                value = typed;
                return true;
            }

            try
            {
                value = (T)Convert.ChangeType(raw, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public T Get<T>(string name, T fallback = default)
        {
            return TryGet<T>(name, out var value) ? value : fallback;
        }

        public void Set<T>(string name, T value)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var key = name.Trim().ToUpperInvariant();
            if (!definitions.TryGetValue(key, out var definition))
                Register(new SystemVariableDefinition(key, typeof(T), value));

            definition = definitions[key];
            var sanitized = Sanitize(definition, value);
            if (values.TryGetValue(key, out var existing) && Equals(existing, sanitized))
                return;

            values[key] = sanitized;
            VariableChanged?.Invoke(this, new SystemVariableChangedEventArgs(key, sanitized));
        }

        private static object Sanitize(SystemVariableDefinition definition, object value)
        {
            var sanitized = definition.Sanitize != null ? definition.Sanitize(value) : value;
            return sanitized == null ? null : Convert.ChangeType(sanitized, definition.ValueType);
        }
    }
}
