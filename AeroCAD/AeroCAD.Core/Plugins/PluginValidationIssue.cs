namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginValidationIssue
    {
        public PluginValidationIssue(PluginValidationIssueSeverity severity, string message, string source = null)
        {
            Severity = severity;
            Message = string.IsNullOrWhiteSpace(message) ? "Plugin validation issue." : message;
            Source = source;
        }

        public PluginValidationIssueSeverity Severity { get; }
        public string Message { get; }
        public string Source { get; }
    }
}
