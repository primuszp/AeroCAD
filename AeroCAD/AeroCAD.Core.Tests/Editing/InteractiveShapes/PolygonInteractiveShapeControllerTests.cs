using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class PolygonInteractiveShapeControllerTests
    {
        [Fact]
        public void RadiusStep_ClickCreatesPolygonUsingCenterToClickDistance()
        {
            RunOnStaThread(() =>
            {
                var layer = new Layer();
                var document = new TestCadDocumentService(layer);
                var controller = new PolygonInteractiveShapeController();
                var host = new TestHost(document) { CurrentStep = controller.InitialStep };

                var sides = controller.TrySubmitToken(host, CommandInputToken.Scalar("4", 4d));
                host.CurrentStep = sides.NextStep;

                var center = controller.TrySubmitViewportPoint(host, new Point(0, 0));
                host.CurrentStep = center.NextStep;

                var created = controller.TrySubmitViewportPoint(host, new Point(10, 0));

                Assert.True(created.Handled);
                var polygon = Assert.IsType<Polyline>(Assert.Single(document.Added));
                Assert.Equal(5, polygon.Points.Count);
                Assert.Contains(polygon.Points, point => Math.Abs((point - new Point(0, 0)).Length - 10d) < 1e-6);
            });
        }

        private static void RunOnStaThread(Action action)
        {
            Exception exception = null;
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;
        }

        private sealed class TestHost : IInteractiveCommandHost
        {
            public TestHost(ICadDocumentService document)
            {
                ToolService = new TestToolService(document);
            }

            public IToolService ToolService { get; }
            public CommandStep CurrentStep { get; set; }
            public bool TryResolveScalarInput(CommandInputToken token, out double scalar)
            {
                scalar = token?.ScalarValue ?? 0d;
                return token?.ScalarValue != null;
            }

            public bool TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point)
            {
                point = token?.PointValue ?? default;
                return token?.PointValue != null;
            }

            public Point ResolveFinalPoint(Point? basePoint, Point rawPos) => rawPos;
            public void MoveToStep(CommandStep step) { CurrentStep = step; }
            public void EndSession(string closingMessage = null) { }
            public void DeactivateTool() { }
            public void ReturnToSelectionMode() { }
            public bool ApplyResult(InteractiveCommandResult result) => false;
        }

        private sealed class TestToolService : IToolService
        {
            private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

            public TestToolService(ICadDocumentService document)
            {
                var viewport = new Viewport();
                _ = new RubberObject(viewport, null);

                Viewport = viewport;
                services[typeof(ICadDocumentService)] = document;
                services[typeof(IUndoRedoService)] = new TestUndoRedoService();
            }

            public IViewport Viewport { get; }
            public IReadOnlyCollection<ITool> Tools => Array.Empty<ITool>();
            public object GetService(Type serviceType) => services.TryGetValue(serviceType, out var value) ? value : null;
            public T GetService<T>() where T : class => GetService(typeof(T)) as T;
            public void RegisterTool(ITool tool) { }
            public void UnregisterTool(ITool tool) { }
            public void SuspendAll() { }
            public void SuspendAll(ITool exclude) { }
            public void UnsuspendAll() { }
            public ITool GetTool(Guid id) => null;
            public ITool GetTool(string name) => null;
            public TTool GetTool<TTool>() where TTool : class, ITool => null;
            public bool ActivateTool(Guid id) => false;
            public bool ActivateTool(ITool tool) => false;
            public void DeactivateAll() { }
            public bool DeactivateTool(ITool tool) => false;
        }

        private sealed class TestCadDocumentService : ICadDocumentService
        {
            private readonly Layer layer;
            public List<Entity> Added { get; } = new List<Entity>();

            public TestCadDocumentService(Layer layer)
            {
                this.layer = layer;
            }

            public IReadOnlyList<Layer> Layers => new[] { layer };
            public IEnumerable<Entity> Entities => Added;
            public event EventHandler<LayerChangedEventArgs> LayerAdded { add { } remove { } }
            public event EventHandler<LayerChangedEventArgs> LayerRemoved { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityAdded { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityRemoved { add { } remove { } }
            public Layer CreateLayer(string name, System.Windows.Media.Color color) => layer;
            public void AddLayer(Layer layer) { }
            public bool RemoveLayer(Guid layerId) => false;
            public Layer GetLayer(Guid layerId) => layer;
            public Layer GetLayerForEntity(Entity entity) => layer;
            public void AddEntity(Guid layerId, Entity entity) => Added.Add(entity);
            public void RemoveEntity(Entity entity) => Added.Remove(entity);
        }

        private sealed class TestUndoRedoService : IUndoRedoService
        {
            public bool CanUndo => false;
            public bool CanRedo => false;
            public string UndoDescription => string.Empty;
            public string RedoDescription => string.Empty;
            public event EventHandler StateChanged { add { } remove { } }
            public void Execute(IUndoableCommand command) => command.Execute();
            public void PushCompleted(IUndoableCommand command) { }
            public void Undo() { }
            public void Redo() { }
            public void Clear() { }
        }
    }
}
