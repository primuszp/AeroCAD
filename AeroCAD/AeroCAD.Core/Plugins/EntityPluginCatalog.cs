using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class EntityPluginCatalog : IEntityPluginCatalog
    {
        private readonly IReadOnlyList<EntityPluginDescriptor> descriptors;

        public EntityPluginCatalog(IEnumerable<IEntityPlugin> plugins)
        {
            descriptors = (plugins ?? Enumerable.Empty<IEntityPlugin>())
                .Select(plugin => plugin?.Descriptor)
                .Where(descriptor => descriptor != null)
                .ToArray();
        }

        public IReadOnlyList<EntityPluginDescriptor> Descriptors => descriptors;

        public IReadOnlyList<EntityPluginDescriptor> GetByCapability(EntityPluginCapability capability)
        {
            return descriptors
                .Where(descriptor => (descriptor.Capabilities & capability) == capability)
                .ToArray();
        }

        public EntityPluginDescriptor Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return descriptors.FirstOrDefault(descriptor => string.Equals(descriptor.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
