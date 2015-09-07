using System;
using System.Collections.Generic;
using Primusz.Cadves.Core.Drawing.Layers;
using Primusz.Cadves.Core.Tools;

namespace Primusz.Cadves.Core.Drawing
{
    public class ModelSpace : IServiceProvider
    {
        private Dictionary<Type, object> services;
        private readonly Viewport viewport;

        public ModelSpace(Viewport viewport)
        {
            this.viewport = viewport;
            InitializeServices();
        }

        private void InitializeServices()
        {
            services = new Dictionary<Type, object>
            {
                { typeof(ToolService), new ToolService(this, viewport) },
                { typeof(RubberObject), new RubberObject(viewport) }
            };

            var service = GetService<ToolService>();

            service.RegisterTool(new PanZoomTool());
            service.RegisterTool(new SelectionTool());
            service.GetTool("PanZoomTool").Activate();
            service.GetTool("SelectionTool").Activate();
        }

        #region IServiceProvider Members

        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return services.ContainsKey(serviceType) ? services[serviceType] : null;
        }

        #endregion
    }
}
