using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// Declarative description of a third-party interactive shape.
    /// This is the first step toward separating shape metadata from controller implementation.
    /// </summary>
    public interface IInteractiveShapeDefinition
    {
        IInteractiveShapePipeline Pipeline { get; }
        string Name { get; }
        string CommandName { get; }
        CommandStep InitialStep { get; }
        IReadOnlyList<CommandStep> Steps { get; }
        InteractiveCommandRegistration CreateCommandRegistration();
    }
}
