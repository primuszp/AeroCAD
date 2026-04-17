using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class GripPreviewService : IGripPreviewService
    {
        private readonly IReadOnlyList<IGripPreviewStrategy> strategies;

        public GripPreviewService(IEnumerable<IGripPreviewStrategy> strategies)
        {
            this.strategies = strategies.ToList();
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            if (entity == null)
                return GripPreview.Empty;

            var strategy = strategies.FirstOrDefault(candidate => candidate.CanHandle(entity));
            return strategy?.CreatePreview(entity, gripIndex, newPosition) ?? GripPreview.Empty;
        }
    }
}

