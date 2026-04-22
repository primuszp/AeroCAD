using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface IInteractiveShapePipeline
    {
        string Name { get; }
        string CommandName { get; }
        IReadOnlyList<CommandStep> Steps { get; }
        string[] Aliases { get; }
        string Description { get; }
        bool AssignActiveLayer { get; }
        string MenuGroup { get; }
        string MenuLabel { get; }
        CommandStep InitialStep { get; }
        InteractiveCommandRegistration CreateCommandRegistration();
        IInteractiveCommandController CreateController(System.Func<Layer> activeLayerResolver);
    }
}
