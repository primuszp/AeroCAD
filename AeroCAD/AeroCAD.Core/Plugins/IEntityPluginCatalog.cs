using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface IEntityPluginCatalog
    {
        IReadOnlyList<EntityPluginDescriptor> Descriptors { get; }
        IReadOnlyList<EntityPluginDescriptor> GetByCapability(EntityPluginCapability capability);
        EntityPluginDescriptor Find(string name);
    }
}
