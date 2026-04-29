using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Primusz.AeroCAD.SamplePlugin;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class RoadPlanCommandControllerTests
    {
        [Fact]
        public void Complete_WithTwoPoints_CreatesRoadPlanEntity()
        {
            RunOnStaThread(() =>
            {
            var document = new TestCadDocumentService(new Layer());
            var controller = new RoadPlanCommandController();
            var host = new TestHost(document, new TestUndoRedoService(), new TestCommandFeedbackService())
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("100,0", new Point(100, 0))));
            var result = controller.TryComplete(host);

            Assert.True(result.Handled);
            var entity = Assert.Single(document.AddedEntities);
            var roadPlan = Assert.IsType<RoadPlanEntity>(entity);
            Assert.Equal(2, roadPlan.Vertices.Count);
            Assert.Equal(new Point(0, 0), roadPlan.Vertices[0].Location);
            Assert.Equal(new Point(100, 0), roadPlan.Vertices[1].Location);
            });
        }

        [Fact]
        public void Radius_AfterThreePoints_SetsPreviousVertexRadius()
        {
            RunOnStaThread(() =>
            {
            var document = new TestCadDocumentService(new Layer());
            var controller = new RoadPlanCommandController();
            var host = new TestHost(document, new TestUndoRedoService(), new TestCommandFeedbackService())
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("100,0", new Point(100, 0))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("200,100", new Point(200, 100))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Keyword("R", "R")));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Scalar("80", 80)));
            controller.TryComplete(host);

            var roadPlan = Assert.IsType<RoadPlanEntity>(Assert.Single(document.AddedEntities));
            Assert.Equal(80, roadPlan.Vertices[1].Radius);
            });
        }

        [Fact]
        public void Undo_RemovesFirstPointAndReturnsToFirstPointStep()
        {
            RunOnStaThread(() =>
            {
            var controller = new RoadPlanCommandController();
            var feedback = new TestCommandFeedbackService();
            var host = new TestHost(new TestCadDocumentService(new Layer()), new TestUndoRedoService(), feedback)
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));
            var result = controller.TrySubmitToken(host, CommandInputToken.Keyword("U", "U"));

            Assert.True(result.Handled);
            Assert.Equal("FirstPoint", result.NextStep.Id);
            Assert.Contains("First alignment point removed.", feedback.Messages);
            });
        }

        [Fact]
        public void Close_WithThreePoints_CreatesClosedRoadPlan()
        {
            RunOnStaThread(() =>
            {
            var document = new TestCadDocumentService(new Layer());
            var controller = new RoadPlanCommandController();
            var host = new TestHost(document, new TestUndoRedoService(), new TestCommandFeedbackService())
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("100,0", new Point(100, 0))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("100,100", new Point(100, 100))));
            var result = controller.TrySubmitToken(host, CommandInputToken.Keyword("C", "C"));

            Assert.True(result.Handled);
            var roadPlan = Assert.IsType<RoadPlanEntity>(Assert.Single(document.AddedEntities));
            Assert.True(roadPlan.Vertices.Count >= 4);
            Assert.Equal(roadPlan.Vertices[0].Location, roadPlan.Vertices[roadPlan.Vertices.Count - 1].Location);
            });
        }

        [Fact]
        public void Radius_BeforeThreePoints_DoesNotLeaveNextPointStep()
        {
            RunOnStaThread(() =>
            {
            var controller = new RoadPlanCommandController();
            var feedback = new TestCommandFeedbackService();
            var host = new TestHost(new TestCadDocumentService(new Layer()), new TestUndoRedoService(), feedback)
            {
                CurrentStep = controller.InitialStep
            };

            controller.OnActivated(host);
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("0,0", new Point(0, 0))));
            Apply(host, controller.TrySubmitToken(host, CommandInputToken.Point("100,0", new Point(100, 0))));
            var result = controller.TrySubmitToken(host, CommandInputToken.Keyword("R", "R"));

            Assert.True(result.Handled);
            Assert.Equal("NextPoint", result.NextStep.Id);
            Assert.Contains("At least three points are required before setting a curve radius.", feedback.Messages);
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
            public TestHost(ICadDocumentService document, IUndoRedoService undoRedo, ICommandFeedbackService feedback)
            {
                ToolService = new TestToolService(document, undoRedo, feedback);
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

            public TestToolService(ICadDocumentService document, IUndoRedoService undoRedo, ICommandFeedbackService feedback)
            {
                services[typeof(ICadDocumentService)] = document;
                services[typeof(IUndoRedoService)] = undoRedo;
                services[typeof(ICommandFeedbackService)] = feedback;
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

        private sealed class TestCommandFeedbackService : ICommandFeedbackService
        {
            public string Prompt => string.Empty;
            public string ActiveCommandName => string.Empty;
            public bool HasActiveCommand => true;
            public CommandPrompt ActivePrompt => CommandPrompt.Default;
            public CommandSession ActiveSession => null;
            public List<string> Inputs { get; } = new List<string>();
            public List<string> Messages { get; } = new List<string>();
            public event EventHandler StateChanged { add { } remove { } }
            public event EventHandler<CommandFeedbackMessageEventArgs> MessageLogged { add { } remove { } }
            public CommandInputToken ParseInput(string rawInput) => CommandInputToken.Text(rawInput, rawInput);
            public void BeginCommand(CommandSession session) { }
            public void BeginCommand(string commandName, string prompt) { }
            public void SetPrompt(CommandPrompt prompt) { }
            public void SetPrompt(string prompt) { }
            public void LogInput(CommandInputToken token) => Inputs.Add(token?.FormatForDisplay());
            public void LogInput(string input) => Inputs.Add(input);
            public void LogMessage(string message) => Messages.Add(message);
            public void EndCommand(string closingMessage = null) { }
        }
    }
}
