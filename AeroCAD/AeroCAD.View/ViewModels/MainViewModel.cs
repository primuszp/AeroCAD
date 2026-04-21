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
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class MainViewModel : ViewModelBase
    {
        private static readonly string[] MenuGroupOrder = { "Edit", "Draw", "Modify", "View" };
        private static readonly Color[] DefaultLayerPalette =
        {
            Colors.White,
            Colors.Red,
            Colors.Turquoise,
            Colors.Gold,
            Colors.LawnGreen,
            Colors.Orange,
            Colors.MediumPurple
        };

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
        private readonly CommandLifecycleService commandLifecycleService;
        private readonly IHoverFeedbackService hoverFeedbackService;
        private readonly Viewport viewport;
        private int nextLayerNumber = 1;
        private string statusText = "Ready";
        private string activeToolName = string.Empty;
        private LayerViewModel selectedLayer;
        private bool suppressSelectionDrivenLayerUpdates;

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
            commandLifecycleService = new CommandLifecycleService(new CommandRepeatCoordinator());
            hoverFeedbackService = new HoverFeedbackService();
            modelSpace.RegisterService<IHoverFeedbackService, HoverFeedbackService>((HoverFeedbackService)hoverFeedbackService);

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
                GetCreationLayer,
                toolName => ActiveToolName = toolName);

            modelSpace.RegisterService<IEditorCommandRuntime, EditorCommandRuntime>(commandRuntime);

            MenuGroups = BuildMenuGroups(modelSpace.GetService<IEditorCommandCatalog>());

            // Toolbar shows all groups except File (empty) and View (status bar already shows Ortho/Grid)
            ToolbarGroups = MenuGroups
                .Where(g => g.GroupName != "_File" && g.GroupName != "_View" && g.Items.Count > 0)
                .ToList()
                .AsReadOnly();

            CommandLine = new CommandLineViewModel(
                input => HandleCommandLineInput(input),
                () => CancelCurrentCommand());
            AddLayerCommand = new RelayCommand(AddDefaultLayer);
            RemoveLayerCommand = new RelayCommand(RemoveSelectedLayer, CanRemoveSelectedLayer);
            MoveSelectionToLayerCommand = new RelayCommand(() => MoveSelectedEntitiesToSelectedLayer(SelectedLayer), CanMoveSelectedEntitiesToSelectedLayer);
            LayerLineWeights = LineWeightPalette.StandardValues.ToList().AsReadOnly();
            LayerLineStyles = Enum.GetValues(typeof(LineStyle)).Cast<LineStyle>().ToList().AsReadOnly();

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
            if (selectionManager != null)
                selectionManager.SelectionChanged += OnSelectionChanged;

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

        public IReadOnlyList<double> LayerLineWeights { get; }

        public IReadOnlyList<LineStyle> LayerLineStyles { get; }

        public CommandLineViewModel CommandLine { get; }

        public ICommand AddLayerCommand { get; }

        public ICommand RemoveLayerCommand { get; }

        public ICommand MoveSelectionToLayerCommand { get; }

        public LayerViewModel SelectedLayer
        {
            get
            {
                var selectionLayer = ResolveSelectedEntitiesLayer();
                if (selectionLayer != null || HasMixedSelectedEntityLayers())
                    return selectionLayer;

                return selectedLayer;
            }
            set
            {
                var displayedLayer = GetDisplayedLayer();
                if (ReferenceEquals(displayedLayer, value))
                    return;

                selectedLayer = value;
                OnPropertyChanged();

                if (suppressSelectionDrivenLayerUpdates)
                    return;

                if (selectedLayer != null && selectedLayer.CanBeActive)
                {
                    if (selectionManager?.SelectedEntities?.Count > 0)
                        MoveSelectedEntitiesToSelectedLayer(selectedLayer);
                    else
                        SetActiveLayer(selectedLayer);
                }

                CommandManager.InvalidateRequerySuggested();
            }
        }

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

        public LayerViewModel AddLayer(
            string name,
            Color color,
            double lineWeight = 0.13d,
            LineStyle lineStyle = LineStyle.Solid,
            bool isVisible = true,
            bool isFrozen = false,
            bool isLocked = false)
        {
            var layer = documentService.CreateLayer(name, color);
            layer.Style.LineWeight = lineWeight;
            layer.Style.LineStyle = lineStyle;
            layer.Style.IsVisible = isVisible;
            layer.Style.IsFrozen = isFrozen;
            layer.Style.IsLocked = isLocked;

            var vm = new LayerViewModel(layer);
            vm.PropertyChanged += OnLayerViewModelPropertyChanged;
            Layers.Add(vm);
            nextLayerNumber = Math.Max(nextLayerNumber, Layers.Count + 1);

            if (Layers.Count == 1)
            {
                SetActiveLayer(vm);
            }

            SelectLayerWithoutSelectionSideEffects(vm);
            return vm;
        }

        public LayerViewModel AddLayer()
        {
            var color = DefaultLayerPalette[(nextLayerNumber - 1) % DefaultLayerPalette.Length];
            return AddLayer($"Layer {nextLayerNumber++}", color);
        }

        public void AddEntity(Layer layer, Entity entity)
        {
            if (layer == null || entity == null)
                return;

            documentService.AddEntity(layer.Id, entity);
        }

        public Layer GetActiveLayer()
        {
            var displayLayer = GetDisplayedLayer();
            if (displayLayer != null)
                return displayLayer.Layer;

            return GetCreationLayer();
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
            var snapped = hoverFeedbackService.ResolveStatusPoint(editorStateService?.Mode ?? EditorMode.Idle, HasSelectedGrips(), snapResult);
            if (snapped.HasValue)
                StatusText = FormatCoordinates(snapped.Value);
        }

        private void OnViewportMouseLeave(object sender, MouseEventArgs e)
        {
            var pos = viewport.Position;
            StatusText = FormatCoordinates(pos);
        }

        private Point? ResolveHoverStatusPoint(Point worldPoint)
        {
            var snapResult = viewport.GetRubberObject()?.SnapPoint;
            return hoverFeedbackService.ResolveStatusPoint(editorStateService?.Mode ?? EditorMode.Idle, HasSelectedGrips(), snapResult);
        }

        private static string FormatCoordinates(Point point)
        {
            return $"X: {point.X:F2}  Y: {point.Y:F2}";
        }

        private void HandleCommandLineInput(string input)
        {
            var activeInteractiveTool = toolRuntime?.GetActiveInteractiveTool();
            commandLifecycleService.TryHandleCommandLineInput(
                input,
                activeInteractiveTool,
                trimmed => commandFeedbackService?.ParseInput(trimmed) ?? CommandInputToken.Text(trimmed, trimmed),
                token => activeInteractiveTool?.TrySubmitToken(token) ?? false,
                commandRuntime.Execute,
                () => commandFeedbackService?.ActiveCommandName,
                RefreshViewportVisuals,
                message => commandFeedbackService?.LogMessage(message));
        }

        private void CancelCurrentCommand()
        {
            commandRuntime.CancelCurrentCommand();
            RefreshViewportVisuals();
        }

        private void AddDefaultLayer()
        {
            AddLayer();
        }

        private bool CanRemoveSelectedLayer()
        {
            return SelectedLayer != null && Layers.Count > 1;
        }

        private void RemoveSelectedLayer()
        {
            var layerToRemove = SelectedLayer;
            if (layerToRemove == null || Layers.Count <= 1)
                return;

            var wasActive = layerToRemove.IsActive;
            layerToRemove.PropertyChanged -= OnLayerViewModelPropertyChanged;
            layerToRemove.Dispose();
            documentService.RemoveLayer(layerToRemove.Layer.Id);
            Layers.Remove(layerToRemove);

            if (ReferenceEquals(SelectedLayer, layerToRemove))
                SelectLayerWithoutSelectionSideEffects(Layers.FirstOrDefault());

            if (wasActive)
                EnsureActiveLayerIsValid();
        }

        private void SetActiveLayer(LayerViewModel target, bool force = false)
        {
            if (target == null || (!force && !target.CanBeActive))
                return;

            foreach (var layerVm in Layers)
                layerVm.SetActive(ReferenceEquals(layerVm, target));

            SelectLayerWithoutSelectionSideEffects(target);
            editorStateService?.SetActiveLayer(target.Layer);
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanMoveSelectedEntitiesToSelectedLayer()
        {
            return SelectedLayer != null
                && SelectedLayer.CanBeActive
                && selectionManager?.SelectedEntities?.Count > 0;
        }

        private void MoveSelectedEntitiesToSelectedLayer(LayerViewModel targetLayerVm)
        {
            if (targetLayerVm == null || !targetLayerVm.CanBeActive || selectionManager?.SelectedEntities?.Count <= 0)
                return;

            var targetLayer = targetLayerVm.Layer;
            var selectedEntities = selectionManager.SelectedEntities.ToList();
            foreach (var entity in selectedEntities)
                documentService.AddEntity(targetLayer.Id, entity);

            SelectLayerWithoutSelectionSideEffects(targetLayerVm);
            RefreshViewportVisuals();
            CommandManager.InvalidateRequerySuggested();
        }

        private void EnsureActiveLayerIsValid()
        {
            var active = Layers.FirstOrDefault(vm => vm.IsActive && vm.CanBeActive);
            if (active != null)
            {
                editorStateService?.SetActiveLayer(active.Layer);
                if (!ReferenceEquals(SelectedLayer, active))
                    SelectedLayer = active;
                return;
            }

            var fallback = Layers.FirstOrDefault(vm => vm.CanBeActive) ?? Layers.FirstOrDefault();
            if (fallback == null)
            {
                editorStateService?.SetActiveLayer(null);
                if (SelectedLayer != null)
                    SelectedLayer = null;
                return;
            }

            if (!ReferenceEquals(SelectedLayer, fallback))
                SelectLayerWithoutSelectionSideEffects(fallback);

            SetActiveLayer(fallback, true);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSelectionDrivenLayerUpdates)
                return;

            OnPropertyChanged(nameof(SelectedLayer));
            CommandManager.InvalidateRequerySuggested();
        }

        private LayerViewModel ResolveSelectedEntitiesLayer()
        {
            if (selectionManager?.SelectedEntities == null || selectionManager.SelectedEntities.Count == 0)
                return null;

            Layer commonLayer = null;
            foreach (var entity in selectionManager.SelectedEntities)
            {
                var entityLayer = documentService.GetLayerForEntity(entity);
                if (entityLayer == null)
                    return null;

                if (commonLayer == null)
                {
                    commonLayer = entityLayer;
                    continue;
                }

                if (commonLayer.Id != entityLayer.Id)
                    return null;
            }

            return Layers.FirstOrDefault(vm => vm.Layer.Id == commonLayer.Id);
        }

        private bool HasMixedSelectedEntityLayers()
        {
            if (selectionManager?.SelectedEntities == null || selectionManager.SelectedEntities.Count <= 1)
                return false;

            Layer commonLayer = null;
            foreach (var entity in selectionManager.SelectedEntities)
            {
                var entityLayer = documentService.GetLayerForEntity(entity);
                if (entityLayer == null)
                    return true;

                if (commonLayer == null)
                {
                    commonLayer = entityLayer;
                    continue;
                }

                if (commonLayer.Id != entityLayer.Id)
                    return true;
            }

            return false;
        }

        private void SelectLayerWithoutSelectionSideEffects(LayerViewModel layerViewModel)
        {
            suppressSelectionDrivenLayerUpdates = true;
            try
            {
                selectedLayer = layerViewModel;
                OnPropertyChanged(nameof(SelectedLayer));
            }
            finally
            {
                suppressSelectionDrivenLayerUpdates = false;
            }
        }

        private LayerViewModel GetDisplayedLayer()
        {
            var selectionLayer = ResolveSelectedEntitiesLayer();
            if (selectionLayer != null || HasMixedSelectedEntityLayers())
                return selectionLayer;

            return selectedLayer;
        }

        private Layer GetCreationLayer()
        {
            if (selectedLayer != null && selectedLayer.CanBeActive)
                return selectedLayer.Layer;

            if (editorStateService?.ActiveLayer != null)
            {
                var activeVm = Layers.FirstOrDefault(vm => vm.Layer == editorStateService.ActiveLayer);
                if (activeVm != null && activeVm.CanBeActive)
                    return activeVm.Layer;
            }

            var active = Layers.FirstOrDefault(vm => vm.IsActive && vm.CanBeActive);
            if (active != null)
                return active.Layer;

            var firstEditable = Layers.FirstOrDefault(vm => vm.CanBeActive);
            if (firstEditable != null)
                return firstEditable.Layer;

            return Layers.Count > 0 ? Layers[0].Layer : null;
        }

        private void OnLayerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerViewModel.IsVisible) ||
                e.PropertyName == nameof(LayerViewModel.IsFrozen) ||
                e.PropertyName == nameof(LayerViewModel.IsLocked) ||
                e.PropertyName == nameof(LayerViewModel.LineStyle) ||
                e.PropertyName == nameof(LayerViewModel.LineWeight) ||
                e.PropertyName == nameof(LayerViewModel.Color) ||
                e.PropertyName == nameof(LayerViewModel.ColorText))
            {
                EnsureActiveLayerIsValid();
            }

            CommandManager.InvalidateRequerySuggested();
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

        private bool HasSelectedGrips()
        {
            return gripService?.GetSelectedGrips()?.Count > 0;
        }
    }
}
