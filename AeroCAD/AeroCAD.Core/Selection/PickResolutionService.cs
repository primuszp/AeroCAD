using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Selection
{
    public class PickResolutionService : IPickResolutionService
    {
        private readonly ICadDocumentService documentService;

        public PickResolutionService(ICadDocumentService documentService)
        {
            this.documentService = documentService;
        }

        public Entity ResolvePrimary(IEnumerable<Entity> hits)
        {
            return ResolvePrimary(hits, null);
        }

        public Entity ResolvePrimary(IEnumerable<Entity> hits, Func<Entity, bool> predicate)
        {
            if (hits == null)
                return null;

            var filteredHits = hits
                .Where(entity => entity != null && (predicate == null || predicate(entity)))
                .Distinct()
                .ToList();

            if (filteredHits.Count == 0)
                return null;

            return filteredHits
                .OrderByDescending(GetLayerOrder)
                .ThenByDescending(GetEntityOrder)
                .FirstOrDefault();
        }

        private int GetLayerOrder(Entity entity)
        {
            if (entity == null || documentService == null)
                return -1;

            var layer = documentService.GetLayerForEntity(entity);
            if (layer == null)
                return -1;

            return documentService.Layers
                .Select((candidate, index) => new { candidate, index })
                .Where(item => item.candidate.Id == layer.Id)
                .Select(item => item.index)
                .DefaultIfEmpty(-1)
                .First();
        }

        private static int GetEntityOrder(Entity entity)
        {
            var layer = entity?.RenderHost as Drawing.Layers.Layer;
            if (layer == null)
                return -1;

            return layer.Entities
                .Select((candidate, index) => new { candidate, index })
                .Where(item => item.candidate.Id == entity.Id)
                .Select(item => item.index)
                .DefaultIfEmpty(-1)
                .First();
        }
    }
}
