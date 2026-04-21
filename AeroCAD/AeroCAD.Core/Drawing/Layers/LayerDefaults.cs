using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public sealed class LayerSeed
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public double LineWeight { get; set; }
        public LineStyle LineStyle { get; set; } = LineStyle.Solid;
        public bool IsVisible { get; set; } = true;
        public bool IsFrozen { get; set; }
        public bool IsLocked { get; set; }
    }

    public static class LayerDefaults
    {
        public static IReadOnlyList<LayerSeed> CreateAutoCadSeeds()
        {
            return new[]
            {
                new LayerSeed { Name = "0", Color = Colors.White, LineWeight = 0.13d },
                new LayerSeed { Name = "DEFPOINTS", Color = Colors.Red, LineWeight = 0.18d },
                new LayerSeed { Name = "DIMS", Color = Colors.Yellow, LineWeight = 0.25d },
                new LayerSeed { Name = "OBJECT", Color = Colors.Green, LineWeight = 0.35d },
                new LayerSeed { Name = "TEXT", Color = Colors.Cyan, LineWeight = 0.50d }
            }.ToList().AsReadOnly();
        }
    }
}
