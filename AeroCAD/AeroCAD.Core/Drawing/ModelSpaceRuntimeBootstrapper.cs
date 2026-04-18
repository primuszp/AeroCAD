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
        private readonly IReadOnlyList<IEntityPlugin> plugins;
        private readonly IReadOnlyList<ICadModule> modules;

        public ModelSpaceRuntimeBootstrapper(
            Viewport viewport,
            ICadDocumentService documentService,
            ISelectionManager selectionManager,
            IEditorStateService editorStateService,
            Overlay overlay,
            IToolService toolService,
            IEditorCommandCatalog commandCatalog,
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
            this.plugins = plugins;
            this.modules = modules;
        }

        public void Bootstrap()
        {
            WireEvents();
            RegisterDefaultTools();
            RegisterPluginTools();
            RegisterPluginInteractiveCommands();
            RegisterPluginCommands();
            RegisterModuleInteractiveCommands();
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
            foreach (var plugin in plugins)
            {
                foreach (var tool in plugin.Descriptor.Tools)
                    toolService.RegisterTool(tool);
            }
        }

        private void RegisterPluginInteractiveCommands()
        {
            foreach (var plugin in plugins)
            {
                foreach (var registration in plugin.Descriptor.InteractiveCommands)
                {
                    toolService.RegisterTool(new RegisteredInteractiveCommandTool(registration.ControllerFactory, registration.ToolName));
                    commandCatalog.Register(registration.CreateCommandDefinition());
                }
            }
        }

        private void RegisterPluginCommands()
        {
            foreach (var plugin in plugins)
            {
                foreach (var definition in plugin.Descriptor.Commands)
                    commandCatalog.Register(definition);
            }
        }

        private void RegisterModuleInteractiveCommands()
        {
            foreach (var module in modules)
            {
                foreach (var registration in module.InteractiveCommands ?? System.Linq.Enumerable.Empty<InteractiveCommandRegistration>())
                {
                    toolService.RegisterTool(new RegisteredInteractiveCommandTool(registration.ControllerFactory, registration.ToolName));
                    commandCatalog.Register(registration.CreateCommandDefinition());
                }
            }
        }

        private void RegisterModuleCommands()
        {
            foreach (var module in modules)
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
