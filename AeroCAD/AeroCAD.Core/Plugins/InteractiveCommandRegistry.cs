using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveCommandRegistry : IInteractiveCommandRegistry
    {
        private readonly IReadOnlyList<InteractiveCommandRegistration> registrations;

        public InteractiveCommandRegistry(IEnumerable<IEntityPlugin> plugins, IEnumerable<ICadModule> modules)
        {
            registrations = (plugins ?? Enumerable.Empty<IEntityPlugin>())
                .SelectMany(plugin => plugin?.Descriptor?.InteractiveCommands ?? Enumerable.Empty<InteractiveCommandRegistration>())
                .Concat((modules ?? Enumerable.Empty<ICadModule>())
                    .SelectMany(module => module?.InteractiveCommands ?? Enumerable.Empty<InteractiveCommandRegistration>()))
                .ToArray();
        }

        public IReadOnlyList<InteractiveCommandRegistration> Registrations => registrations;

        public InteractiveCommandRegistration Find(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return null;

            return registrations.FirstOrDefault(registration => string.Equals(registration.CommandName, commandName.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
