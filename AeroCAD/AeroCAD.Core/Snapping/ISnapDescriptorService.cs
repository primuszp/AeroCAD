using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public interface ISnapDescriptorService
    {
        IEnumerable<ISnapDescriptor> GetEntityDescriptors(IEnumerable<Entity> entityCandidates);

        IEnumerable<ISnapDescriptor> GetSelectedGripDescriptors();

        IEnumerable<ISnapDescriptor> GetEntityAndSelectedGripDescriptors(IEnumerable<Entity> entityCandidates);
    }
}
