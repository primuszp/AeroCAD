using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Rendering
{
    public sealed class EntityRenderContext
    {
        public EntityRenderContext(Layer layer, Pen pen, Pen highlightPen, Pen highlightGlowPen)
        {
            Layer = layer;
            Pen = pen;
            HighlightPen = highlightPen;
            HighlightGlowPen = highlightGlowPen;
        }

        public Layer Layer { get; }

        public Pen Pen { get; }

        public Pen HighlightPen { get; }

        public Pen HighlightGlowPen { get; }
    }
}

