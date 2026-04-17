using System;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface IGridSettingsService
    {
        bool IsEnabled { get; }

        double MinorSpacingX { get; set; }

        double MinorSpacingY { get; set; }

        int MajorLineEvery { get; set; }

        Color MinorLineColor { get; set; }

        Color MajorLineColor { get; set; }

        double MinimumScreenSpacing { get; set; }

        void Toggle();

        event EventHandler StateChanged;
    }
}

