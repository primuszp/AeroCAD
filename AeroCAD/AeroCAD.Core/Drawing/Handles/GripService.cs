using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Drawing.Handles
{
    public class GripService : IGripService
    {
        private readonly ISelectionManager selectionManager;

        public GripService(ISelectionManager selectionManager)
        {
            this.selectionManager = selectionManager;
        }

        public IReadOnlyList<GripDescriptor> GetSelectedGrips()
        {
            if (selectionManager?.SelectedEntities == null || selectionManager.SelectedEntities.Count == 0)
                return System.Array.Empty<GripDescriptor>();

            return selectionManager.SelectedEntities
                .SelectMany(entity => entity.GetGripDescriptors())
                .ToList();
        }

        public GripDescriptor FindSnapCandidate(Point worldPoint, double toleranceWorld)
        {
            GripDescriptor best = null;
            double bestDistance = toleranceWorld;

            foreach (var grip in GetSelectedGrips())
            {
                var gripPoint = grip.GetPoint();
                double dx = gripPoint.X - worldPoint.X;
                double dy = gripPoint.Y - worldPoint.Y;
                double distance = System.Math.Sqrt(dx * dx + dy * dy);
                if (distance > bestDistance)
                    continue;

                bestDistance = distance;
                best = grip;
            }

            return best;
        }
    }
}
