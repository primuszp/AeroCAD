using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Drawing.Markers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Snapping;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class ModelSpaceComposition
    {
        private readonly Viewport viewport;
        private readonly List<IEntityPlugin> plugins = new List<IEntityPlugin>();
        private readonly List<ICadModule> modules = new List<ICadModule>();

        public ModelSpaceComposition(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public ModelSpaceComposition RegisterPlugin(IEntityPlugin plugin)
        {
            plugins.Add(plugin);
            return this;
        }

        public ModelSpaceComposition RegisterModule(ICadModule module)
        {
            if (module != null)
                modules.Add(module);
            return this;
        }

        public Dictionary<Type, object> BuildServices()
        {
            var entityPlugins = plugins.AsReadOnly();
            var pluginDescriptors = entityPlugins.Select(plugin => plugin.Descriptor).ToArray();
            var pluginCatalog = new EntityPluginCatalog(entityPlugins);
            var moduleCatalog = new CadModuleCatalog(modules);
            var interactiveCommandRegistry = new InteractiveCommandRegistry(entityPlugins, modules);
            var shapeRegistry = new InteractiveShapeRegistry(modules.SelectMany(module => module?.Shapes ?? Enumerable.Empty<IInteractiveShapeDefinition>()));
            var pluginDiscoveryService = new PluginDiscoveryService();
            var selectionManager = new SelectionManager();
            var gripService = new GripService(selectionManager);
            var markerAppearance = new MarkerAppearanceService();
            var editorState = new EditorStateService(viewport);
            var entityRenderService = new EntityRenderService(pluginDescriptors.Select(descriptor => descriptor.RenderStrategy).ToArray());
            var document = new CadDocumentService(entityRenderService);
            var entityBoundsService = new EntityBoundsService(pluginDescriptors.Select(descriptor => descriptor.BoundsStrategy).ToArray());
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
            var gripPreviewService = new GripPreviewService(pluginDescriptors.Select(descriptor => descriptor.GripPreviewStrategy).Where(strategy => strategy != null).ToArray());
            var selectionMovePreviewService = new SelectionMovePreviewService(pluginDescriptors.Select(descriptor => descriptor.SelectionMovePreviewStrategy).Where(strategy => strategy != null).ToArray());
            var transientEntityPreviewService = new TransientEntityPreviewService(pluginDescriptors.Select(descriptor => descriptor.TransientEntityPreviewStrategy).Where(strategy => strategy != null).ToArray());
            var entityOffsetService = new EntityOffsetService(pluginDescriptors.Select(descriptor => descriptor.OffsetStrategy).Where(strategy => strategy != null).ToArray());
            var entityTrimExtendService = new EntityTrimExtendService(pluginDescriptors.Select(descriptor => descriptor.TrimExtendStrategy).Where(strategy => strategy != null).ToArray());
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
            var runtimeBootstrapper = new ModelSpaceRuntimeBootstrapper(viewport, document, selectionManager, editorState, overlay, toolService, commandCatalog, shapeRegistry, entityPlugins, modules.AsReadOnly());

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
                { typeof(IEntityPluginCatalog), pluginCatalog },
                { typeof(EntityPluginCatalog), pluginCatalog },
                { typeof(IPluginDiscoveryService), pluginDiscoveryService },
                { typeof(PluginDiscoveryService), pluginDiscoveryService },
                { typeof(ICadModuleCatalog), moduleCatalog },
                { typeof(CadModuleCatalog), moduleCatalog },
                { typeof(IInteractiveCommandRegistry), interactiveCommandRegistry },
                { typeof(InteractiveCommandRegistry), interactiveCommandRegistry },
                { typeof(IInteractiveShapeRegistry), shapeRegistry },
                { typeof(InteractiveShapeRegistry), shapeRegistry },
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
