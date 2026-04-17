using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Tools;
using Primusz.AeroCAD.View.Editor;
using Primusz.AeroCAD.View.Commands;
using Primusz.AeroCAD.View.Input;

namespace Primusz.AeroCAD.View.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ModelSpace modelSpace;
        private readonly ICadDocumentService documentService;
        private readonly IUndoRedoService undoRedoService;
        private readonly IEditorStateService editorStateService;
        private readonly ICommandFeedbackService commandFeedbackService;
        private readonly EditorCommandRuntime commandRuntime;
        private readonly IEditorToolRuntime toolRuntime;
        private readonly ISelectionManager selectionManager;
        private readonly IOrthoService orthoService;
        private readonly IGridSettingsService gridSettingsService;
        private readonly KeyboardShortcutService keyboardShortcutService;
        private readonly Viewport viewport;
        private string statusText = "Ready";
        private string activeToolName = string.Empty;

        public MainViewModel(Viewport viewport)
        {
            this.viewport = viewport;
            modelSpace = new ModelSpace(viewport);

            documentService = modelSpace.GetService<ICadDocumentService>();
            undoRedoService = modelSpace.GetService<IUndoRedoService>();
            editorStateService = modelSpace.GetService<IEditorStateService>();
            commandFeedbackService = modelSpace.GetService<ICommandFeedbackService>();
            toolRuntime = modelSpace.GetService<IEditorToolRuntime>();
            selectionManager = modelSpace.GetService<ISelectionManager>();
            orthoService = modelSpace.GetService<IOrthoService>();
            gridSettingsService = modelSpace.GetService<IGridSettingsService>();
            keyboardShortcutService = new KeyboardShortcutService();
            commandRuntime = new EditorCommandRuntime(
                modelSpace.GetService<IEditorCommandCatalog>(),
                commandFeedbackService,
                undoRedoService,
                selectionManager,
                orthoService,
                gridSettingsService,
                editorStateService,
                toolRuntime,
                documentService,
                viewport,
                GetActiveLayer,
                toolName => ActiveToolName = toolName);

            CommandLine = new CommandLineViewModel(HandleCommandLineInput, CancelCurrentCommand);

            undoRedoService.StateChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(CanRedo));
                CommandManager.InvalidateRequerySuggested();
            };

            if (commandFeedbackService != null)
            {
                commandFeedbackService.StateChanged += (s, e) =>
                {
                    CommandLine.Prompt = commandFeedbackService.Prompt;
                };
                commandFeedbackService.MessageLogged += (s, e) => CommandLine.WriteMessage(e.Message);
                CommandLine.Prompt = commandFeedbackService.Prompt;
            }

            if (orthoService != null)
                orthoService.StateChanged += (s, e) => OnPropertyChanged(nameof(IsOrthoActive));
            if (gridSettingsService != null)
                gridSettingsService.StateChanged += (s, e) => OnPropertyChanged(nameof(IsGridVisible));

            DependencyPropertyDescriptor
                .FromProperty(Viewport.PositionProperty, typeof(Viewport))
                .AddValueChanged(viewport, OnViewportPositionChanged);

            InitializeCommands();
            InitializeKeyboardShortcuts();
            ActiveToolName = "SelectionTool";
        }

        public bool CanUndo => undoRedoService?.CanUndo ?? false;

        public bool CanRedo => undoRedoService?.CanRedo ?? false;

        public bool IsOrthoActive => orthoService?.IsEnabled ?? false;

        public bool IsGridVisible => gridSettingsService?.IsEnabled ?? false;

        public string StatusText
        {
            get => statusText;
            private set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        public string ActiveToolName
        {
            get => activeToolName;
            private set
            {
                activeToolName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LayerViewModel> Layers { get; } = new ObservableCollection<LayerViewModel>();

        public CommandLineViewModel CommandLine { get; }

        public ICommand UndoCommand { get; private set; }

        public ICommand RedoCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public ICommand ActivateSelectionToolCommand { get; private set; }

        public ICommand ActivateLineToolCommand { get; private set; }

        public ICommand ActivatePolylineToolCommand { get; private set; }

        public ICommand ActivateCircleToolCommand { get; private set; }

        public ICommand ActivateArcToolCommand { get; private set; }

        public ICommand ActivateMoveToolCommand { get; private set; }

        public ICommand ActivateCopyToolCommand { get; private set; }

        public ICommand ActivateOffsetToolCommand { get; private set; }

        public ICommand ActivateTrimToolCommand { get; private set; }

        public ICommand ActivateExtendToolCommand { get; private set; }

        public ICommand ToggleOrthoCommand { get; private set; }

        public ICommand ToggleGridCommand { get; private set; }

        public void ToggleOrtho()
        {
            orthoService?.Toggle();
            commandFeedbackService?.LogMessage(
                orthoService?.IsEnabled == true ? "Ortho mode ON." : "Ortho mode OFF.");
        }

        public void ToggleGrid()
        {
            gridSettingsService?.Toggle();
            commandFeedbackService?.LogMessage(
                gridSettingsService?.IsEnabled == true ? "Grid mode ON." : "Grid mode OFF.");
        }

        public bool TryHandleShortcut(KeyEventArgs e, bool isTextInputFocused)
        {
            return keyboardShortcutService.TryHandle(e, isTextInputFocused);
        }

        public bool TryHandleGlobalEnter()
        {
            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            if (activeInteractiveTool == null)
                return false;

            var handled = activeInteractiveTool.TryComplete();
            if (handled)
                RefreshViewportVisuals();
            return handled;
        }

        public bool TryHandleGlobalEscape()
        {
            CancelCurrentCommand();
            return true;
        }

        public bool TryHandleGlobalDelete()
        {
            return DeleteSelectedEntities();
        }

        public LayerViewModel AddLayer(string name, Color color)
        {
            var layer = documentService.CreateLayer(name, color);
            var vm = new LayerViewModel(layer);
            Layers.Add(vm);

            if (Layers.Count == 1)
            {
                vm.IsActive = true;
                editorStateService?.SetActiveLayer(layer);
            }

            return vm;
        }

        public void AddEntity(Layer layer, Entity entity)
        {
            if (layer == null || entity == null)
                return;

            documentService.AddEntity(layer.Id, entity);
        }

        public Layer GetActiveLayer()
        {
            if (editorStateService?.ActiveLayer != null)
                return editorStateService.ActiveLayer;

            foreach (var vm in Layers)
            {
                if (vm.IsActive)
                    return vm.Layer;
            }

            return Layers.Count > 0 ? Layers[0].Layer : null;
        }

        private void InitializeCommands()
        {
            UndoCommand = new RelayCommand(
                () => commandRuntime.Execute("UNDO"),
                () => undoRedoService?.CanUndo ?? false);

            RedoCommand = new RelayCommand(
                () => commandRuntime.Execute("REDO"),
                () => undoRedoService?.CanRedo ?? false);

            CancelCommand = new RelayCommand(() => commandRuntime.Execute("CANCEL"));

            ToggleOrthoCommand = new RelayCommand(() => commandRuntime.Execute("ORTHO"));
            ToggleGridCommand = new RelayCommand(() => commandRuntime.Execute("GRID"));

            ActivateSelectionToolCommand = new RelayCommand(() => commandRuntime.Execute("SELECT"));
            ActivateLineToolCommand = new RelayCommand(() => commandRuntime.Execute("LINE"));
            ActivatePolylineToolCommand = new RelayCommand(() => commandRuntime.Execute("PLINE"));
            ActivateCircleToolCommand = new RelayCommand(() => commandRuntime.Execute("CIRCLE"));
            ActivateArcToolCommand = new RelayCommand(() => commandRuntime.Execute("ARC"));
            ActivateMoveToolCommand = new RelayCommand(() => commandRuntime.Execute("MOVE"));
            ActivateCopyToolCommand = new RelayCommand(() => commandRuntime.Execute("COPY"));
            ActivateOffsetToolCommand = new RelayCommand(() => commandRuntime.Execute("OFFSET"));
            ActivateTrimToolCommand = new RelayCommand(() => commandRuntime.Execute("TRIM"));
            ActivateExtendToolCommand = new RelayCommand(() => commandRuntime.Execute("EXTEND"));
        }

        private void InitializeKeyboardShortcuts()
        {
            keyboardShortcutService.Register(Key.Escape, TryHandleGlobalEscape);
            keyboardShortcutService.Register(Key.Enter, TryHandleGlobalEnter);
            keyboardShortcutService.Register(
                Key.Delete,
                TryHandleGlobalDelete,
                allowWhenTextInputFocused: true,
                canHandle: context => !context.IsTextInputFocused || string.IsNullOrWhiteSpace(CommandLine?.CurrentInput));
            keyboardShortcutService.Register(Key.F8, () =>
            {
                return commandRuntime.Execute("ORTHO");
            }, allowWhenTextInputFocused: true);
            keyboardShortcutService.Register(Key.F7, () =>
            {
                return commandRuntime.Execute("GRID");
            }, allowWhenTextInputFocused: true);
        }

        private void OnViewportPositionChanged(object sender, EventArgs e)
        {
            var pos = viewport.Position;
            StatusText = $"X: {pos.X:F2}  Y: {pos.Y:F2}";
        }

        private void HandleCommandLineInput(string input)
        {
            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            var trimmedInput = (input ?? string.Empty).Trim();
            var normalized = trimmedInput.ToUpperInvariant();
            var token = commandFeedbackService?.ParseInput(trimmedInput) ?? CommandInputToken.Text(trimmedInput, trimmedInput);

            if (trimmedInput.Length == 0)
            {
                if (activeInteractiveTool != null)
                {
                    activeInteractiveTool.TryComplete();
                    RefreshViewportVisuals();
                }
                return;
            }

            if (activeInteractiveTool != null && activeInteractiveTool.TrySubmitToken(token))
            {
                RefreshViewportVisuals();
                return;
            }

            if (commandRuntime.TryResolveAndExecute(normalized))
            {
                RefreshViewportVisuals();
                return;
            }

            if (activeInteractiveTool != null)
                commandFeedbackService?.LogMessage($"Invalid input for active command: {input}");
            else
                commandFeedbackService?.LogMessage($"Unknown command: {input}");
        }

        private void CancelCurrentCommand()
        {
            commandRuntime.CancelCurrentCommand();
            RefreshViewportVisuals();
        }

        private bool DeleteSelectedEntities()
        {
            var deleted = commandRuntime.DeleteSelectedEntities();
            if (deleted)
                RefreshViewportVisuals();
            return deleted;
        }

        private void RefreshViewportVisuals()
        {
            viewport.GetRubberObject()?.InvalidateVisual();
            viewport.RefreshView();
            viewport.InvalidateVisual();
        }
    }
}


