using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Drawing.Markers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Snapping;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Drawing
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
            var composition = new ModelSpaceComposition(viewport);
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
            return services.ContainsKey(serviceType) ? services[serviceType] : null;
        }

        #endregion
    }
}

