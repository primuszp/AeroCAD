using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapEngine : ISnapEngine
    {
        public double ToleranceWorld { get; set; } = 10.0;

        public ISnapModePolicy ModePolicy { get; }

        public SnapResult CurrentSnap { get; private set; }

        public SnapEngine(ISnapModePolicy modePolicy)
        {
            ModePolicy = modePolicy;
        }

        public void Update(Point worldPos, IEnumerable<Entity> candidates)
        {
            var descriptors = candidates
                .OfType<ISnappable>()
                .SelectMany(entity => entity.GetSnapDescriptors());

            Update(worldPos, descriptors);
        }

        public void Update(Point worldPos, IEnumerable<ISnapDescriptor> descriptors)
        {
            CurrentSnap = null;
            var descriptorList = descriptors?.ToList() ?? new List<ISnapDescriptor>();
            foreach (var snapType in ModePolicy.EvaluationOrder)
            {
                if (!ModePolicy.IsEnabled(snapType))
                    continue;

                var result = FindBestSnap(worldPos, descriptorList, snapType);
                if (result != null)
                    CurrentSnap = result;
                if (CurrentSnap != null)
                    return;
            }
        }

        public Point Snap(Point rawPos)
        {
            return CurrentSnap?.Point ?? rawPos;
        }

        private SnapResult FindBestSnap(Point worldPos, IEnumerable<ISnapDescriptor> descriptors, SnapType snapType)
        {
            SnapResult bestResult = null;
            double bestDistance = ToleranceWorld;

            foreach (var descriptor in descriptors)
            {
                if (descriptor.Type != snapType)
                    continue;

                var result = descriptor.TrySnap(worldPos, ToleranceWorld);
                if (result == null)
                    continue;

                double dx = result.Point.X - worldPos.X;
                double dy = result.Point.Y - worldPos.Y;
                double distance = System.Math.Sqrt(dx * dx + dy * dy);

                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    bestResult = result;
                }
            }

            return bestResult;
        }
    }
}

