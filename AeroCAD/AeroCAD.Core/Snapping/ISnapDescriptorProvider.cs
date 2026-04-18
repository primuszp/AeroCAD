using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public interface ISnapDescriptorProvider
    {
        SnapDescriptorProviderKind Kind { get; }

        IEnumerable<ISnapDescriptor> GetDescriptors(IEnumerable<Entity> entityCandidates);
    }
}
