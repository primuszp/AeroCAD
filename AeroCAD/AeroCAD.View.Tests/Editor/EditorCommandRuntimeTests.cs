using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Tools;
using Primusz.AeroCAD.View.Editor;
using Xunit;

namespace Primusz.AeroCAD.View.Tests.Editor
{
    public class EditorCommandRuntimeTests
    {
        [Fact]
        public void Execute_WhenInteractiveCommandIsActive_DoesNotStartAnotherCommand()
        {
            var activeTool = new FakeInteractiveTool { IsActive = true };
            var toolRuntime = new FakeToolRuntime(activeTool);
            var runtime = new EditorCommandRuntime(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                toolRuntime,
                null,
                null,
                null,
                _ => { });

            var executed = runtime.Execute("MOVE");

            Assert.False(executed);
        }

        [Fact]
        public void DeleteSelectedEntities_SkipsLockedLayerEntities()
        {
            RunOnStaThread(() =>
            {
                var document = new FakeDocumentService();
                var editableLayer = document.CreateLayer("Editable", System.Windows.Media.Colors.White);
                var lockedLayer = document.CreateLayer("Locked", System.Windows.Media.Colors.White);
                lockedLayer.Style.IsLocked = true;

                var keep = new Line(new Point(0, 0), new Point(10, 0));
                var skip = new Line(new Point(0, 10), new Point(10, 10));
                document.AddEntity(editableLayer.Id, keep);
                document.AddEntity(lockedLayer.Id, skip);

                var selection = new FakeSelectionManager(new Entity[] { keep, skip });
                var feedback = new FakeFeedbackService();
                var undoRedo = new FakeUndoRedoService();
                var toolRuntime = new FakeToolRuntime(null);
                var runtime = new EditorCommandRuntime(
                    null,
                    feedback,
                    undoRedo,
                    selection,
                    null,
                    null,
                    null,
                    toolRuntime,
                    document,
                    null,
                    null,
                    _ => { });

                var deleted = runtime.DeleteSelectedEntities();

                Assert.True(deleted);
                Assert.Empty(selection.SelectedEntities);
                Assert.Equal(1, document.RemovedCount);
                Assert.Contains(feedback.Messages, message => message.Contains("1 entity deleted."));
                Assert.Contains(feedback.Messages, message => message.Contains("locked or hidden entity(s) skipped."));
            });
        }

        private sealed class FakeInteractiveTool : ICommandInteractiveTool, ITool
        {
            public bool IsActive { get; set; }
            public Guid Id { get; } = Guid.NewGuid();
            public string Name { get; } = "Fake";
            public IToolService ToolService { get; set; }
            public CadCursorType CursorType => CadCursorType.CrosshairOnly;
            public int InputPriority => 0;
            public bool CanActivate => true;
            public bool Enabled { get; set; } = true;
            public bool IsSuspended { get; set; }
            public bool Activate() => true;
            public bool Deactivate() => true;
            public bool TrySubmitToken(CommandInputToken token) => false;
            public bool TrySubmitText(string input) => false;
            public bool TrySubmitPoint(Point point) => false;
            public bool TryComplete() => false;
            public bool TryCancel() => false;
        }

        private sealed class FakeToolRuntime : IEditorToolRuntime
        {
            private readonly ICommandInteractiveTool activeTool;

            public FakeToolRuntime(ICommandInteractiveTool activeTool)
            {
                this.activeTool = activeTool;
            }

            public ICommandInteractiveTool GetActiveInteractiveTool() => activeTool;
            public bool CancelActiveInteractiveTool() => false;
            public bool ActivateSelectionMode() => true;
            public bool ActivateModalTool<TTool>(Layer activeLayer = null) where TTool : class, ITool => true;
            public bool ActivateModalTool(Type toolType, Layer activeLayer = null) => true;
            public bool ActivateModalTool(string toolName, Layer activeLayer = null) => true;
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

        private sealed class FakeSelectionManager : ISelectionManager
        {
            public FakeSelectionManager(IEnumerable<Entity> selectedEntities)
            {
                items = new List<Entity>(selectedEntities);
            }

            private readonly List<Entity> items;
            public IReadOnlyList<Entity> SelectedEntities => items.AsReadOnly();
            public event EventHandler<SelectionChangedEventArgs> SelectionChanged { add { } remove { } }
            public void Select(Entity entity) => items.Add(entity);
            public void SelectRange(IEnumerable<Entity> entities) => items.AddRange(entities);
            public void Deselect(Entity entity) => items.Remove(entity);
            public void ClearSelection() => items.Clear();
            public bool IsSelected(Entity entity) => items.Contains(entity);
        }

        private sealed class FakeDocumentService : ICadDocumentService
        {
            private readonly List<Layer> layers = new List<Layer>();
            private readonly Dictionary<Guid, Layer> owners = new Dictionary<Guid, Layer>();

            public int RemovedCount { get; private set; }

            public IReadOnlyList<Layer> Layers => layers.AsReadOnly();
            public IEnumerable<Entity> Entities
            {
                get
                {
                    foreach (var layer in layers)
                    {
                        foreach (var entity in layer.Entities)
                            yield return entity;
                    }
                }
            }

            public event EventHandler<LayerChangedEventArgs> LayerAdded { add { } remove { } }
            public event EventHandler<LayerChangedEventArgs> LayerRemoved { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityAdded { add { } remove { } }
            public event EventHandler<EntityChangedEventArgs> EntityRemoved { add { } remove { } }

            public Layer CreateLayer(string name, System.Windows.Media.Color color)
            {
                var layer = new Layer { LayerName = name, Color = color };
                AddLayer(layer);
                return layer;
            }

            public void AddLayer(Layer layer) => layers.Add(layer);
            public bool RemoveLayer(Guid layerId) => false;
            public Layer GetLayer(Guid layerId) => layers.Find(layer => layer.Id == layerId);
            public Layer GetLayerForEntity(Entity entity) => entity != null && owners.TryGetValue(entity.Id, out var layer) ? layer : null;

            public void AddEntity(Guid layerId, Entity entity)
            {
                var layer = GetLayer(layerId);
                layer.Add(entity);
                owners[entity.Id] = layer;
            }

            public void RemoveEntity(Entity entity)
            {
                var layer = GetLayerForEntity(entity);
                if (layer == null)
                    return;

                layer.Remove(entity);
                owners.Remove(entity.Id);
                RemovedCount++;
            }
        }

        private sealed class FakeFeedbackService : ICommandFeedbackService
        {
            public string Prompt => string.Empty;
            public string ActiveCommandName => string.Empty;
            public bool HasActiveCommand => false;
            public CommandPrompt ActivePrompt => CommandPrompt.Default;
            public CommandSession ActiveSession => null;
            public event EventHandler StateChanged { add { } remove { } }
            public event EventHandler<CommandFeedbackMessageEventArgs> MessageLogged { add { } remove { } }
            public List<string> Messages { get; } = new List<string>();
            public CommandInputToken ParseInput(string rawInput) => CommandInputToken.Text(rawInput, rawInput);
            public void BeginCommand(CommandSession session) { }
            public void BeginCommand(string commandName, string prompt) { }
            public void SetPrompt(CommandPrompt prompt) { }
            public void SetPrompt(string prompt) { }
            public void LogInput(CommandInputToken token) { }
            public void LogInput(string input) { }
            public void LogMessage(string message) => Messages.Add(message);
            public void EndCommand(string closingMessage = null) { }
        }

        private sealed class FakeUndoRedoService : Primusz.AeroCAD.Core.Commands.IUndoRedoService
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
    }
}
