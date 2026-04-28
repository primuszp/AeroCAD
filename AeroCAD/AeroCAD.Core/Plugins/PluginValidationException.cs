using System;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginValidationException : InvalidOperationException
    {
        public PluginValidationException(PluginValidationResult result)
            : base(CreateMessage(result))
        {
            Result = result;
        }

        public PluginValidationResult Result { get; }

        private static string CreateMessage(PluginValidationResult result)
        {
            var issues = result?.Issues?.Where(issue => issue.Severity == PluginValidationIssueSeverity.Error).ToArray()
                ?? Array.Empty<PluginValidationIssue>();
            if (issues.Length == 0)
                return "Plugin validation failed.";

            return "Plugin validation failed: " + string.Join("; ", issues.Select(issue => issue.Message));
        }
    }
}
