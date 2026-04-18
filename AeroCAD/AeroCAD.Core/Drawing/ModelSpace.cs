using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class ModelSpace : IServiceProvider
    {
        private Dictionary<Type, object> services;
        private readonly ModelSpaceComposition composition;

        public ModelSpace(Viewport viewport)
        {
            composition = new ModelSpaceComposition(viewport);
        }

        public ModelSpace RegisterPlugin(IEntityPlugin plugin)
        {
            composition.RegisterPlugin(plugin);
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
