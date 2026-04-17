using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public interface IInteractiveCommandHost
    {
        IToolService ToolService { get; }

        CommandStep CurrentStep { get; }

        bool TryResolveScalarInput(CommandInputToken token, out double scalar);

        bool TryResolvePointInput(CommandInputToken token, System.Windows.Point? basePoint, out System.Windows.Point point);

        System.Windows.Point ResolveFinalPoint(System.Windows.Point? basePoint, System.Windows.Point rawPos);

        void MoveToStep(CommandStep step);

        void EndSession(string closingMessage = null);

        void DeactivateTool();

        void ReturnToSelectionMode();

        bool ApplyResult(InteractiveCommandResult result);
    }
}

