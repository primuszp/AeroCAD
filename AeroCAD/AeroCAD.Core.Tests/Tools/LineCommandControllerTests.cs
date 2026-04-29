using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Tools
{
    public class LineCommandControllerTests
    {
        [Fact]
        public void Undo_AfterCreatedSegment_RemovesDocumentLine()
        {
            RunOnStaThread(() =>
            {
                var layer = new Layer();
                var document = new TestCadDocumentService(layer);
                var controller = new LineCommandController();
                var host = new TestHost(document, new TestUndoRedoService())
                {
                    CurrentStep = controller.InitialStep
                };

                controller.OnActivated(host);
                Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));
                Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("10,0", new Point(10, 0))));

                Assert.Single(document.EntitiesList);

                var undoResult = controller.TrySubmitToken(host, CommandInputToken.Keyword("U", "U"));

                Assert.True(undoResult.Handled);
                Assert.Empty(document.EntitiesList);
                Assert.Equal(RubberState.Start, host.RubberObject.CurrentState);
                Assert.Equal(new Point(0, 0), host.RubberObject.Start);
            });
        }

        [Fact]
        public void Undo_AfterOnlyFirstPoint_ClearsRubberLineAndReturnsToFirstPoint()
        {
            RunOnStaThread(() =>
            {
                var controller = new LineCommandController();
                var host = new TestHost(new TestCadDocumentService(new Layer()), new TestUndoRedoService())
                {
                    CurrentStep = controller.InitialStep
                };

                controller.OnActivated(host);
                Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));

                var undoResult = controller.TrySubmitToken(host, CommandInputToken.Keyword("U", "U"));

                Assert.True(undoResult.Handled);
                Assert.Equal("FirstPoint", undoResult.NextStep.Id);
                Assert.Equal(RubberState.Idle, host.RubberObject.CurrentState);
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

        private static void Apply(TestHost host, InteractiveCommandResult result)
        {
            Assert.True(result.Handled);
            if (result.NextStep != null)
                host.CurrentStep = result.NextStep;
        }

        private sealed class TestHost : IInteractiveCommandHost
        {
            public TestHost(ICadDocumentService document, IUndoRedoService undoRedo)
            {
                var viewport = new Viewport();
                RubberObject = new RubberObject(viewport, null);
                ToolService = new TestToolService(viewport, document, undoRedo);
            }

            public IToolService ToolService { get; }

            public RubberObject RubberObject { get; }

            public CommandStep CurrentStep { get; set; }

            public bool TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point)
            {
                point = token?.PointValue ?? default;
                return token?.Kind == CommandInputTokenKind.Point;
            }

            public bool TryResolveScalarInput(CommandInputToken token, out double scalar)
            {
                scalar = token?.ScalarValue ?? default;
                return token?.Kind == CommandInputTokenKind.Scalar;
            }

            public Point ResolveFinalPoint(Point? basePoint, Point rawPos) => rawPos;
            public void MoveToStep(CommandStep step) => CurrentStep = step;
            public void EndSession(string closingMessage = null) { }
            public void DeactivateTool() { }
            public void ReturnToSelectionMode() { }
            public bool ApplyResult(InteractiveCommandResult result) => result?.Handled == true;
        }

        private sealed class TestToolService : IToolService
        {
            private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

            public TestToolService(IViewport viewport, ICadDocumentService document, IUndoRedoService undoRedo)
            {
                Viewport = viewport;
                services[typeof(ICadDocumentService)] = document;
                services[typeof(IUndoRedoService)] = undoRedo;
            }

            public IViewport Viewport { get; }
            public IReadOnlyCollection<ITool> Tools => Array.Empty<ITool>();
            public object GetService(Type serviceType) => services.TryGetValue(serviceType, out var service) ? service : null;
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

            public TestCadDocumentService(Layer layer)
            {
                this.layer = layer;
            }

            public List<Entity> EntitiesList { get; } = new List<Entity>();
            public IReadOnlyList<Layer> Layers => new[] { layer };
            public IEnumerable<Entity> Entities => EntitiesList;
            public event EventHandler<LayerChangedEventArgs> LayerAdded { add { } remove { } }
            public event EventHandler<LayerChangedEventArgs> LayerRemoved { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityAdded { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityRemoved { add { } remove { } }
            public Layer CreateLayer(string name, Color color) => layer;
            public void AddLayer(Layer layer) { }
            public bool RemoveLayer(Guid layerId) => false;
            public Layer GetLayer(Guid layerId) => layer;
            public Layer GetLayerForEntity(Entity entity) => EntitiesList.Contains(entity) ? layer : null;
            public void AddEntity(Guid layerId, Entity entity) => EntitiesList.Add(entity);
            public void RemoveEntity(Entity entity) => EntitiesList.Remove(entity);
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
