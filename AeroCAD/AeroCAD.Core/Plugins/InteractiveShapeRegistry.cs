using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveShapeRegistry : IInteractiveShapeRegistry
    {
        private readonly IReadOnlyList<IInteractiveShapeDefinition> definitions;

        public InteractiveShapeRegistry(IEnumerable<IInteractiveShapeDefinition> definitions)
        {
            this.definitions = (definitions ?? Enumerable.Empty<IInteractiveShapeDefinition>())
                .Where(definition => definition != null)
                .ToArray();
        }

        public IReadOnlyList<IInteractiveShapeDefinition> Definitions => definitions;

        public IInteractiveShapeDefinition Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return definitions.FirstOrDefault(definition =>
                string.Equals(definition.Name, name.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(definition.CommandName, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
