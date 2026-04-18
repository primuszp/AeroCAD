using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class EntitySnapDescriptorProvider : ISnapDescriptorProvider
    {
        public SnapDescriptorProviderKind Kind => SnapDescriptorProviderKind.Entities;

        public IEnumerable<ISnapDescriptor> GetDescriptors(IEnumerable<Entity> entityCandidates)
        {
            return entityCandidates?
                .OfType<ISnappable>()
                .SelectMany(entity => entity.GetSnapDescriptors())
                ?? Enumerable.Empty<ISnapDescriptor>();
        }
    }
}
