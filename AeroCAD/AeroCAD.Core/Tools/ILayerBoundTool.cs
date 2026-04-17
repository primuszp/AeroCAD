using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Tools
{
    public interface ILayerBoundTool
    {
        Layer ActiveLayer { get; set; }
    }
}
