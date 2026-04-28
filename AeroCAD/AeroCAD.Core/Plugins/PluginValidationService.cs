using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginValidationService
    {
        private readonly Version coreVersion;

        public PluginValidationService()
            : this(typeof(PluginValidationService).GetTypeInfo().Assembly.GetName().Version)
        {
        }

        public PluginValidationService(Version coreVersion)
        {
            this.coreVersion = coreVersion ?? new Version(0, 0);
        }

        public PluginValidationResult Validate(IEnumerable<IEntityPlugin> plugins, IEnumerable<ICadModule> modules)
        {
            var issues = new List<PluginValidationIssue>();
            var pluginArray = (plugins ?? Enumerable.Empty<IEntityPlugin>()).Where(plugin => plugin != null).ToArray();
            var moduleArray = (modules ?? Enumerable.Empty<ICadModule>()).Where(module => module != null).ToArray();

            var descriptors = ValidatePluginDescriptors(pluginArray, issues);
            ValidateModuleManifests(moduleArray, issues);
            ValidateCommandCollisions(descriptors, moduleArray, issues);

            return new PluginValidationResult(issues);
        }

        private static IReadOnlyList<EntityPluginDescriptor> ValidatePluginDescriptors(IEnumerable<IEntityPlugin> plugins, List<PluginValidationIssue> issues)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var descriptors = new List<EntityPluginDescriptor>();

            foreach (var plugin in plugins)
            {
                EntityPluginDescriptor descriptor;
                try
                {
                    descriptor = plugin.Descriptor;
                }
                catch (Exception ex)
                {
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, $"Plugin descriptor could not be read: {ex.Message}", plugin.GetType().FullName));
                    continue;
                }

                if (descriptor == null)
                {
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, "Plugin descriptor is null.", plugin.GetType().FullName));
                    continue;
                }

                descriptors.Add(descriptor);

                if (descriptor.RenderStrategy == null)
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, $"Plugin '{descriptor.Name}' is missing a render strategy.", descriptor.Name));

                if (descriptor.BoundsStrategy == null)
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, $"Plugin '{descriptor.Name}' is missing a bounds strategy.", descriptor.Name));

                if (names.TryGetValue(descriptor.Name, out var existing))
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, $"Duplicate entity plugin name '{descriptor.Name}' in '{existing}' and '{plugin.GetType().FullName}'.", descriptor.Name));
                else
                    names[descriptor.Name] = plugin.GetType().FullName;
            }

            return descriptors;
        }

        private void ValidateModuleManifests(IEnumerable<ICadModule> modules, List<PluginValidationIssue> issues)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var module in modules)
            {
                if (string.IsNullOrWhiteSpace(module.Name))
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, "Module name is required.", module.GetType().FullName));
                else if (!names.Add(module.Name))
                    issues.Add(new PluginValidationIssue(PluginValidationIssueSeverity.Error, $"Duplicate module name '{module.Name}'.", module.Name));

                var manifest = (module as IPluginManifestProvider)?.Manifest ?? PluginManifest.FromModule(module);
                if (manifest?.MinimumCoreVersion != null && manifest.MinimumCoreVersion > coreVersion)
                {
                    issues.Add(new PluginValidationIssue(
                        PluginValidationIssueSeverity.Error,
                        $"Module '{module.Name}' requires AeroCAD Core {manifest.MinimumCoreVersion} or newer, current version is {coreVersion}.",
                        module.Name));
                }
            }
        }

        private static void ValidateCommandCollisions(IEnumerable<EntityPluginDescriptor> descriptors, IEnumerable<ICadModule> modules, List<PluginValidationIssue> issues)
        {
            var owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var descriptor in descriptors)
            {
                foreach (var registration in descriptor.InteractiveCommands ?? Enumerable.Empty<InteractiveCommandRegistration>())
                    RegisterCommandAliases(registration.CreateCommandDefinition(), $"plugin '{descriptor.Name}'", owners, issues);

                foreach (var definition in descriptor.Commands ?? Enumerable.Empty<EditorCommandDefinition>())
                    RegisterCommandAliases(definition, $"plugin '{descriptor.Name}'", owners, issues);
            }

            foreach (var module in modules)
            {
                foreach (var registration in module.InteractiveCommands ?? Enumerable.Empty<InteractiveCommandRegistration>())
                    RegisterCommandAliases(registration.CreateCommandDefinition(), $"module '{module.Name}'", owners, issues);

                foreach (var shape in module.Shapes ?? Enumerable.Empty<IInteractiveShapeDefinition>())
                {
                    var registration = shape?.Pipeline?.CreateRuntime()?.CreateCommandRegistration();
                    if (registration != null)
                        RegisterCommandAliases(registration.CreateCommandDefinition(), $"shape in module '{module.Name}'", owners, issues);
                }

                foreach (var definition in module.Commands ?? Enumerable.Empty<EditorCommandDefinition>())
                    RegisterCommandAliases(definition, $"module '{module.Name}'", owners, issues);
            }
        }

        private static void RegisterCommandAliases(EditorCommandDefinition definition, string owner, Dictionary<string, string> owners, List<PluginValidationIssue> issues)
        {
            if (definition == null)
                return;

            foreach (var alias in definition.Aliases ?? Enumerable.Empty<string>())
            {
                if (owners.TryGetValue(alias, out var existingOwner))
                {
                    if (!definition.ReplaceExistingCommand)
                    {
                        issues.Add(new PluginValidationIssue(
                            PluginValidationIssueSeverity.Error,
                            $"Command alias '{alias}' from {owner} conflicts with {existingOwner}.",
                            owner));
                    }
                }
                else
                {
                    owners[alias] = owner;
                }
            }
        }
    }
}
