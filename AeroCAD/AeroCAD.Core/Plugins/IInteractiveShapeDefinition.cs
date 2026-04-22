using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// Declarative description of a third-party interactive shape.
    /// This is the first step toward separating shape metadata from controller implementation.
    /// </summary>
    public interface IInteractiveShapeDefinition
    {
        string Name { get; }

        string CommandName { get; }

        CommandStep InitialStep { get; }

        IReadOnlyList<CommandStep> Steps { get; }

        Func<Func<Layer>, IInteractiveCommandController> ControllerFactory { get; }

        InteractiveCommandRegistration CreateCommandRegistration();
    }
}
