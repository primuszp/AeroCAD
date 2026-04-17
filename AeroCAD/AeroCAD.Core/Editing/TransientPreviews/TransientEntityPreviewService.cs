using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public class TransientEntityPreviewService : ITransientEntityPreviewService
    {
        private readonly IReadOnlyList<ITransientEntityPreviewStrategy> strategies;

        public TransientEntityPreviewService(IEnumerable<ITransientEntityPreviewStrategy> strategies)
        {
            this.strategies = (strategies ?? Enumerable.Empty<ITransientEntityPreviewStrategy>())
                .ToList()
                .AsReadOnly();
        }

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            if (entity == null)
                return GripPreview.Empty;

            var strategy = strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
            return strategy?.CreatePreview(entity, color) ?? GripPreview.Empty;
        }
    }
}
