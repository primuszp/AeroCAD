using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapModePolicy : ISnapModePolicy
    {
        private readonly HashSet<SnapType> enabledModes;

        public SnapModePolicy(IEnumerable<SnapType> enabledModes = null, IEnumerable<SnapType> evaluationOrder = null)
        {
            EvaluationOrder = (evaluationOrder ?? new[] { SnapType.Endpoint, SnapType.Midpoint, SnapType.Nearest }).ToList().AsReadOnly();
            this.enabledModes = new HashSet<SnapType>(enabledModes ?? Array.Empty<SnapType>());
        }

        public IReadOnlyList<SnapType> EvaluationOrder { get; }

        public bool IsEnabled(SnapType snapType)
        {
            return enabledModes.Contains(snapType);
        }

        public void Enable(SnapType snapType)
        {
            enabledModes.Add(snapType);
        }

        public void Disable(SnapType snapType)
        {
            enabledModes.Remove(snapType);
        }

        public void SetEnabledModes(IEnumerable<SnapType> snapTypes)
        {
            enabledModes.Clear();

            if (snapTypes == null)
                return;

            foreach (var snapType in snapTypes)
                enabledModes.Add(snapType);
        }
    }
}

