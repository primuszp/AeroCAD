using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SelectedGripSnapDescriptorProvider : ISnapDescriptorProvider
    {
        private readonly IGripService gripService;

        public SelectedGripSnapDescriptorProvider(IGripService gripService)
        {
            this.gripService = gripService;
        }

        public SnapDescriptorProviderKind Kind => SnapDescriptorProviderKind.SelectedGrips;

        public IEnumerable<ISnapDescriptor> GetDescriptors(IEnumerable<Entity> entityCandidates)
        {
            return gripService?.GetSelectedGrips()
                .Select(grip => new SnapPointDescriptor(grip.ToSnapType(), grip.GetPoint, grip.Owner, grip.Index))
                ?? Enumerable.Empty<ISnapDescriptor>();
        }
    }
}
