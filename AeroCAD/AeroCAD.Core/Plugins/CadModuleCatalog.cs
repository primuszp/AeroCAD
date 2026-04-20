using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class CadModuleCatalog : ICadModuleCatalog
    {
        private readonly IReadOnlyList<ICadModule> modules;

        public CadModuleCatalog(IEnumerable<ICadModule> modules)
        {
            this.modules = (modules ?? Enumerable.Empty<ICadModule>())
                .Where(module => module != null)
                .ToArray();
        }

        public IReadOnlyList<ICadModule> Modules => modules;

        public ICadModule Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return modules.FirstOrDefault(module => string.Equals(module.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
