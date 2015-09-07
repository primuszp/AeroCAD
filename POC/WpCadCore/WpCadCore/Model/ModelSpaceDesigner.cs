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

        public ModelSpaceDesigner(ModelSpace surface)
        {
            this.ModelSpaceView = surface;
            this.InitializeComponent();
        }


        private void InitializeComponent()
        {
            this.services = new Dictionary<Type, object>();
            this.toolService = new ToolService(this, ModelSpaceView);

            toolService.RegisterTool(new PanZoomTool());
            toolService.RegisterTool(new SelectionTool());
            toolService.RegisterTool(new MoveTool());

            toolService.GetTool("PanZoomTool").Activate();
            toolService.GetTool("SelectionTool").Activate();
            toolService.GetTool("MoveTool").Activate();
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
