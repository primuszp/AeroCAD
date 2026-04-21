using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginDiscoveryResult
    {
        public PluginDiscoveryResult(IReadOnlyList<ICadModule> modules, IReadOnlyList<IEntityPlugin> plugins)
        {
            Modules = modules;
            Plugins = plugins;
        }

        public IReadOnlyList<ICadModule> Modules { get; }

        public IReadOnlyList<IEntityPlugin> Plugins { get; }
    }
}
