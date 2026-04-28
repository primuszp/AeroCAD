using System;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginDiscoveryIssue
    {
        public PluginDiscoveryIssue(string source, string message, Exception exception = null)
        {
            Source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source;
            Message = string.IsNullOrWhiteSpace(message) ? "Plugin discovery failed." : message;
            Exception = exception;
        }

        public string Source { get; }

        public string Message { get; }

        public Exception Exception { get; }

        public override string ToString()
        {
            return Exception == null
                ? $"{Source}: {Message}"
                : $"{Source}: {Message} ({Exception.GetType().Name}: {Exception.Message})";
        }
    }
}
