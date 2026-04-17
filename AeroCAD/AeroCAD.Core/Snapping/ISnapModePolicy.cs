using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Snapping
{
    public interface ISnapModePolicy
    {
        IReadOnlyList<SnapType> EvaluationOrder { get; }

        bool IsEnabled(SnapType snapType);

        void Enable(SnapType snapType);

        void Disable(SnapType snapType);

        void SetEnabledModes(IEnumerable<SnapType> snapTypes);
    }
}

