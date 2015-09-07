using System;
using System.Collections.Generic;
using System.Windows.Input;
using WpCadCore.Controls;

namespace WpCadCore.Tool
{
    class ToolService : IToolService
    {
        #region Fields

        private IServiceProvider hostProvider;
        private Dictionary<Guid, ITool> tools;

        #endregion

        public ToolService(IServiceProvider host, IModelSpace canvas)
        {
            this.hostProvider = host;
            this.ModelSpaceView = canvas;
            this.InitializeService();
        }

        #region Initialization

        private void InitializeService()
        {
            this.tools = new Dictionary<Guid, ITool>();

            this.ModelSpaceView.PreviewMouseRightButtonDown += new MouseButtonEventHandler(ModelSpaceView_ButtonDown);
            this.ModelSpaceView.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ModelSpaceView_ButtonDown);
            this.ModelSpaceView.PreviewMouseRightButtonUp += new MouseButtonEventHandler(ModelSpaceView_ButtonUp);
            this.ModelSpaceView.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(ModelSpaceView_ButtonUp);
            this.ModelSpaceView.MouseWheel += new MouseWheelEventHandler(ModelSpaceView_MouseWheel);
            this.ModelSpaceView.MouseMove += new MouseEventHandler(ModelSpaceView_MouseMove);
        }

        #region Input events binding

        private void ModelSpaceView_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (ITool tool in tools.Values)
            {
                if (tool is IMouseListener)
                {
                    ((IMouseListener)tool).MouseDown(e);
                    if (e.Handled) return;
                }
            }
        }

        private void ModelSpaceView_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (ITool tool in tools.Values)
            {
                if (tool is IMouseListener)
                {
                    ((IMouseListener)tool).MouseUp(e);
                    if (e.Handled) return;
                }
            }
        }

        private void ModelSpaceView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            foreach (ITool tool in tools.Values)
            {
                if (tool is IMouseListener)
                {
                    ((IMouseListener)tool).MouseWheel(e);
                    if (e.Handled) return;
                }
            }
        }

        private void ModelSpaceView_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (ITool tool in tools.Values)
            {
                if (tool is IMouseListener)
                {
                    ((IMouseListener)tool).MouseMove(e);
                    if (e.Handled) return;
                }
            }
        }

        #endregion

        #endregion

        #region IToolService Members

        public IModelSpace ModelSpaceView { get; private set; }

        public void RegisterTool(ITool tool)
        {
            if (tool == null) return;

            if (!tools.ContainsKey(tool.Id))
            {
                tools.Add(tool.Id, tool);
                tool.ToolService = this;
            }
        }

        public void UnregisterTool(ITool tool)
        {
            if (tool == null) return;

            if (tools.ContainsKey(tool.Id))
                tools.Remove(tool.Id);
        }

        public void SuspendAll()
        {
            foreach (ITool tool in tools.Values)
                tool.IsSuspended = true;
        }

        public void SuspendAll(ITool exclude)
        {
            foreach (ITool tool in tools.Values)
            {
                if (tool.Id != exclude.Id) tool.IsSuspended = true;
            }
        }

        public void UnsuspendAll()
        {
            foreach (ITool tool in tools.Values)
                tool.IsSuspended = false;
        }

        public ITool GetTool(Guid id)
        {
            if (tools.ContainsKey(id))
                return tools[id];
            else
                return null;
        }

        public ITool GetTool(string name)
        {
            foreach (ITool tool in tools.Values)
            {
                if (tool.Name == name) return tool;
            }
            return null;
        }

        public bool ActivateTool(Guid id)
        {
            ITool tool = GetTool(id);
            return ActivateTool(tool);
        }

        public bool ActivateTool(ITool tool)
        {
            if (tool != null && tool.CanActivate)
                return tool.Activate();
            else
                return false;
        }

        public bool DeactivateTool(ITool tool)
        {
            if (tool != null && tool.Enabled && tool.IsActive)
                return tool.Deactivate();
            else
                return false;
        }

        public void DeactivateAll()
        {
            foreach (ITool tool in tools.Values)
                tool.Deactivate();
        }

        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            return hostProvider.GetService(serviceType);
        }

        #endregion
    }
}
