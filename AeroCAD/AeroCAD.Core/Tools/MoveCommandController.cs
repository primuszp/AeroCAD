using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class MoveCommandController : MoveCopyCommandControllerBase
    {
        public override string CommandName => "MOVE";

        protected override string EndedMessage => "Move command ended.";

        protected override bool ShouldReturnToSelectionModeOnFinish() => true;

        protected override InteractiveCommandResult CommitDisplacement(IInteractiveCommandHost host, Vector displacement)
        {
            var updatedRecords = session.StateRecords
                .Select(record =>
                {
                    var after = record.Before.Clone();
                    after.Translate(displacement);
                    return new ModifyEntitiesCommand.EntityStateRecord(record.Target, record.Before, after);
                })
                .ToList();

            var command = new ModifyEntitiesCommand(
                updatedRecords,
                "Move Selection",
                () => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update());

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            ClearRubberPreview(host);
            return FinishMove(host);
        }

        private InteractiveCommandResult FinishMove(IInteractiveCommandHost host)
        {
            session.Reset();
            return EndCommand(host, EndedMessage);
        }
    }
}
