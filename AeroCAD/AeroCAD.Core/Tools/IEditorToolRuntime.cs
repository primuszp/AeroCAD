using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Tools
{
    public interface IEditorToolRuntime
    {
        ICommandInteractiveTool GetActiveInteractiveTool();

        bool CancelActiveInteractiveTool();

        bool ActivateSelectionMode();

        bool ActivateModalTool<TTool>(Layer activeLayer = null) where TTool : class, ITool;

        bool ActivateModalTool(System.Type toolType, Layer activeLayer = null);
    }
}
