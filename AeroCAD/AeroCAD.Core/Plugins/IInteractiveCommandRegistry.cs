using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public interface IInteractiveCommandRegistry
    {
        IReadOnlyList<InteractiveCommandRegistration> Registrations { get; }
        InteractiveCommandRegistration Find(string commandName);
    }
}
