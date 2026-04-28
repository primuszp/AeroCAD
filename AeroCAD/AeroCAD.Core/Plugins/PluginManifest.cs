using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginManifest
    {
        public PluginManifest(
            string id,
            string displayName = null,
            string version = null,
            Version minimumCoreVersion = null,
            IEnumerable<string> commandNames = null,
            IEnumerable<string> entityPluginNames = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Plugin manifest id is required.", nameof(id));

            Id = id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            Version = string.IsNullOrWhiteSpace(version) ? "1.0.0" : version.Trim();
            MinimumCoreVersion = minimumCoreVersion;
            CommandNames = Normalize(commandNames);
            EntityPluginNames = Normalize(entityPluginNames);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Version { get; }
        public Version MinimumCoreVersion { get; }
        public IReadOnlyList<string> CommandNames { get; }
        public IReadOnlyList<string> EntityPluginNames { get; }

        public static PluginManifest FromModule(ICadModule module)
        {
            if (module == null)
                return null;

            return new PluginManifest(module.Name, module.Name, module.Version);
        }

        private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim().ToUpperInvariant())
                .Distinct()
                .ToArray();
        }
    }
}
