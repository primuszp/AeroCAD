using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapDescriptorService : ISnapDescriptorService
    {
        private readonly IReadOnlyList<ISnapDescriptorProvider> providers;

        public SnapDescriptorService(IEnumerable<ISnapDescriptorProvider> providers)
        {
            this.providers = providers?.ToList() ?? throw new ArgumentNullException(nameof(providers));
        }

        public IEnumerable<ISnapDescriptor> GetEntityDescriptors(IEnumerable<Entity> entityCandidates)
        {
            return GetDescriptors(SnapDescriptorProviderKind.Entities, entityCandidates);
        }

        public IEnumerable<ISnapDescriptor> GetSelectedGripDescriptors()
        {
            return GetDescriptors(SnapDescriptorProviderKind.SelectedGrips, Enumerable.Empty<Entity>());
        }

        public IEnumerable<ISnapDescriptor> GetEntityAndSelectedGripDescriptors(IEnumerable<Entity> entityCandidates)
        {
            return GetDescriptors(
                new[] { SnapDescriptorProviderKind.Entities, SnapDescriptorProviderKind.SelectedGrips },
                entityCandidates);
        }

        private IEnumerable<ISnapDescriptor> GetDescriptors(SnapDescriptorProviderKind kind, IEnumerable<Entity> entityCandidates)
        {
            return GetDescriptors(new[] { kind }, entityCandidates);
        }

        private IEnumerable<ISnapDescriptor> GetDescriptors(IEnumerable<SnapDescriptorProviderKind> kinds, IEnumerable<Entity> entityCandidates)
        {
            var kindSet = new HashSet<SnapDescriptorProviderKind>(kinds);
            var candidates = entityCandidates?.ToList() ?? new List<Entity>();

            return providers
                .Where(provider => kindSet.Contains(provider.Kind))
                .SelectMany(provider => provider.GetDescriptors(candidates));
        }
    }
}
