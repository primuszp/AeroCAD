using System;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Markers
{
    public interface IMarkerAppearanceService
    {
        event EventHandler AppearanceChanged;

        double MarkerSize { get; set; }

        double MarkerStrokeThickness { get; set; }

        Color GripEndpointColor { get; set; }

        Color GripMidpointColor { get; set; }

        Color GripActiveColor { get; set; }

        Color GripBorderColor { get; set; }

        Color SnapStrokeColor { get; set; }

        Color SnapHoverColor { get; set; }
    }
}

