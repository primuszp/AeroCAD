using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PluginDiscoveryService : IPluginDiscoveryService
    {
        public PluginDiscoveryResult Discover(IEnumerable<Assembly> assemblies)
        {
            var modules = new List<ICadModule>();
            var plugins = new List<IEntityPlugin>();
            var issues = new List<PluginDiscoveryIssue>();

            foreach (var assembly in assemblies ?? Enumerable.Empty<Assembly>())
            {
                foreach (var type in GetLoadableTypes(assembly, issues))
                {
                    if (typeof(ICadModule).IsAssignableFrom(type))
                    {
                        var module = CreateInstance<ICadModule>(type, issues);
                        if (module != null)
                            modules.Add(module);
                    }

                    if (typeof(IEntityPlugin).IsAssignableFrom(type))
                    {
                        var plugin = CreateInstance<IEntityPlugin>(type, issues);
                        if (plugin != null)
                            plugins.Add(plugin);
                    }
                }
            }

            return new PluginDiscoveryResult(modules.AsReadOnly(), plugins.AsReadOnly(), issues.AsReadOnly());
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly, ICollection<PluginDiscoveryIssue> issues)
        {
            if (assembly == null)
                yield break;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type != null).ToArray();
                issues.Add(new PluginDiscoveryIssue(assembly.FullName, "Some plugin types could not be loaded.", ex));
                foreach (var loaderException in ex.LoaderExceptions ?? Array.Empty<Exception>())
                    issues.Add(new PluginDiscoveryIssue(assembly.FullName, "A plugin dependency could not be loaded.", loaderException));
            }
            catch (Exception ex)
            {
                issues.Add(new PluginDiscoveryIssue(assembly.FullName, "Assembly types could not be inspected.", ex));
                yield break;
            }

            foreach (var type in types)
            {
                if (type == null || type.IsAbstract || type.IsInterface)
                    continue;

                yield return type;
            }
        }

        private static T CreateInstance<T>(Type type, ICollection<PluginDiscoveryIssue> issues) where T : class
        {
            if (type == null || type.IsAbstract || type.IsInterface)
                return null;

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                issues.Add(new PluginDiscoveryIssue(type.FullName, "Plugin type has no public parameterless constructor."));
                return null;
            }

            try
            {
                return Activator.CreateInstance(type) as T;
            }
            catch (Exception ex)
            {
                issues.Add(new PluginDiscoveryIssue(type.FullName, "Plugin type could not be instantiated.", ex));
                return null;
            }
        }
    }
}
