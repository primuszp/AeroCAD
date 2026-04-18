using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Drawing;

namespace Primusz.AeroCAD.Core.Tools
{
    public class ToolService : IToolService
    {
        private readonly Dictionary<Guid, ITool> tools;
        private readonly Dictionary<Guid, int> registrationOrder;
        private List<ITool> orderedTools;
        private bool orderedToolsDirty;
        private readonly IServiceProvider provider;
        private int nextRegistrationOrder;

        public ToolService(IServiceProvider serviceProvider, IViewport viewport)
        {
            provider = serviceProvider;
            Viewport = viewport;
            tools = new Dictionary<Guid, ITool>();
            registrationOrder = new Dictionary<Guid, int>();
            orderedTools = new List<ITool>();
            orderedToolsDirty = true;
            InitializeService();
        }

        public IViewport Viewport { get; private set; }

        public IReadOnlyCollection<ITool> Tools => GetOrderedTools().AsReadOnly();

        private void InitializeService()
        {
            Viewport.KeyUp += KeyUp;
            Viewport.KeyDown += KeyDown;

            Viewport.PreviewMouseLeftButtonUp += MouseButtonUp;
            Viewport.PreviewMouseRightButtonUp += MouseButtonUp;
            Viewport.PreviewMouseLeftButtonDown += MouseButtonDown;
            Viewport.PreviewMouseRightButtonDown += MouseButtonDown;

            Viewport.MouseMove += MouseMove;
            Viewport.MouseWheel += MouseWheel;

            Keyboard.Focus(Viewport);
        }

        private IEnumerable<TListener> GetListeners<TListener>() where TListener : class
        {
            return GetOrderedTools()
                .OfType<TListener>();
        }

        private List<ITool> GetOrderedTools()
        {
            if (!orderedToolsDirty)
                return orderedTools;

            orderedTools = tools.Values
                .OrderByDescending(tool => tool.InputPriority)
                .ThenBy(tool => registrationOrder.TryGetValue(tool.Id, out int order) ? order : int.MaxValue)
                .ToList();
            orderedToolsDirty = false;
            return orderedTools;
        }

        private void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdatePointerPosition(e);
            foreach (var listener in GetListeners<IMouseListener>())
            {
                listener.MouseButtonDown(e);
                if (e.Handled) return;
            }
        }

        private void MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            UpdatePointerPosition(e);
            foreach (var listener in GetListeners<IMouseListener>())
            {
                listener.MouseButtonUp(e);
                if (e.Handled) return;
            }
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            UpdatePointerPosition(e);
            foreach (var listener in GetListeners<IMouseListener>())
            {
                listener.MouseWheel(e);
                if (e.Handled) return;
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            UpdatePointerPosition(e);
            foreach (var listener in GetListeners<IMouseListener>())
            {
                listener.MouseMove(e);
                if (e.Handled) return;
            }
        }

        private void UpdatePointerPosition(MouseEventArgs e)
        {
            if (e == null || Viewport == null)
                return;

            Point screenPoint = e.GetPosition(Viewport);
            Viewport.Position = Viewport.Unproject(screenPoint);
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                GetService<IUndoRedoService>()?.Undo();
                e.Handled = true;
                return;
            }
            if ((e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control) ||
                (e.Key == Key.Z && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
            {
                GetService<IUndoRedoService>()?.Redo();
                e.Handled = true;
                return;
            }

            foreach (var listener in GetListeners<IKeyboardListener>())
            {
                listener.KeyDown(e);
                if (e.Handled) return;
            }
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            foreach (var listener in GetListeners<IKeyboardListener>())
            {
                listener.KeyUp(e);
                if (e.Handled) return;
            }
        }

        public void RegisterTool(ITool tool)
        {
            if (tool == null || tools.ContainsKey(tool.Id))
                return;

            tools.Add(tool.Id, tool);
            registrationOrder[tool.Id] = nextRegistrationOrder++;
            orderedToolsDirty = true;
            tool.ToolService = this;
        }

        public void UnregisterTool(ITool tool)
        {
            if (tool == null)
                return;

            tools.Remove(tool.Id);
            registrationOrder.Remove(tool.Id);
            orderedToolsDirty = true;
        }

        public void SuspendAll()
        {
            foreach (ITool tool in tools.Values)
                tool.IsSuspended = true;
        }

        public void SuspendAll(ITool exclude)
        {
            foreach (ITool tool in tools.Values.Where(tool => exclude == null || tool.Id != exclude.Id))
                tool.IsSuspended = true;
        }

        public void UnsuspendAll()
        {
            foreach (ITool tool in tools.Values)
                tool.IsSuspended = false;
        }

        public ITool GetTool(Guid id)
        {
            return tools.ContainsKey(id) ? tools[id] : null;
        }

        public ITool GetTool(string name)
        {
            return tools.Values.FirstOrDefault(tool => tool.Name == name);
        }

        public TTool GetTool<TTool>() where TTool : class, ITool
        {
            return tools.Values.OfType<TTool>().FirstOrDefault();
        }

        public bool ActivateTool(Guid id)
        {
            return ActivateTool(GetTool(id));
        }

        public bool ActivateTool(ITool tool)
        {
            return tool != null && tool.CanActivate && tool.Activate();
        }

        public bool DeactivateTool(ITool tool)
        {
            return tool != null && tool.Enabled && tool.IsActive && tool.Deactivate();
        }

        public void DeactivateAll()
        {
            foreach (ITool tool in tools.Values)
                tool.Deactivate();
        }

        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return provider.GetService(serviceType);
        }
    }
}
