using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// Base class for CAD modules. Version defaults to "1.0.0".
    /// Subclasses implement Name and Plugins; everything else is optional.
    /// </summary>
    public abstract class CadModuleBase : ICadModule
    {
        public abstract string Name { get; }
        public virtual string Version => "1.0.0";
        public abstract IEnumerable<IEntityPlugin> Plugins { get; }
    }
}
