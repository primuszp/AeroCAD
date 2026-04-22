using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.View.Editor
{
    public class EditorCommandRuntime : IEditorCommandRuntime
    {
        private readonly IEditorCommandCatalog commandCatalog;
        private readonly ICommandFeedbackService commandFeedbackService;
        private readonly IUndoRedoService undoRedoService;
        private readonly ISelectionManager selectionManager;
        private readonly IOrthoService orthoService;
        private readonly IGridSettingsService gridSettingsService;
        private readonly IEditorStateService editorStateService;
        private readonly IEditorToolRuntime toolRuntime;
        private readonly ICadDocumentService documentService;
        private readonly Viewport viewport;
        private readonly Func<Layer> activeLayerProvider;
        private readonly Action<string> activeToolChanged;
        private readonly Dictionary<string, Func<bool>> commandExecutors = new Dictionary<string, Func<bool>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EditorCommandDefinition> commandDefinitions = new Dictionary<string, EditorCommandDefinition>(StringComparer.OrdinalIgnoreCase);

        public EditorCommandRuntime(
            IEditorCommandCatalog commandCatalog,
            ICommandFeedbackService commandFeedbackService,
            IUndoRedoService undoRedoService,
            ISelectionManager selectionManager,
            IOrthoService orthoService,
            IGridSettingsService gridSettingsService,
            IEditorStateService editorStateService,
            IEditorToolRuntime toolRuntime,
            ICadDocumentService documentService,
            Viewport viewport,
            Func<Layer> activeLayerProvider,
            Action<string> activeToolChanged)
        {
            this.commandCatalog = commandCatalog;
            this.commandFeedbackService = commandFeedbackService;
            this.undoRedoService = undoRedoService;
            this.selectionManager = selectionManager;
            this.orthoService = orthoService;
            this.gridSettingsService = gridSettingsService;
            this.editorStateService = editorStateService;
            this.toolRuntime = toolRuntime;
            this.documentService = documentService;
            this.viewport = viewport;
            this.activeLayerProvider = activeLayerProvider;
            this.activeToolChanged = activeToolChanged;

            RegisterDefaultCommands();
        }

        public bool Execute(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return false;

            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            if (activeInteractiveTool != null && !IsBuiltInCommand(commandName))
            {
                commandFeedbackService?.LogMessage("Finish or cancel the active command first.");
                return false;
            }

            var normalizedName = commandName.Trim().ToUpperInvariant();
            if (!commandExecutors.TryGetValue(normalizedName, out var executor))
            {
                if (!commandDefinitions.TryGetValue(normalizedName, out var fallbackDefinition))
                {
                    // Fall through to catalog for plugin-registered commands
                    if (commandCatalog == null || !commandCatalog.TryResolve(normalizedName, out fallbackDefinition))
                        return false;
                }

                if (!ValidatePolicy(fallbackDefinition))
                    return true;

                return ExecuteDefinition(fallbackDefinition);
            }

            if (commandDefinitions.TryGetValue(normalizedName, out var definition) && !ValidatePolicy(definition))
                return true;

            return executor();
        }

        private static bool IsBuiltInCommand(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return false;

            switch (commandName.Trim().ToUpperInvariant())
            {
                case "CANCEL":
                case "ESC":
                case "STOP":
                case "SELECT":
                case "S":
                case "SEL":
                    return true;
                default:
                    return false;
            }
        }

        public bool TryResolveAndExecute(string input)
        {
            if (commandCatalog == null || string.IsNullOrWhiteSpace(input))
                return false;

            return commandCatalog.TryResolve(input, out var definition) && Execute(definition.Name);
        }

        public bool CompleteActiveCommand()
        {
            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            return activeInteractiveTool != null && activeInteractiveTool.TryComplete();
        }

        public void CancelCurrentCommand()
        {
            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            if (activeInteractiveTool != null)
            {
                activeInteractiveTool.TryCancel();
                ActivateSelectionMode();
                return;
            }

            if (editorStateService != null && editorStateService.Mode == EditorMode.SelectionWindow)
            {
                var rubberObject = viewport?.GetRubberObject();
                if (rubberObject != null)
                {
                    rubberObject.SnapPoint = null;
                    rubberObject.Cancel();
                }

                ActivateSelectionMode();
                commandFeedbackService?.EndCommand("Selection window canceled.");
                return;
            }

            if (selectionManager?.SelectedEntities.Count > 0)
            {
                selectionManager.ClearSelection();
                ActivateSelectionMode();
                commandFeedbackService?.EndCommand("Selection cleared.");
                return;
            }

            ActivateSelectionMode();
            commandFeedbackService?.EndCommand("Selection mode active.");
        }

        public bool DeleteSelectedEntities()
        {
            if (selectionManager == null || selectionManager.SelectedEntities.Count == 0)
                return false;

            var entitiesToDelete = selectionManager.SelectedEntities.ToList();
            selectionManager.ClearSelection();

            var command = new RemoveEntitiesCommand(
                documentService,
                entitiesToDelete,
                entitiesToDelete.Count == 1 ? "Delete Entity" : "Delete Entities");

            undoRedoService?.Execute(command);
            ActivateSelectionMode();
            commandFeedbackService?.LogMessage(
                entitiesToDelete.Count == 1
                    ? "1 entity deleted."
                    : $"{entitiesToDelete.Count} entities deleted.");

            return true;
        }

        private void RegisterDefaultCommands()
        {
            Register(
                new EditorCommandDefinition("SELECT", new[] { "S", "SEL" }, "Return to selection mode.",
                    menuGroup: "Edit", menuLabel: "_Select"),
                () =>
                {
                    ActivateSelectionMode();
                    commandFeedbackService?.EndCommand("Selection mode active.");
                    return true;
                });

            Register(
                new EditorCommandDefinition("UNDO", new[] { "U" }, "Undo last command.",
                    menuGroup: "Edit", menuLabel: "_Undo"),
                () =>
                {
                    if (undoRedoService?.CanUndo ?? false)
                    {
                        undoRedoService.Undo();
                        commandFeedbackService?.LogMessage("Undo executed.");
                    }
                    else
                    {
                        commandFeedbackService?.LogMessage("No undo available.");
                    }

                    return true;
                });

            Register(
                new EditorCommandDefinition("REDO", new[] { "R" }, "Redo last undone command.",
                    menuGroup: "Edit", menuLabel: "_Redo"),
                () =>
                {
                    if (undoRedoService?.CanRedo ?? false)
                    {
                        undoRedoService.Redo();
                        commandFeedbackService?.LogMessage("Redo executed.");
                    }
                    else
                    {
                        commandFeedbackService?.LogMessage("No redo available.");
                    }

                    return true;
                });

            Register(
                new EditorCommandDefinition("ORTHO", new[] { "F8" }, "Toggle orthogonal mode.",
                    menuGroup: "View", menuLabel: "_Ortho"),
                () =>
                {
                    orthoService?.Toggle();
                    commandFeedbackService?.LogMessage(
                        orthoService?.IsEnabled == true ? "Ortho mode ON." : "Ortho mode OFF.");
                    return true;
                });

            Register(
                new EditorCommandDefinition("GRID", ["F7"], "Toggle adaptive grid.",
                    menuGroup: "View", menuLabel: "_Grid"),
                () =>
                {
                    gridSettingsService?.Toggle();
                    commandFeedbackService?.LogMessage(
                        gridSettingsService?.IsEnabled == true ? "Grid mode ON." : "Grid mode OFF.");
                    return true;
                });

            Register(
                new EditorCommandDefinition("CANCEL", ["ESC", "STOP"], "Cancel the active command."),
                () =>
                {
                    CancelCurrentCommand();
                    return true;
                });

            Register(
                new EditorCommandDefinition("HELP", new[] { "?" }, "List available commands."),
                () =>
                {
                    var commands = commandCatalog?.Commands
                        .OrderBy(definition => definition.Name)
                        .Select(definition => $"{definition.Name} - {definition.Description}")
                        .ToList();

                    if (commands == null || commands.Count == 0)
                    {
                        commandFeedbackService?.LogMessage("No commands registered.");
                        return true;
                    }

                    commandFeedbackService?.LogMessage("Commands:");
                    foreach (var command in commands)
                        commandFeedbackService?.LogMessage(command);

                    return true;
                });
        }

        private void Register(EditorCommandDefinition definition, Func<bool> executor = null)
        {
            commandCatalog?.Register(definition);
            if (executor != null)
                commandExecutors[definition.Name] = executor;
            commandDefinitions[definition.Name] = definition;
        }

        private bool ExecuteDefinition(EditorCommandDefinition definition)
        {
            if (definition == null)
                return false;

            var activeLayer = definition.AssignActiveLayer ? activeLayerProvider?.Invoke() : null;
            if (definition.AssignActiveLayer && activeLayer != null)
                editorStateService?.SetActiveLayer(activeLayer);

            bool activated;
            if (!string.IsNullOrWhiteSpace(definition.ModalToolName))
            {
                activated = toolRuntime?.ActivateModalTool(definition.ModalToolName, activeLayer) ?? false;
                if (activated)
                    activeToolChanged?.Invoke(definition.ModalToolName);
            }
            else if (definition.ModalToolType != null)
            {
                activated = toolRuntime?.ActivateModalTool(definition.ModalToolType, activeLayer) ?? false;
                if (activated)
                    activeToolChanged?.Invoke(definition.ModalToolType.Name);
            }
            else
            {
                return false;
            }

            return activated;
        }

        private void ActivateSelectionMode()
        {
            toolRuntime?.ActivateSelectionMode();
            activeToolChanged?.Invoke("SelectionTool");
        }

        private bool ValidatePolicy(EditorCommandDefinition definition)
        {
            var policy = definition?.Policy ?? EditorCommandPolicy.Default;
            if (policy.SelectionRequirement == CommandSelectionRequirement.None)
                return true;

            var selectedEntities = selectionManager?.SelectedEntities;
            int selectedCount = selectedEntities?.Count ?? 0;

            if (policy.SelectionRequirement == CommandSelectionRequirement.Any && selectedCount <= 0)
            {
                commandFeedbackService?.LogMessage(policy.SelectionFailureMessage);
                return false;
            }

            if (policy.SelectionRequirement == CommandSelectionRequirement.Single && selectedCount != 1)
            {
                commandFeedbackService?.LogMessage(policy.SelectionFailureMessage);
                return false;
            }

            if (policy.SupportedSelectionEntityTypes.Count == 0 || selectedCount == 0)
                return true;

            bool supported = selectedEntities.All(entity =>
                policy.SupportedSelectionEntityTypes.Any(type => type.IsInstanceOfType(entity)));

            if (!supported)
            {
                commandFeedbackService?.LogMessage(policy.SupportedTypesFailureMessage);
                return false;
            }

            return true;
        }
    }
}

