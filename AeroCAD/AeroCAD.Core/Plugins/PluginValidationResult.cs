using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginValidationResult
    {
        public PluginValidationResult(IEnumerable<PluginValidationIssue> issues)
        {
            Issues = (issues ?? Enumerable.Empty<PluginValidationIssue>()).ToArray();
        }

        public IReadOnlyList<PluginValidationIssue> Issues { get; }
        public bool HasErrors => Issues.Any(issue => issue.Severity == PluginValidationIssueSeverity.Error);
    }
}
