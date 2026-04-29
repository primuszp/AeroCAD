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
    public class PointSequenceCommandControllerBaseTests
    {
        [Fact]
        public void Complete_WithMinimumPoints_AddsEntityThroughUndoRedo()
        {
            RunOnStaThread(() =>
            {
                var document = new TestCadDocumentService(new Layer());
                var undoRedo = new TestUndoRedoService();
                var controller = new TestPointSequenceController();
                var host = new TestHost(document, undoRedo)
                {
                    CurrentStep = controller.InitialStep
                };

                controller.OnActivated(host);
                Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("1,2", new Point(1, 2))));
                Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("3,4", new Point(3, 4))));
                var result = controller.TryComplete(host);

                Assert.True(result.Handled);
                Assert.Single(document.AddedEntities);
                Assert.Equal(1, undoRedo.ExecuteCount);
            });
        }

        [Fact]
        public void Undo_RemovesPointAndReturnsToFirstStepWhenEmpty()
        {
            RunOnStaThread(() =>
            {
                var controller = new TestPointSequenceController();
                var host = new TestHost(new TestCadDocumentService(new Layer()), new TestUndoRedoService())
                {
                    CurrentStep = controller.InitialStep
                };

                controller.OnActivated(host);
                Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("1,2", new Point(1, 2))));
                var result = controller.SubmitUndo(host);

                Assert.True(result.Handled);
                Assert.Equal("FirstPoint", result.NextStep.Id);
                Assert.Empty(controller.CapturedPoints);
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

        private sealed class TestPointSequenceController : PointSequenceCommandControllerBase
        {
            private static readonly CommandStep TestFirstPointStep = new CommandStep("FirstPoint", "Specify first point:");
            private static readonly CommandStep TestNextPointStep = new CommandStep("NextPoint", "Specify next point:");

            public override string CommandName => "TESTSEQ";

            public IReadOnlyList<Point> CapturedPoints => Points;

            protected override CommandStep FirstPointStep => TestFirstPointStep;

            protected override CommandStep NextPointStep => TestNextPointStep;

            public InteractiveCommandResult SubmitUndo(IInteractiveCommandHost host)
            {
                return UndoLastPoint(host);
            }

            protected override Entity CreateEntity()
            {
                return new Polyline(Points);
            }
        }

        private sealed class TestHost : IInteractiveCommandHost
        {
            public TestHost(ICadDocumentService document, IUndoRedoService undoRedo)
            {
                ToolService = new TestToolService(document, undoRedo);
            }

            public IToolService ToolService { get; }

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

            public TestToolService(ICadDocumentService document, IUndoRedoService undoRedo)
            {
                services[typeof(ICadDocumentService)] = document;
                services[typeof(IUndoRedoService)] = undoRedo;
            }

            public IViewport Viewport => null;
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

            public List<Entity> AddedEntities { get; } = new List<Entity>();
            public IReadOnlyList<Layer> Layers => new[] { layer };
            public IEnumerable<Entity> Entities => AddedEntities;
            public event EventHandler<LayerChangedEventArgs> LayerAdded { add { } remove { } }
            public event EventHandler<LayerChangedEventArgs> LayerRemoved { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityAdded { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityRemoved { add { } remove { } }
            public Layer CreateLayer(string name, Color color) => layer;
            public void AddLayer(Layer layer) { }
            public bool RemoveLayer(Guid layerId) => false;
            public Layer GetLayer(Guid layerId) => layer;
            public Layer GetLayerForEntity(Entity entity) => layer;
            public void AddEntity(Guid layerId, Entity entity) => AddedEntities.Add(entity);
            public void RemoveEntity(Entity entity) => AddedEntities.Remove(entity);
        }

        private sealed class TestUndoRedoService : IUndoRedoService
        {
            public int ExecuteCount { get; private set; }
            public bool CanUndo => false;
            public bool CanRedo => false;
            public string UndoDescription => string.Empty;
            public string RedoDescription => string.Empty;
            public event EventHandler StateChanged { add { } remove { } }
            public void Execute(IUndoableCommand command)
            {
                ExecuteCount++;
                command.Execute();
            }
            public void PushCompleted(IUndoableCommand command) { }
            public void Undo() { }
            public void Redo() { }
            public void Clear() { }
        }
    }
}
