using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Tools
{
    public sealed class RegisteredInteractiveShapeTool : InteractiveCommandTool<IInteractiveCommandController>
    {
        public RegisteredInteractiveShapeTool(IInteractiveShapeRuntime runtime)
            : base(activeLayerResolver => runtime.CreateController(activeLayerResolver), runtime.ToolName)
        {
        }
    }
}
