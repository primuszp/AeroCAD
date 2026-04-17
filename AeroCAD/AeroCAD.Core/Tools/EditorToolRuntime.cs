using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class EditorToolRuntime : IEditorToolRuntime
    {
        private readonly IToolService toolService;
        private readonly IEditorStateService editorStateService;

        public EditorToolRuntime(IToolService toolService, IEditorStateService editorStateService)
        {
            this.toolService = toolService;
            this.editorStateService = editorStateService;
        }

        public ICommandInteractiveTool GetActiveInteractiveTool()
        {
            return toolService?.Tools
                .Where(tool => tool.IsActive)
                .OrderByDescending(tool => tool.InputPriority)
                .OfType<ICommandInteractiveTool>()
                .FirstOrDefault();
        }

        public bool CancelActiveInteractiveTool()
        {
            var activeInteractiveTool = GetActiveInteractiveTool();
            return activeInteractiveTool != null && activeInteractiveTool.TryCancel();
        }

        public bool ActivateSelectionMode()
        {
            CancelActiveInteractiveTool();

            editorStateService?.SetMode(EditorMode.Idle);
            var selectionTool = toolService?.GetTool<SelectionTool>();
            if (selectionTool == null)
                return false;

            selectionTool.IsSuspended = false;
            if (!selectionTool.IsActive)
                return toolService.ActivateTool(selectionTool);

            if (toolService.Viewport != null)
            {
                toolService.Viewport.ActiveCursorType = selectionTool.CursorType;
                toolService.Viewport.GetRubberObject()?.InvalidateVisual();
            }

            return true;
        }

        public bool ActivateModalTool<TTool>(Layer activeLayer = null) where TTool : class, ITool
        {
            CancelOtherModalTools(typeof(TTool));

            var tool = toolService?.GetTool<TTool>();
            if (tool == null)
                return false;

            var layerBoundTool = tool as ILayerBoundTool;
            if (layerBoundTool != null && activeLayer != null)
                layerBoundTool.ActiveLayer = activeLayer;

            return toolService.ActivateTool(tool);
        }

        public bool ActivateModalTool(System.Type toolType, Layer activeLayer = null)
        {
            if (toolType == null || !typeof(ITool).IsAssignableFrom(toolType))
                return false;

            CancelOtherModalTools(toolType);

            var tool = toolService?.Tools.FirstOrDefault(candidate => candidate.GetType() == toolType);
            if (tool == null)
                return false;

            var layerBoundTool = tool as ILayerBoundTool;
            if (layerBoundTool != null && activeLayer != null)
                layerBoundTool.ActiveLayer = activeLayer;

            return toolService.ActivateTool(tool);
        }

        private void CancelOtherModalTools(System.Type keepType)
        {
            if (toolService?.Tools == null)
                return;

            foreach (var modalTool in toolService.Tools.Where(tool => tool.IsActive && tool is IModalTool && tool.GetType() != keepType))
            {
                var interactiveTool = modalTool as ICommandInteractiveTool;
                if (interactiveTool != null)
                    interactiveTool.TryCancel();
                else
                    toolService.DeactivateTool(modalTool);
            }
        }
    }
}
