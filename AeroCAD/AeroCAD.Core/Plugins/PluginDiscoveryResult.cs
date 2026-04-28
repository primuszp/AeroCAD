using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginDiscoveryResult
    {
        public PluginDiscoveryResult(
            IReadOnlyList<ICadModule> modules,
            IReadOnlyList<IEntityPlugin> plugins,
            IReadOnlyList<PluginDiscoveryIssue> issues = null)
        {
            Modules = modules;
            Plugins = plugins;
            Issues = issues ?? new List<PluginDiscoveryIssue>().AsReadOnly();
        }

        public IReadOnlyList<ICadModule> Modules { get; }

        public IReadOnlyList<IEntityPlugin> Plugins { get; }

        public IReadOnlyList<PluginDiscoveryIssue> Issues { get; }
    }
}
