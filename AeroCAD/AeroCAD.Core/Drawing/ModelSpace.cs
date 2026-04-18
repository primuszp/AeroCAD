using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class ModelSpace : IServiceProvider
    {
        private Dictionary<Type, object> services;
        private readonly ModelSpaceComposition composition;
        private readonly List<ICadModule> modules = new List<ICadModule>();

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

        public ModelSpace RegisterPlugin(IEntityPlugin plugin)
        {
            composition.RegisterPlugin(plugin);
            return this;
        }

        /// <summary>
        /// Registers all plugins contained in the module and tracks the module for identification.
        /// </summary>
        public ModelSpace RegisterModule(ICadModule module)
        {
            if (module == null) return this;
            modules.Add(module);
            composition.RegisterModule(module);
            foreach (var plugin in module.Plugins ?? System.Linq.Enumerable.Empty<IEntityPlugin>())
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
                throw new System.InvalidOperationException("Call Initialize() before registering additional services.");

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
                throw new System.InvalidOperationException("Call Initialize() before registering additional services.");

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
