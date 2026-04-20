using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Primusz.AeroCAD.Core.Snapping;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.View.Editor;
using Primusz.AeroCAD.View.Commands;
using Primusz.AeroCAD.View.Input;
using Primusz.AeroCAD.View.Plugins;

namespace Primusz.AeroCAD.View.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly string[] MenuGroupOrder = { "Edit", "Draw", "Modify", "View" };

        private readonly ModelSpace modelSpace;
        private readonly ICadDocumentService documentService;
        private readonly IUndoRedoService undoRedoService;
        private readonly IEditorStateService editorStateService;
        private readonly ICommandFeedbackService commandFeedbackService;
        private readonly EditorCommandRuntime commandRuntime;
        private readonly IEditorToolRuntime toolRuntime;
        private readonly ISelectionManager selectionManager;
        private readonly IGripService gripService;
        private readonly ISnapEngine snapEngine;
        private readonly ISnapDescriptorService snapDescriptorService;
        private readonly ISpatialQueryService spatialQueryService;
        private readonly Overlay overlay;
        private readonly IOrthoService orthoService;
        private readonly IGridSettingsService gridSettingsService;
        private readonly KeyboardShortcutService keyboardShortcutService;
        private readonly CommandRepeatCoordinator commandRepeatCoordinator;
        private readonly Viewport viewport;
        private string statusText = "Ready";
        private string activeToolName = string.Empty;

        public MainViewModel(Viewport viewport)
        {
            this.viewport = viewport;
            modelSpace = new ModelSpace(viewport)
                .RegisterModule(new RectangleModule())
                .RegisterModule(new ViewInteractiveEntityModule());
            modelSpace.Initialize();

            documentService = modelSpace.GetService<ICadDocumentService>();
            undoRedoService = modelSpace.GetService<IUndoRedoService>();
            editorStateService = modelSpace.GetService<IEditorStateService>();
            commandFeedbackService = modelSpace.GetService<ICommandFeedbackService>();
            toolRuntime = modelSpace.GetService<IEditorToolRuntime>();
            selectionManager = modelSpace.GetService<ISelectionManager>();
            gripService = modelSpace.GetService<IGripService>();
            snapEngine = modelSpace.GetService<ISnapEngine>();
            snapDescriptorService = modelSpace.GetService<ISnapDescriptorService>();
            spatialQueryService = modelSpace.GetService<ISpatialQueryService>();
            overlay = modelSpace.GetService<Overlay>();
            orthoService = modelSpace.GetService<IOrthoService>();
            gridSettingsService = modelSpace.GetService<IGridSettingsService>();
            keyboardShortcutService = new KeyboardShortcutService();
            commandRepeatCoordinator = new CommandRepeatCoordinator();

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

            modelSpace.RegisterService<IEditorCommandRuntime, EditorCommandRuntime>(commandRuntime);

            MenuGroups = BuildMenuGroups(modelSpace.GetService<IEditorCommandCatalog>());

            // Toolbar shows all groups except File (empty) and View (status bar already shows Ortho/Grid)
            ToolbarGroups = MenuGroups
                .Where(g => g.GroupName != "_File" && g.GroupName != "_View" && g.Items.Count > 0)
                .ToList()
                .AsReadOnly();

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
            viewport.MouseMove += OnViewportMouseMove;
            viewport.MouseLeave += OnViewportMouseLeave;
            viewport.GetRubberObject().SnapPointChanged += OnSnapPointChanged;

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

        /// <summary>
        /// Menu groups built from the command catalog. Each group maps to a top-level menu item;
        /// items within a group map to sub-menu entries. Plugin commands appear automatically.
        /// </summary>
        public IReadOnlyList<MenuGroupViewModel> MenuGroups { get; }

        /// <summary>
        /// Toolbar groups — same source as MenuGroups but excludes File and View groups.
        /// Plugin commands with a Draw/Modify group appear here automatically.
        /// </summary>
        public IReadOnlyList<MenuGroupViewModel> ToolbarGroups { get; }

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

        private IReadOnlyList<MenuGroupViewModel> BuildMenuGroups(IEditorCommandCatalog catalog)
        {
            if (catalog == null)
                return new List<MenuGroupViewModel>().AsReadOnly();

            // Special CanExecute predicates for commands that can be disabled
            var canExecuteMap = new Dictionary<string, Func<bool>>(StringComparer.OrdinalIgnoreCase)
            {
                ["UNDO"] = () => undoRedoService?.CanUndo ?? false,
                ["REDO"] = () => undoRedoService?.CanRedo ?? false
            };

            // Group commands by MenuGroup, preserving catalog insertion order within each group
            var grouped = new Dictionary<string, List<EditorCommandDefinition>>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in catalog.Commands.Where(d => d.MenuGroup != null))
            {
                if (!grouped.TryGetValue(definition.MenuGroup, out var list))
                {
                    list = new List<EditorCommandDefinition>();
                    grouped[definition.MenuGroup] = list;
                }
                list.Add(definition);
            }

            // Known groups first in declared order, unknown groups alphabetically after
            var orderedKeys = MenuGroupOrder
                .Where(g => grouped.ContainsKey(g))
                .Concat(grouped.Keys.Except(MenuGroupOrder, StringComparer.OrdinalIgnoreCase).OrderBy(g => g));

            var result = new List<MenuGroupViewModel> { new MenuGroupViewModel("_File", new List<MenuItemViewModel>()) };

            foreach (var key in orderedKeys)
            {
                var items = grouped[key].Select(definition =>
                {
                    canExecuteMap.TryGetValue(definition.Name, out var canExec);
                    ICommand command = canExec != null
                        ? new RelayCommand(() => commandRuntime.Execute(definition.Name), canExec)
                        : new RelayCommand(() => commandRuntime.Execute(definition.Name));
                    return new MenuItemViewModel(definition.MenuLabel ?? definition.Description, command);
                }).ToList();

                var displayName = "_" + char.ToUpper(key[0]) + key.Substring(1).ToLowerInvariant();
                result.Add(new MenuGroupViewModel(displayName, items));
            }

            return result.AsReadOnly();
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
            keyboardShortcutService.Register(Key.F8, () => commandRuntime.Execute("ORTHO"), allowWhenTextInputFocused: true);
            keyboardShortcutService.Register(Key.F7, () => commandRuntime.Execute("GRID"), allowWhenTextInputFocused: true);
        }

        private void OnViewportPositionChanged(object sender, EventArgs e)
        {
            var pos = viewport.Position;
            StatusText = FormatCoordinates(pos);
        }

        private void OnViewportMouseMove(object sender, MouseEventArgs e)
        {
            var screenPoint = e.GetPosition(viewport);
            var worldPoint = viewport.Unproject(screenPoint);
            var statusPoint = ResolveHoverStatusPoint(worldPoint);

            StatusText = FormatCoordinates(statusPoint ?? worldPoint);
        }

        private void OnSnapPointChanged(object sender, EventArgs e)
        {
            var snapResult = viewport.GetRubberObject()?.SnapPoint;
            if (snapResult == null)
                return;

            Point snapped;
            if (snapResult.SourceEntity != null && snapResult.SourceGripIndex.HasValue)
                snapped = snapResult.SourceEntity.GetGripPoint(snapResult.SourceGripIndex.Value);
            else
                snapped = snapResult.SourcePoint ?? snapResult.Point;

            StatusText = FormatCoordinates(snapped);
        }

        private void OnViewportMouseLeave(object sender, MouseEventArgs e)
        {
            var pos = viewport.Position;
            StatusText = FormatCoordinates(pos);
        }

        private Point? ResolveHoverStatusPoint(Point worldPoint)
        {
            var snapResult = viewport.GetRubberObject()?.SnapPoint;
            if (snapResult != null && snapResult.SourceEntity != null && snapResult.SourceGripIndex.HasValue)
                return snapResult.SourceEntity.GetGripPoint(snapResult.SourceGripIndex.Value);

            if (snapResult != null)
                return snapResult.SourcePoint ?? snapResult.Point;

            return null;
        }

        private static string FormatCoordinates(Point point)
        {
            return $"X: {point.X:F2}  Y: {point.Y:F2}";
        }

        private void HandleCommandLineInput(string input)
        {
            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            var trimmedInput = (input ?? string.Empty).Trim();
            var normalized = trimmedInput.ToUpperInvariant();
            var token = commandFeedbackService?.ParseInput(trimmedInput) ?? CommandInputToken.Text(trimmedInput, trimmedInput);

            if (trimmedInput.Length == 0)
            {
                commandRepeatCoordinator.HandleBlankSubmit(
                    activeInteractiveTool,
                    commandRuntime.Execute,
                    () => commandFeedbackService?.ActiveCommandName,
                    RefreshViewportVisuals);
                return;
            }

            if (activeInteractiveTool != null && activeInteractiveTool.TrySubmitToken(token))
            {
                RefreshViewportVisuals();
                if (!string.IsNullOrWhiteSpace(commandFeedbackService?.ActiveCommandName))
                    commandRepeatCoordinator.RememberExecutedCommand(commandFeedbackService.ActiveCommandName);
                return;
            }

            if (commandRuntime.TryResolveAndExecute(normalized))
            {
                commandRepeatCoordinator.RememberExecutedCommand(normalized);
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
