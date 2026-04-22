using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Tools
{
    public class CopyCommandController : MoveCopyCommandControllerBase
    {
        private readonly Dictionary<System.Guid, System.Guid> sourceLayers = new Dictionary<System.Guid, System.Guid>();

        public override string CommandName => "COPY";

        protected override string EndedMessage => "Copy command ended.";

        protected override bool ShouldReturnToSelectionModeOnFinish() => false;

        protected override void OnSelectionInitialized(IInteractiveCommandHost host)
        {
            sourceLayers.Clear();
            var document = host.ToolService.GetService<ICadDocumentService>();
            if (document == null)
                return;

            foreach (var entity in session.SelectedEntities)
            {
                var layer = document.GetLayerForEntity(entity);
                if (layer != null)
                    sourceLayers[entity.Id] = layer.Id;
            }
        }

        protected override InteractiveCommandResult CommitDisplacement(IInteractiveCommandHost host, System.Windows.Vector displacement)
        {
            var document = host.ToolService.GetService<ICadDocumentService>();
            if (document == null)
                return InteractiveCommandResult.HandledOnly();

            var records = session.SelectedEntities
                .Where(entity => entity != null && sourceLayers.ContainsKey(entity.Id))
                .Select(entity =>
                {
                    var duplicate = entity.Duplicate();
                    duplicate.Translate(displacement);
                    return new AddEntitiesCommand.AddedEntityRecord(sourceLayers[entity.Id], duplicate);
                })
                .ToList();

            if (records.Count == 0)
                return InteractiveCommandResult.HandledOnly();

            var command = new AddEntitiesCommand(
                document,
                records,
                records.Count == 1 ? "Copy Entity" : "Copy Entities");

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update();
            ClearRubberPreview(host);
            return InteractiveCommandResult.MoveToStep(TargetPointStep);
        }
    }
}
