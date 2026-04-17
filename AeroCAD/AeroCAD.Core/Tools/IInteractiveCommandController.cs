using System.Windows;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public interface IInteractiveCommandController
    {
        string CommandName { get; }

        CommandStep InitialStep { get; }

        EditorMode EditorMode { get; }

        void OnActivated(IInteractiveCommandHost host);

        void OnPointerMove(IInteractiveCommandHost host, Point rawPoint);

        InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint);

        InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token);

        InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host);

        InteractiveCommandResult TryComplete(IInteractiveCommandHost host);

        InteractiveCommandResult TryCancel(IInteractiveCommandHost host);
    }
}

