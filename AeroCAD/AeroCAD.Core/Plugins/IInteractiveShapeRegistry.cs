using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface IInteractiveShapeRegistry
    {
        IReadOnlyList<IInteractiveShapeDefinition> Definitions { get; }
        IInteractiveShapeDefinition Find(string name);
    }
}
