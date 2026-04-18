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

        public ModelSpaceRuntimeBootstrapper(
            Viewport viewport,
            ICadDocumentService documentService,
            ISelectionManager selectionManager,
            IEditorStateService editorStateService,
            Overlay overlay,
            IToolService toolService,
            IEditorCommandCatalog commandCatalog,
            IReadOnlyList<IEntityPlugin> plugins)
        {
            this.viewport = viewport;
            this.documentService = documentService;
            this.selectionManager = selectionManager;
            this.editorStateService = editorStateService;
            this.overlay = overlay;
            this.toolService = toolService;
            this.commandCatalog = commandCatalog;
            this.plugins = plugins;
        }

        public void Bootstrap()
        {
            WireEvents();
            RegisterDefaultTools();
            RegisterPluginTools();
            RegisterPluginCommands();
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
            toolService.RegisterTool(new LineTool());
            toolService.RegisterTool(new PolylineTool());
            toolService.RegisterTool(new CircleTool());
            toolService.RegisterTool(new ArcTool());
            toolService.RegisterTool(new MoveTool());
            toolService.RegisterTool(new CopyTool());
            toolService.RegisterTool(new OffsetTool());
            toolService.RegisterTool(new TrimTool());
            toolService.RegisterTool(new ExtendTool());
        }

        private void RegisterPluginTools()
        {
            foreach (var plugin in plugins)
            {
                foreach (var tool in plugin.CreateTools())
                    toolService.RegisterTool(tool);
            }
        }

        private void RegisterPluginCommands()
        {
            foreach (var plugin in plugins)
            {
                foreach (var definition in plugin.CreateCommands())
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
