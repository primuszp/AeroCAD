using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpCadCore.Controls;
using WpCadCore.Tool;

namespace WpCadCore.Model
{
    class ModelSpaceDesigner : IModelSpaceDesigner
    {
        private Dictionary<Type, object> services;
        private ToolService toolService;

        public ModelSpace ModelSpaceView { get; private set; }

        public ModelSpaceDesigner(ModelSpace canvas)
        {
            this.ModelSpaceView = canvas;
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.services = new Dictionary<Type, object>();
            this.toolService = new ToolService(this, ModelSpaceView);
        }

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (this.services.ContainsKey(serviceType))
                return this.services[serviceType];
            return null;
        }

        #endregion
    }
}
