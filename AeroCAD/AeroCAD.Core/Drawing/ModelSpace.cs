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
