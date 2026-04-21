using System.Collections.Generic;
using System.Reflection;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface IPluginDiscoveryService
    {
        PluginDiscoveryResult Discover(IEnumerable<Assembly> assemblies);
    }
}
