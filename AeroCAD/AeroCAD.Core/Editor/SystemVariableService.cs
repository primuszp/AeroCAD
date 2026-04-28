using System;
using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class SystemVariableService : ISystemVariableService
    {
        public const string PdMode = "PDMODE";
        public const string PdSize = "PDSIZE";

        private readonly Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [PdMode] = 0,
            [PdSize] = 0d
        };

        public event EventHandler<SystemVariableChangedEventArgs> VariableChanged;

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
            var sanitized = Sanitize(key, value);
            if (values.TryGetValue(key, out var existing) && Equals(existing, sanitized))
                return;

            values[key] = sanitized;
            VariableChanged?.Invoke(this, new SystemVariableChangedEventArgs(key, sanitized));
        }

        private static object Sanitize<T>(string key, T value)
        {
            if (string.Equals(key, PdMode, StringComparison.OrdinalIgnoreCase))
            {
                var mode = Convert.ToInt32(value);
                return mode < 0 ? 0 : mode;
            }

            if (string.Equals(key, PdSize, StringComparison.OrdinalIgnoreCase))
                return Convert.ToDouble(value);

            return value;
        }
    }
}
