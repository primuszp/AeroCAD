using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public class PickSettingsService : IPickSettingsService
    {
        private double pickBoxSizePixels = 8.0d;

        public double PickBoxSizePixels
        {
            get => pickBoxSizePixels;
            set => pickBoxSizePixels = value > 0 ? value : 8.0d;
        }

        public double GetPickRadiusWorld(double zoom)
        {
            double effectiveZoom = Math.Abs(zoom) < double.Epsilon ? 1.0d : Math.Abs(zoom);
            return (PickBoxSizePixels * 0.5d) / effectiveZoom;
        }
    }
}
