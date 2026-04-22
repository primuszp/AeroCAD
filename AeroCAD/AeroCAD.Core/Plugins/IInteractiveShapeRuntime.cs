using System;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface IInteractiveShapeRuntime
    {
        string CommandName { get; }
        string ToolName { get; }
        ITool CreateTool();
        InteractiveCommandRegistration CreateCommandRegistration();
    }
}
