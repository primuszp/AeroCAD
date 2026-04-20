using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface ICadModuleCatalog
    {
        IReadOnlyList<ICadModule> Modules { get; }
        ICadModule Find(string name);
    }
}
