using System;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    /// <summary>
    /// AutoCAD-standard lineweight values in millimetres and their model-space pixel mapping.
    /// In Model Space lineweights are zoom-independent: 0.25 mm = 1 screen pixel at display scale 1.
    /// </summary>
    public static class LineWeightPalette
    {
        /// <summary>
        /// Pixels per millimetre at display scale 1.0 (matches AutoCAD's default: 0.25 mm → 1 px).
        /// </summary>
        public const double PixelsPerMm = 4.0;

        /// <summary>AutoCAD standard lineweight values in mm (ascending order).</summary>
        public static readonly double[] StandardValues =
        {
            0.05, 0.09, 0.13, 0.15, 0.18, 0.20,
            0.25, 0.30, 0.35, 0.40, 0.50, 0.53,
            0.60, 0.70, 0.80, 0.90, 1.00, 1.06,
            1.20, 1.40, 1.58, 2.00, 2.11
        };

        /// <summary>Default lineweight (0.25 mm → 1 px in model space).</summary>
        public const double Default = 0.25;

        /// <summary>
        /// Converts a lineweight in mm to screen-space pixels at the given display scale.
        /// </summary>
        public static double ToScreenPixels(double lineWeightMm, double displayScale = 1.0)
        {
            return Math.Max(lineWeightMm, 0.05) * PixelsPerMm * displayScale;
        }

        /// <summary>
        /// Snaps <paramref name="value"/> to the nearest standard AutoCAD lineweight.
        /// </summary>
        public static double Snap(double value)
        {
            double best = StandardValues[0];
            double bestDist = Math.Abs(value - best);

            foreach (var v in StandardValues)
            {
                double dist = Math.Abs(value - v);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = v;
                }
            }

            return best;
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> is within the valid lineweight range.
        /// </summary>
        public static bool IsValid(double value) => value >= 0.05 && value <= 2.11;
    }
}
