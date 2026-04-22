using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Tools
{
    public class OffsetCommandControllerTests
    {
        [Fact]
        public void DistanceStep_WithPreselectedEntity_SkipsEntitySelection()
        {
            RunOnStaThread(() =>
            {
                var entity = new Line(new Point(0, 0), new Point(10, 0));
                var layer = new Layer();
                var controller = new OffsetCommandController();
                var host = new TestHost(
                    new TestSelectionManager(new[] { entity }),
                    new TestCadDocumentService(layer, entity))
                {
                    CurrentStep = controller.InitialStep
                };

                controller.OnActivated(host);

                var result = controller.TrySubmitToken(host, CommandInputToken.Scalar("5", 5.0d));

                Assert.True(result.Handled);
                Assert.NotNull(result.NextStep);
                Assert.Equal("Specify point on side to offset:", result.NextStep.Prompt?.Text);
            });
        }

        [Fact]
        public void DistanceStep_WithoutPreselection_RequestsEntitySelection()
        {
            var controller = new OffsetCommandController();
            var host = new TestHost(
                new TestSelectionManager(Array.Empty<Entity>()),
                new TestCadDocumentService(null, null))
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);

            var result = controller.TrySubmitToken(host, CommandInputToken.Scalar("5", 5.0d));

            Assert.True(result.Handled);
            Assert.NotNull(result.NextStep);
            Assert.Equal("Select object to offset [Exit/Undo]:", result.NextStep.Prompt?.Text);
        }

        [Fact]
        public void DistanceStep_LogsDistanceOnlyOnce()
        {
            var controller = new OffsetCommandController();
            var feedback = new TestCommandFeedbackService();
            var host = new TestHost(
                new TestSelectionManager(Array.Empty<Entity>()),
                new TestCadDocumentService(null, null),
                feedback)
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);

            var result = controller.TrySubmitToken(host, CommandInputToken.Scalar("5", 5.0d));

            Assert.True(result.Handled);
            Assert.Empty(feedback.LoggedInputs);
        }

        [Fact]
        public void DistanceStep_DoesNotAcceptViewportPoint()
        {
            var controller = new OffsetCommandController();
            var host = new TestHost(
                new TestSelectionManager(Array.Empty<Entity>()),
                new TestCadDocumentService(null, null))
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);

            var result = controller.TrySubmitViewportPoint(host, new Point(10, 10));

            Assert.True(result.Handled);
            Assert.Null(result.NextStep);
        }

        [Fact]
        public void SidePoint_AfterOffset_RequestsNewEntitySelection()
        {
            RunOnStaThread(() =>
            {
                var entity = new Line(new Point(0, 0), new Point(10, 0));
                var layer = new Layer();
                var document = new TestCadDocumentService(layer, entity);
                var controller = new OffsetCommandController();
                var host = new TestHost(
                    new TestSelectionManager(new[] { entity }),
                    document,
                    null,
                    new TestOffsetService(),
                    new TestUndoRedoService())
                {
                    CurrentStep = controller.InitialStep
                };

                controller.OnActivated(host);

                var distanceResult = controller.TrySubmitToken(host, CommandInputToken.Scalar("5", 5.0d));
                host.CurrentStep = distanceResult.NextStep;

                var sideResult = controller.TrySubmitViewportPoint(host, new Point(0, 10));

                Assert.True(sideResult.Handled);
                Assert.NotNull(sideResult.NextStep);
                Assert.Equal("Select object to offset [Exit/Undo]:", sideResult.NextStep.Prompt?.Text);
                Assert.Equal(1, document.AddedEntityCount);
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
            public TestHost(
                ISelectionManager selectionManager,
                ICadDocumentService documentService,
                ICommandFeedbackService feedbackService = null,
                Primusz.AeroCAD.Core.Editing.Offsets.IEntityOffsetService offsetService = null,
                Primusz.AeroCAD.Core.Commands.IUndoRedoService undoRedoService = null)
            {
                ToolService = new TestToolService(selectionManager, documentService, feedbackService, offsetService, undoRedoService);
            }

            public IToolService ToolService { get; }
            public CommandStep CurrentStep { get; set; }

            public bool TryResolveScalarInput(CommandInputToken token, out double scalar)
            {
                if (token?.ScalarValue.HasValue == true)
                {
                    scalar = token.ScalarValue.Value;
                    return true;
                }

                scalar = default;
                return false;
            }

            public bool TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point)
            {
                point = default;
                return false;
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

            public TestToolService(
                ISelectionManager selectionManager,
                ICadDocumentService documentService,
                ICommandFeedbackService feedbackService,
                Primusz.AeroCAD.Core.Editing.Offsets.IEntityOffsetService offsetService,
                Primusz.AeroCAD.Core.Commands.IUndoRedoService undoRedoService)
            {
                services[typeof(ISelectionManager)] = selectionManager;
                services[typeof(ICadDocumentService)] = documentService;
                if (feedbackService != null)
                    services[typeof(ICommandFeedbackService)] = feedbackService;
                if (offsetService != null)
                    services[typeof(Primusz.AeroCAD.Core.Editing.Offsets.IEntityOffsetService)] = offsetService;
                if (undoRedoService != null)
                    services[typeof(Primusz.AeroCAD.Core.Commands.IUndoRedoService)] = undoRedoService;
            }

            public Primusz.AeroCAD.Core.Drawing.IViewport Viewport => null;
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

        private sealed class TestSelectionManager : ISelectionManager
        {
            public TestSelectionManager(IEnumerable<Entity> selectedEntities)
            {
                SelectedEntities = selectedEntities.ToList().AsReadOnly();
            }

            public IReadOnlyList<Entity> SelectedEntities { get; }
            public event EventHandler<SelectionChangedEventArgs> SelectionChanged { add { } remove { } }
            public void Select(Entity entity) { }
            public void SelectRange(IEnumerable<Entity> entities) { }
            public void Deselect(Entity entity) { }
            public void ClearSelection() { }
            public bool IsSelected(Entity entity) => SelectedEntities.Contains(entity);
        }

        private sealed class TestCadDocumentService : ICadDocumentService
        {
            private readonly Layer layer;
            private readonly Entity entity;
            public int AddedEntityCount { get; private set; }

            public TestCadDocumentService(Layer layer, Entity entity)
            {
                this.layer = layer;
                this.entity = entity;
            }

            public IReadOnlyList<Layer> Layers => layer == null ? Array.Empty<Layer>() : new[] { layer };
            public IEnumerable<Entity> Entities => entity == null ? Array.Empty<Entity>() : new[] { entity };
            public event EventHandler<LayerChangedEventArgs> LayerAdded { add { } remove { } }
            public event EventHandler<LayerChangedEventArgs> LayerRemoved { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityAdded { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityRemoved { add { } remove { } }
            public Layer CreateLayer(string name, System.Windows.Media.Color color) => layer;
            public void AddLayer(Layer layer) { }
            public bool RemoveLayer(Guid layerId) => false;
            public Layer GetLayer(Guid layerId) => layer;
            public Layer GetLayerForEntity(Entity entity) => this.entity != null && ReferenceEquals(entity, this.entity) ? layer : null;
            public void AddEntity(Guid layerId, Entity entity) { AddedEntityCount++; }
            public void RemoveEntity(Entity entity) { }
        }

        private sealed class TestOffsetService : Primusz.AeroCAD.Core.Editing.Offsets.IEntityOffsetService
        {
            public bool CanOffset(Entity entity) => true;
            public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint) => null;
            public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint) => new Line(new Point(0, distance), new Point(10, distance));
        }

        private sealed class TestUndoRedoService : Primusz.AeroCAD.Core.Commands.IUndoRedoService
        {
            public bool CanUndo => false;
            public bool CanRedo => false;
            public string UndoDescription => string.Empty;
            public string RedoDescription => string.Empty;
            public event EventHandler StateChanged { add { } remove { } }
            public void Execute(Primusz.AeroCAD.Core.Commands.IUndoableCommand command) => command.Execute();
            public void PushCompleted(Primusz.AeroCAD.Core.Commands.IUndoableCommand command) { }
            public void Undo() { }
            public void Redo() { }
            public void Clear() { }
        }

        private sealed class TestCommandFeedbackService : ICommandFeedbackService
        {
            public string Prompt => string.Empty;
            public string ActiveCommandName => string.Empty;
            public bool HasActiveCommand => true;
            public CommandPrompt ActivePrompt => CommandPrompt.Default;
            public CommandSession ActiveSession => null;
            public event EventHandler StateChanged { add { } remove { } }
            public event EventHandler<CommandFeedbackMessageEventArgs> MessageLogged { add { } remove { } }
            public List<string> LoggedInputs { get; } = new List<string>();
            public CommandInputToken ParseInput(string rawInput) => CommandInputToken.Text(rawInput, rawInput);
            public void BeginCommand(CommandSession session) { }
            public void BeginCommand(string commandName, string prompt) { }
            public void SetPrompt(CommandPrompt prompt) { }
            public void SetPrompt(string prompt) { }
            public void LogInput(CommandInputToken token) => LoggedInputs.Add(token?.FormatForDisplay());
            public void LogInput(string input) => LoggedInputs.Add(input);
            public void LogMessage(string message) { }
            public void EndCommand(string closingMessage = null) { }
        }
    }
}
