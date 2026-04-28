using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class ModelSpace : IServiceProvider
    {
        private Dictionary<Type, object> services;
        private readonly ModelSpaceComposition composition;
        private readonly List<ICadModule> modules = new List<ICadModule>();
        private readonly List<PluginDiscoveryIssue> extensionDiscoveryIssues = new List<PluginDiscoveryIssue>();

        public ModelSpace(Viewport viewport)
        {
            composition = new ModelSpaceComposition(viewport);
            RegisterModule(new BuiltInGeometryModule());
            RegisterModule(new BuiltInModifyModule());
        }

        /// <summary>
        /// Registered modules, available after Initialize() for diagnostics or UI (e.g. "About" dialogs).
        /// </summary>
        public IReadOnlyList<ICadModule> Modules => modules.AsReadOnly();

        public IReadOnlyList<PluginDiscoveryIssue> ExtensionDiscoveryIssues => extensionDiscoveryIssues.AsReadOnly();

        public ModelSpace RegisterPlugin(IEntityPlugin plugin)
        {
            composition.RegisterPlugin(plugin);
            return this;
        }

        public ModelSpace LoadExtensionsFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            var discovery = new PluginDiscoveryService();
            var result = discovery.Discover(assemblies);

            foreach (var module in result.Modules)
                RegisterModule(module);

            foreach (var plugin in result.Plugins)
                RegisterPlugin(plugin);

            extensionDiscoveryIssues.AddRange(result.Issues);
            return this;
        }

        public ModelSpace LoadExtensionsFromDirectory(string directory, string searchPattern = "*.dll")
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return this;

            var assemblies = Directory.GetFiles(directory, searchPattern)
                .Select(path =>
                {
                    try
                    {
                        return Assembly.LoadFrom(path);
                    }
                    catch (Exception ex)
                    {
                        extensionDiscoveryIssues.Add(new PluginDiscoveryIssue(path, "Extension assembly could not be loaded.", ex));
                        return null;
                    }
                })
                .Where(assembly => assembly != null)
                .ToArray();

            return LoadExtensionsFromAssemblies(assemblies);
        }

        /// <summary>
        /// Registers all plugins contained in the module and tracks the module for identification.
        /// </summary>
        public ModelSpace RegisterModule(ICadModule module)
        {
            if (module == null) return this;
            modules.Add(module);
            composition.RegisterModule(module);
            foreach (var plugin in module.Plugins ?? Enumerable.Empty<IEntityPlugin>())
                RegisterPlugin(plugin);
            return this;
        }

        public void Initialize()
        {
            services = composition.BuildServices();
            composition.Bootstrap();
        }

        /// <summary>
        /// Registers a custom service after Initialize() has been called.
        /// Both the interface type and the concrete type are registered so that
        /// GetService&lt;TInterface&gt;() and GetService&lt;TConcrete&gt;() both resolve.
        /// </summary>
        public ModelSpace RegisterService<TInterface, TConcrete>(TConcrete instance)
            where TInterface : class
            where TConcrete : class, TInterface
        {
            if (services == null)
                throw new InvalidOperationException("Call Initialize() before registering additional services.");

            services[typeof(TInterface)] = instance;
            services[typeof(TConcrete)] = instance;
            return this;
        }

        /// <summary>
        /// Registers a custom service after Initialize() has been called, keyed by its concrete type only.
        /// </summary>
        public ModelSpace RegisterService<TConcrete>(TConcrete instance) where TConcrete : class
        {
            if (services == null)
                throw new InvalidOperationException("Call Initialize() before registering additional services.");

            services[typeof(TConcrete)] = instance;
            return this;
        }

        #region IServiceProvider

        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return services != null && services.ContainsKey(serviceType) ? services[serviceType] : null;
        }

        #endregion
    }
}
