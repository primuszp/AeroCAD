namespace Primusz.AeroCAD.Core.Tools
{
    public class RectangleTool : InteractiveCommandTool<RectangleCommandController>
    {
        public RectangleTool()
            : base(layerProvider => new RectangleCommandController(layerProvider), "RectangleTool")
        {
        }
    }
}
