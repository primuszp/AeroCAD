using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapModePolicy : ISnapModePolicy
    {
        private readonly HashSet<SnapType> enabledModes;
        private readonly List<SnapType> evaluationOrder;

        public SnapModePolicy(IEnumerable<SnapType> enabledModes = null, IEnumerable<SnapType> evaluationOrder = null)
        {
            this.enabledModes = new HashSet<SnapType>(enabledModes ?? Array.Empty<SnapType>());
            this.evaluationOrder = (evaluationOrder ?? new[] { SnapType.Endpoint, SnapType.Midpoint, SnapType.Nearest }).ToList();
            Normalize();
        }

        public IReadOnlyList<SnapType> EvaluationOrder => evaluationOrder.AsReadOnly();

        public bool IsEnabled(SnapType snapType)
        {
            return enabledModes.Contains(snapType);
        }

        public void Enable(SnapType snapType)
        {
            enabledModes.Add(snapType);
            Normalize();
        }

        public void Disable(SnapType snapType)
        {
            enabledModes.Remove(snapType);
            Normalize();
        }

        public void SetEnabledModes(IEnumerable<SnapType> snapTypes)
        {
            enabledModes.Clear();

            if (snapTypes == null)
                return;

            foreach (var snapType in snapTypes)
                enabledModes.Add(snapType);

            Normalize();
        }

        private void Normalize()
        {
            // Ensure every enabled mode is actually evaluated, while preserving the
            // explicitly configured evaluation order for known modes.
            var ordered = evaluationOrder.Distinct().ToList();
            foreach (var enabledMode in enabledModes)
            {
                if (!ordered.Contains(enabledMode))
                    ordered.Add(enabledMode);
            }

            evaluationOrder.Clear();
            evaluationOrder.AddRange(ordered);
        }
    }
}

