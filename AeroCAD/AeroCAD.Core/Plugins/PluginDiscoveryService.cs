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

            foreach (var assembly in assemblies ?? Enumerable.Empty<Assembly>())
            {
                foreach (var type in GetLoadableTypes(assembly))
                {
                    if (typeof(ICadModule).IsAssignableFrom(type))
                    {
                        var module = CreateInstance<ICadModule>(type);
                        if (module != null)
                            modules.Add(module);
                    }

                    if (typeof(IEntityPlugin).IsAssignableFrom(type))
                    {
                        var plugin = CreateInstance<IEntityPlugin>(type);
                        if (plugin != null)
                            plugins.Add(plugin);
                    }
                }
            }

            return new PluginDiscoveryResult(modules.AsReadOnly(), plugins.AsReadOnly());
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
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
            }

            foreach (var type in types)
            {
                if (type == null || type.IsAbstract || type.IsInterface)
                    continue;

                yield return type;
            }
        }

        private static T CreateInstance<T>(Type type) where T : class
        {
            if (type == null || type.IsAbstract || type.IsInterface)
                return null;

            if (type.GetConstructor(Type.EmptyTypes) == null)
                return null;

            try
            {
                return Activator.CreateInstance(type) as T;
            }
            catch
            {
                return null;
            }
        }
    }
}
