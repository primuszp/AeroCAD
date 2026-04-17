using System;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Markers
{
    public class MarkerAppearanceService : IMarkerAppearanceService
    {
        private double markerSize = 10.0d;
        private double markerStrokeThickness = 1.5d;
        private Color gripEndpointColor = Colors.MediumBlue;
        private Color gripMidpointColor = Colors.Cyan;
        private Color gripActiveColor = Colors.Red;
        private Color gripBorderColor = Colors.LightGray;
        private Color snapStrokeColor = Colors.Yellow;
        private Color snapHoverColor = Colors.Orange;

        public event EventHandler AppearanceChanged;

        public double MarkerSize
        {
            get { return markerSize; }
            set
            {
                if (Math.Abs(markerSize - value) < double.Epsilon)
                    return;

                markerSize = value;
                OnAppearanceChanged();
            }
        }

        public double MarkerStrokeThickness
        {
            get { return markerStrokeThickness; }
            set
            {
                if (Math.Abs(markerStrokeThickness - value) < double.Epsilon)
                    return;

                markerStrokeThickness = value;
                OnAppearanceChanged();
            }
        }

        public Color GripEndpointColor
        {
            get { return gripEndpointColor; }
            set
            {
                if (gripEndpointColor.Equals(value))
                    return;

                gripEndpointColor = value;
                OnAppearanceChanged();
            }
        }

        public Color GripMidpointColor
        {
            get { return gripMidpointColor; }
            set
            {
                if (gripMidpointColor.Equals(value))
                    return;

                gripMidpointColor = value;
                OnAppearanceChanged();
            }
        }

        public Color GripActiveColor
        {
            get { return gripActiveColor; }
            set
            {
                if (gripActiveColor.Equals(value))
                    return;

                gripActiveColor = value;
                OnAppearanceChanged();
            }
        }

        public Color GripBorderColor
        {
            get { return gripBorderColor; }
            set
            {
                if (gripBorderColor.Equals(value))
                    return;

                gripBorderColor = value;
                OnAppearanceChanged();
            }
        }

        public Color SnapStrokeColor
        {
            get { return snapStrokeColor; }
            set
            {
                if (snapStrokeColor.Equals(value))
                    return;

                snapStrokeColor = value;
                OnAppearanceChanged();
            }
        }

        public Color SnapHoverColor
        {
            get { return snapHoverColor; }
            set
            {
                if (snapHoverColor.Equals(value))
                    return;

                snapHoverColor = value;
                OnAppearanceChanged();
            }
        }

        private void OnAppearanceChanged()
        {
            AppearanceChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

