using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;

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
        bool ReplaceExistingCommand { get; }
        CommandStep InitialStep { get; }
        InteractiveCommandRegistration CreateCommandRegistration();
        IInteractiveShapeRuntime CreateRuntime();
    }
}
