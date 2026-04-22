using System.Collections.Generic;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class ModelSpaceRuntimeBootstrapper
    {
        private readonly Viewport viewport;
        private readonly ICadDocumentService documentService;
        private readonly ISelectionManager selectionManager;
        private readonly IEditorStateService editorStateService;
        private readonly Overlay overlay;
        private readonly IToolService toolService;
        private readonly IEditorCommandCatalog commandCatalog;
        private readonly IInteractiveCommandRegistry interactiveCommandRegistry;
        private readonly IInteractiveShapeRegistry shapeRegistry;
        private readonly IEntityPluginCatalog pluginCatalog;
        private readonly ICadModuleCatalog moduleCatalog;

        public ModelSpaceRuntimeBootstrapper(
            Viewport viewport,
            ICadDocumentService documentService,
            ISelectionManager selectionManager,
            IEditorStateService editorStateService,
            Overlay overlay,
            IToolService toolService,
            IEditorCommandCatalog commandCatalog,
            IInteractiveShapeRegistry shapeRegistry,
            IReadOnlyList<IEntityPlugin> plugins,
            IReadOnlyList<ICadModule> modules)
        {
            this.viewport = viewport;
            this.documentService = documentService;
            this.selectionManager = selectionManager;
            this.editorStateService = editorStateService;
            this.overlay = overlay;
            this.toolService = toolService;
            this.commandCatalog = commandCatalog;
            this.shapeRegistry = shapeRegistry;
            this.pluginCatalog = new EntityPluginCatalog(plugins);
            this.interactiveCommandRegistry = new InteractiveCommandRegistry(plugins, modules);
            this.moduleCatalog = new CadModuleCatalog(modules);
        }

        public void Bootstrap()
        {
            WireEvents();
            RegisterDefaultTools();
            RegisterPluginTools();
            RegisterModuleShapes();
            RegisterPluginInteractiveCommands();
            RegisterPluginCommands();
            RegisterModuleCommands();
            ActivateDefaultTools();
        }

        private void WireEvents()
        {
            documentService.LayerAdded += (s, e) => viewport.AddLayer(e.Layer);
            selectionManager.SelectionChanged += (s, e) => overlay.Update();
            documentService.EntityRemoved += (s, e) => selectionManager.Deselect(e.Entity);
            editorStateService.StateChanged += (s, e) => overlay.Update();
        }

        private void RegisterDefaultTools()
        {
            toolService.RegisterTool(new PanZoomTool());
            toolService.RegisterTool(new SelectionTool());
            toolService.RegisterTool(new GripDragTool());
        }

        private void RegisterPluginTools()
        {
            foreach (var plugin in pluginCatalog.Descriptors)
            {
                foreach (var tool in plugin.Tools)
                    toolService.RegisterTool(tool);
            }
        }

        private void RegisterPluginInteractiveCommands()
        {
            foreach (var registration in interactiveCommandRegistry.Registrations)
            {
                toolService.RegisterTool(new RegisteredInteractiveCommandTool(registration.ControllerFactory, registration.ToolName));
                commandCatalog.Register(registration.CreateCommandDefinition());
            }
        }

        private void RegisterModuleShapes()
        {
            foreach (var shape in shapeRegistry?.Definitions ?? System.Linq.Enumerable.Empty<IInteractiveShapeDefinition>())
            {
                if (shape == null)
                    continue;

                var runtime = shape.Pipeline.CreateRuntime();
                var registration = runtime.CreateCommandRegistration();
                toolService.RegisterTool(new RegisteredInteractiveShapeTool(runtime));
                commandCatalog.Register(registration.CreateCommandDefinition());
            }
        }

        private void RegisterPluginCommands()
        {
            foreach (var plugin in pluginCatalog.Descriptors)
            {
                foreach (var definition in plugin.Commands)
                    commandCatalog.Register(definition);
            }
        }

        private void RegisterModuleCommands()
        {
            foreach (var module in moduleCatalog.Modules)
            {
                foreach (var definition in module.Commands ?? System.Linq.Enumerable.Empty<EditorCommandDefinition>())
                    commandCatalog.Register(definition);
            }
        }

        private void ActivateDefaultTools()
        {
            toolService.GetTool<PanZoomTool>()?.Activate();
            toolService.GetTool<SelectionTool>()?.Activate();
        }
    }
}
