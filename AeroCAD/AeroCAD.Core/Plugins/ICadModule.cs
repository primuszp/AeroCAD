using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// A deployable unit that groups one or more entity plugins under a shared identity.
    /// Register via ModelSpace.RegisterModule() — all contained plugins are registered automatically.
    /// </summary>
    public interface ICadModule
    {
        string Name { get; }
        string Version { get; }
        IEnumerable<IEntityPlugin> Plugins { get; }
    }
}
