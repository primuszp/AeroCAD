using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Drawing.Markers;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Snapping;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class ModelSpaceComposition
    {
        private readonly Viewport viewport;
        private readonly List<IEntityPlugin> plugins = new List<IEntityPlugin>();

        public ModelSpaceComposition(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public ModelSpaceComposition RegisterPlugin(IEntityPlugin plugin)
        {
            plugins.Add(plugin);
            return this;
        }

        public Dictionary<Type, object> BuildServices()
        {
            var selectionManager = new SelectionManager();
            var gripService = new GripService(selectionManager);
            var markerAppearance = new MarkerAppearanceService();
            var editorState = new EditorStateService(viewport);
            var entityRenderService = new EntityRenderService(
                new IEntityRenderStrategy[]
                {
                    new LineEntityRenderStrategy(),
                    new PolylineEntityRenderStrategy(),
                    new CircleEntityRenderStrategy(),
                    new ArcEntityRenderStrategy()
                }.Concat(plugins.Select(p => p.RenderStrategy).Where(s => s != null)).ToArray());
            var document = new CadDocumentService(entityRenderService);
            var entityBoundsService = new EntityBoundsService(
                new IEntityBoundsStrategy[]
                {
                    new LineBoundsStrategy(),
                    new PolylineBoundsStrategy(),
                    new CircleBoundsStrategy(),
                    new ArcBoundsStrategy()
                }.Concat(plugins.Select(p => p.BoundsStrategy).Where(s => s != null)).ToArray());
            var spatialQueryService = new SpatialQueryService(document, entityBoundsService);
            var pickResolutionService = new PickResolutionService(document);
            var overlay = new Overlay(viewport, markerAppearance, gripService);
            var rubberObject = new RubberObject(viewport, markerAppearance);
            var toolService = new ToolService(new DictionaryServiceProvider(() => Services), viewport);
            var toolRuntime = new EditorToolRuntime(toolService, editorState);
            var commandFeedback = new CommandFeedbackService();
            var commandCatalog = new EditorCommandCatalog();
            var gridSettings = new GridSettingsService();
            var pickSettings = new PickSettingsService();
            var gridLayer = new GridLayer(viewport, gridSettings);
            var gripPreviewService = new GripPreviewService(
                new IGripPreviewStrategy[]
                {
                    new LineGripPreviewStrategy(),
                    new PolylineGripPreviewStrategy(),
                    new CircleGripPreviewStrategy(),
                    new ArcGripPreviewStrategy()
                }.Concat(plugins.Select(p => p.GripPreviewStrategy).Where(s => s != null)).ToArray());
            var selectionMovePreviewService = new SelectionMovePreviewService(
                new ISelectionMovePreviewStrategy[]
                {
                    new LineSelectionMovePreviewStrategy(),
                    new PolylineSelectionMovePreviewStrategy(),
                    new CircleSelectionMovePreviewStrategy(),
                    new ArcSelectionMovePreviewStrategy()
                }.Concat(plugins.Select(p => p.SelectionMovePreviewStrategy).Where(s => s != null)).ToArray());
            var transientEntityPreviewService = new TransientEntityPreviewService(
                new ITransientEntityPreviewStrategy[]
                {
                    new LineTransientEntityPreviewStrategy(),
                    new PolylineTransientEntityPreviewStrategy(),
                    new CircleTransientEntityPreviewStrategy(),
                    new ArcTransientEntityPreviewStrategy()
                }.Concat(plugins.Select(p => p.TransientEntityPreviewStrategy).Where(s => s != null)).ToArray());
            var entityOffsetService = new EntityOffsetService(
                new IEntityOffsetStrategy[]
                {
                    new LineOffsetStrategy(),
                    new PolylineOffsetStrategy(),
                    new CircleOffsetStrategy(),
                    new ArcOffsetStrategy()
                }.Concat(plugins.Select(p => p.OffsetStrategy).Where(s => s != null)).ToArray());
            var entityTrimExtendService = new EntityTrimExtendService(
                new IEntityTrimExtendStrategy[]
                {
                    new LineTrimExtendStrategy(),
                    new PolylineTrimExtendStrategy(),
                    new CircleTrimExtendStrategy(),
                    new ArcTrimExtendStrategy()
                }.Concat(plugins.Select(p => p.TrimExtendStrategy).Where(s => s != null)).ToArray());
            var snapDescriptorService = new SnapDescriptorService(new ISnapDescriptorProvider[]
            {
                new EntitySnapDescriptorProvider(),
                new SelectedGripSnapDescriptorProvider(gripService)
            });
            var snapModePolicy = new SnapModePolicy(
                new[]
                {
                    SnapType.Endpoint,
                    SnapType.Midpoint,
                    SnapType.Center,
                    SnapType.Quadrant
                },
                new[]
                {
                    SnapType.Endpoint,
                    SnapType.Midpoint,
                    SnapType.Center,
                    SnapType.Quadrant,
                    SnapType.Nearest
                });
            var snapEngine = new SnapEngine(snapModePolicy);
            var undoRedoService = new UndoRedoService();
            var orthoService = new OrthoService();
            var runtimeBootstrapper = new ModelSpaceRuntimeBootstrapper(viewport, document, selectionManager, editorState, overlay, toolService, commandCatalog, plugins.AsReadOnly());

            Services = new Dictionary<Type, object>
            {
                { typeof(ISelectionManager), selectionManager },
                { typeof(SelectionManager), selectionManager },
                { typeof(IGripService), gripService },
                { typeof(GripService), gripService },
                { typeof(IMarkerAppearanceService), markerAppearance },
                { typeof(MarkerAppearanceService), markerAppearance },
                { typeof(ICadDocumentService), document },
                { typeof(CadDocumentService), document },
                { typeof(ISnapEngine), snapEngine },
                { typeof(SnapEngine), snapEngine },
                { typeof(ISnapDescriptorService), snapDescriptorService },
                { typeof(SnapDescriptorService), snapDescriptorService },
                { typeof(ISnapModePolicy), snapModePolicy },
                { typeof(SnapModePolicy), snapModePolicy },
                { typeof(IUndoRedoService), undoRedoService },
                { typeof(UndoRedoService), undoRedoService },
                { typeof(IEditorStateService), editorState },
                { typeof(EditorStateService), editorState },
                { typeof(ICommandFeedbackService), commandFeedback },
                { typeof(CommandFeedbackService), commandFeedback },
                { typeof(IEditorCommandCatalog), commandCatalog },
                { typeof(EditorCommandCatalog), commandCatalog },
                { typeof(IEntityRenderService), entityRenderService },
                { typeof(EntityRenderService), entityRenderService },
                { typeof(IEntityBoundsService), entityBoundsService },
                { typeof(EntityBoundsService), entityBoundsService },
                { typeof(ISpatialQueryService), spatialQueryService },
                { typeof(SpatialQueryService), spatialQueryService },
                { typeof(IPickResolutionService), pickResolutionService },
                { typeof(PickResolutionService), pickResolutionService },
                { typeof(IGripPreviewService), gripPreviewService },
                { typeof(GripPreviewService), gripPreviewService },
                { typeof(ISelectionMovePreviewService), selectionMovePreviewService },
                { typeof(SelectionMovePreviewService), selectionMovePreviewService },
                { typeof(ITransientEntityPreviewService), transientEntityPreviewService },
                { typeof(TransientEntityPreviewService), transientEntityPreviewService },
                { typeof(IEntityOffsetService), entityOffsetService },
                { typeof(EntityOffsetService), entityOffsetService },
                { typeof(IEntityTrimExtendService), entityTrimExtendService },
                { typeof(EntityTrimExtendService), entityTrimExtendService },
                { typeof(IOrthoService), orthoService },
                { typeof(OrthoService), orthoService },
                { typeof(IGridSettingsService), gridSettings },
                { typeof(GridSettingsService), gridSettings },
                { typeof(IPickSettingsService), pickSettings },
                { typeof(PickSettingsService), pickSettings },
                { typeof(GridLayer), gridLayer },
                { typeof(Overlay), overlay },
                { typeof(RubberObject), rubberObject },
                { typeof(IToolService), toolService },
                { typeof(ToolService), toolService },
                { typeof(IEditorToolRuntime), toolRuntime },
                { typeof(EditorToolRuntime), toolRuntime },
                { typeof(ModelSpaceRuntimeBootstrapper), runtimeBootstrapper }
            };

            return Services;
        }

        public void Bootstrap()
        {
            GetService<ModelSpaceRuntimeBootstrapper>()?.Bootstrap();
        }

        public Dictionary<Type, object> Services { get; private set; }

        private T GetService<T>() where T : class
        {
            return Services != null && Services.TryGetValue(typeof(T), out var service) ? service as T : null;
        }

        private sealed class DictionaryServiceProvider : IServiceProvider
        {
            private readonly Func<Dictionary<Type, object>> servicesAccessor;

            public DictionaryServiceProvider(Func<Dictionary<Type, object>> servicesAccessor)
            {
                this.servicesAccessor = servicesAccessor;
            }

            public object GetService(Type serviceType)
            {
                var services = servicesAccessor?.Invoke();
                return services != null && services.TryGetValue(serviceType, out var service) ? service : null;
            }
        }
    }
}
