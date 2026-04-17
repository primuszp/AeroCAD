using System;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editor
{
    public class GridSettingsService : IGridSettingsService
    {
        private bool isEnabled;
        private double minorSpacingX = 10.0d;
        private double minorSpacingY = 10.0d;
        private int majorLineEvery = 5;
        private Color minorLineColor = Color.FromRgb(50, 55, 72);
        private Color majorLineColor = Color.FromRgb(73, 79, 105);
        private double minimumScreenSpacing = 20.0d;

        public bool IsEnabled
        {
            get { return isEnabled; }
            private set
            {
                if (isEnabled == value)
                    return;

                isEnabled = value;
                OnStateChanged();
            }
        }

        public double MinorSpacingX
        {
            get { return minorSpacingX; }
            set
            {
                double sanitized = value > 0 ? value : 10.0d;
                if (Math.Abs(minorSpacingX - sanitized) < double.Epsilon)
                    return;

                minorSpacingX = sanitized;
                OnStateChanged();
            }
        }

        public double MinorSpacingY
        {
            get { return minorSpacingY; }
            set
            {
                double sanitized = value > 0 ? value : 10.0d;
                if (Math.Abs(minorSpacingY - sanitized) < double.Epsilon)
                    return;

                minorSpacingY = sanitized;
                OnStateChanged();
            }
        }

        public int MajorLineEvery
        {
            get { return majorLineEvery; }
            set
            {
                int sanitized = value > 0 ? value : 5;
                if (majorLineEvery == sanitized)
                    return;

                majorLineEvery = sanitized;
                OnStateChanged();
            }
        }

        public Color MinorLineColor
        {
            get { return minorLineColor; }
            set
            {
                if (minorLineColor == value)
                    return;

                minorLineColor = value;
                OnStateChanged();
            }
        }

        public Color MajorLineColor
        {
            get { return majorLineColor; }
            set
            {
                if (majorLineColor == value)
                    return;

                majorLineColor = value;
                OnStateChanged();
            }
        }

        public double MinimumScreenSpacing
        {
            get { return minimumScreenSpacing; }
            set
            {
                double sanitized = value > 1 ? value : 20.0d;
                if (Math.Abs(minimumScreenSpacing - sanitized) < double.Epsilon)
                    return;

                minimumScreenSpacing = sanitized;
                OnStateChanged();
            }
        }

        public event EventHandler StateChanged;

        public GridSettingsService()
        {
            isEnabled = true;
        }

        public void Toggle()
        {
            IsEnabled = !IsEnabled;
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

