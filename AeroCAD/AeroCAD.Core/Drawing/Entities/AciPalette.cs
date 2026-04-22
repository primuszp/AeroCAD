using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    /// <summary>
    /// AutoCAD Color Index (ACI) palette — the standard 256-color table.
    /// Index 0 = ByBlock placeholder, 1–255 = actual colors.
    /// </summary>
    public static class AciPalette
    {
        private static readonly Color[] palette = BuildPalette();

        public static Color GetColor(byte index) => palette[index];

        public static byte GetIndex(Color color)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                if (palette[i] == color)
                    return (byte)i;
            }

            return 0;
        }

        public static bool TryGetIndex(Color color, out byte index)
        {
            index = GetIndex(color);
            return index != 0 || palette[0] == color;
        }

        public static Color NormalizeColor(Color color)
        {
            if (TryGetIndex(color, out byte exactIndex))
                return palette[exactIndex];

            int bestDistance = int.MaxValue;
            byte bestIndex = 7;

            // Skip index 0 because it is the ByBlock placeholder, not a real selectable color.
            for (byte i = 1; i < palette.Length; i++)
            {
                Color candidate = palette[i];
                int dr = color.R - candidate.R;
                int dg = color.G - candidate.G;
                int db = color.B - candidate.B;
                int distance = (dr * dr) + (dg * dg) + (db * db);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return palette[bestIndex];
        }

        private static Color[] BuildPalette()
        {
            var colors = new Color[256];

            colors[0] = Colors.Black;          // ByBlock placeholder
            colors[1] = Color.FromRgb(255, 0, 0);
            colors[2] = Color.FromRgb(255, 255, 0);
            colors[3] = Color.FromRgb(0, 255, 0);
            colors[4] = Color.FromRgb(0, 255, 255);
            colors[5] = Color.FromRgb(0, 0, 255);
            colors[6] = Color.FromRgb(255, 0, 255);
            colors[7] = Color.FromRgb(255, 255, 255);
            colors[8] = Color.FromRgb(65, 65, 65);
            colors[9] = Color.FromRgb(128, 128, 128);

            // 24 hue groups (10–249): each group has 10 shades (5 even = pure, 5 odd = tinted)
            var primaryHues = new (byte R, byte G, byte B)[]
            {
                (255,   0,   0), // 10  Red
                (255,  63,   0), // 20  Red-Orange
                (255, 127,   0), // 30  Orange
                (255, 191,   0), // 40  Yellow-Orange
                (255, 255,   0), // 50  Yellow
                (191, 255,   0), // 60  Yellow-Green
                (127, 255,   0), // 70  Lime
                ( 63, 255,   0), // 80  Green-Lime
                (  0, 255,   0), // 90  Green
                (  0, 255,  63), // 100 Cyan-Green
                (  0, 255, 127), // 110 Aqua
                (  0, 255, 191), // 120 Aqua-Cyan
                (  0, 255, 255), // 130 Cyan
                (  0, 191, 255), // 140 Blue-Cyan
                (  0, 127, 255), // 150 Blue-Light
                (  0,  63, 255), // 160 Blue-Light2
                (  0,   0, 255), // 170 Blue
                ( 63,   0, 255), // 180 Blue-Violet
                (127,   0, 255), // 190 Violet
                (191,   0, 255), // 200 Purple
                (255,   0, 255), // 210 Magenta
                (255,   0, 191), // 220 Magenta-Red
                (255,   0, 127), // 230 Red-Magenta
                (255,   0,  63), // 240 Red-Dark
            };

            // Brightness levels for the 5 shade pairs: full → ~65% → 50% → 30% → 15%
            int[] levels = { 255, 165, 127, 76, 38 };

            for (int hue = 0; hue < 24; hue++)
            {
                var (pr, pg, pb) = primaryHues[hue];
                int baseIndex = 10 + hue * 10;

                for (int shade = 0; shade < 5; shade++)
                {
                    int level = levels[shade];
                    double scale = level / 255.0;

                    // Even index: pure shade at brightness level
                    byte r = (byte)(pr * scale);
                    byte g = (byte)(pg * scale);
                    byte b = (byte)(pb * scale);
                    colors[baseIndex + shade * 2] = Color.FromRgb(r, g, b);

                    // Odd index: tint — midpoint between the pure shade and a neutral gray at the same level
                    colors[baseIndex + shade * 2 + 1] = Color.FromRgb(
                        (byte)((r + level) / 2),
                        (byte)((g + level) / 2),
                        (byte)((b + level) / 2));
                }
            }

            // Grayscale ramp 250–255
            colors[250] = Color.FromRgb(51, 51, 51);
            colors[251] = Color.FromRgb(80, 80, 80);
            colors[252] = Color.FromRgb(128, 128, 128);
            colors[253] = Color.FromRgb(179, 179, 179);
            colors[254] = Color.FromRgb(204, 204, 204);
            colors[255] = Color.FromRgb(255, 255, 255);

            return colors;
        }
    }
}
