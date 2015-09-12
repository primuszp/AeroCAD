using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Primusz.Cadves.Core.Drawing;

namespace Primusz.Cadves.Core.Tools
{
    class ToolService : IToolService
    {
        #region Fields

        private Dictionary<Guid, ITool> tools;
        private readonly IServiceProvider provider;

        #endregion

        #region Constructors

        public ToolService(IServiceProvider provider, IViewport viewport)
        {
            this.provider = provider;
            Viewport = viewport;
            InitializeService();
        }

        #endregion

        #region Initialization

        private void InitializeService()
        {
            tools = new Dictionary<Guid, ITool>();

            Viewport.KeyUp += KeyUp;
            Viewport.KeyDown += KeyDown;

            Viewport.PreviewMouseRightButtonUp += MouseButtonUp;
            Viewport.PreviewMouseLeftButtonUp += MouseButtonUp;
            Viewport.PreviewMouseRightButtonDown += MouseButtonDown;
            Viewport.PreviewMouseLeftButtonDown += MouseButtonDown;
            Viewport.MouseWheel += MouseWheel;
            Viewport.MouseMove += MouseMove;

            Keyboard.Focus(Viewport);
        }

        #endregion

        #region Binding input events

        private void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var listener in tools.Values.OfType<IMouseListener>())
            {
                listener.MouseButtonDown(e);
                if (e.Handled) return;
            }
        }

        private void MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (IMouseListener listener in tools.Values.OfType<IMouseListener>())
            {
                listener.MouseButtonUp(e);
                if (e.Handled) return;
            }
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            foreach (IMouseListener listener in tools.Values.OfType<IMouseListener>())
            {
                listener.MouseWheel(e);
                if (e.Handled) return;
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            foreach (IMouseListener listener in tools.Values.OfType<IMouseListener>())
            {
                listener.MouseMove(e);
                if (e.Handled) return;
            }
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            foreach (var listener in tools.Values.OfType<IKeyboardListener>())
            {
                listener.KeyDown(e);
                if (e.Handled) return;
            }
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            foreach (var listener in tools.Values.OfType<IKeyboardListener>())
            {
                listener.KeyUp(e);
                if (e.Handled) return;
            }
        }

        #endregion

        #region IToolService Members

        public IViewport Viewport { get; private set; }

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
            {
                tools.Remove(tool.Id);
            }
        }

        public void SuspendAll()
        {
            foreach (ITool tool in tools.Values)
            {
                tool.IsSuspended = true;
            }
        }

        public void SuspendAll(ITool exclude)
        {
            foreach (ITool tool in tools.Values.Where(tool => tool.Id != exclude.Id))
            {
                tool.IsSuspended = true;
            }
        }

        public void UnsuspendAll()
        {
            foreach (ITool tool in tools.Values)
            {
                tool.IsSuspended = false;
            }
        }

        public ITool GetTool(Guid id)
        {
            return tools.ContainsKey(id) ? tools[id] : null;
        }

        public ITool GetTool(string name)
        {
            return tools.Values.FirstOrDefault(tool => tool.Name == name);
        }

        public bool ActivateTool(Guid id)
        {
            return ActivateTool(GetTool(id));
        }

        public bool ActivateTool(ITool tool)
        {
            if (tool != null && tool.CanActivate)
                return tool.Activate();
            return false;
        }

        public bool DeactivateTool(ITool tool)
        {
            if (tool != null && tool.Enabled && tool.IsActive)
                return tool.Deactivate();
            return false;
        }

        public void DeactivateAll()
        {
            foreach (ITool tool in tools.Values)
            {
                tool.Deactivate();
            }
        }

        #endregion

        #region From IServiceProvider interface

        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return provider.GetService(serviceType);
        }

        #endregion
    }
}
