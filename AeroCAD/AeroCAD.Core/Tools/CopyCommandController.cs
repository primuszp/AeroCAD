using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Tools
{
    public class CopyCommandController : CommandControllerBase
    {
        private static readonly CommandStep BasePointStep = new CommandStep("BasePoint", "Specify base point:");
        private static readonly CommandStep TargetPointStep = new CommandStep("TargetPoint", "Specify second point:");

        private readonly MoveCopyInteractiveShapeSession session = new MoveCopyInteractiveShapeSession();
        private readonly Dictionary<System.Guid, System.Guid> sourceLayers = new Dictionary<System.Guid, System.Guid>();

        public override string CommandName => "COPY";

        public override CommandStep InitialStep => BasePointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var document = host.ToolService.GetService<ICadDocumentService>();
            var selectionManager = host.ToolService.GetService<ISelectionManager>();
            session.InitializeSelection(selectionManager);

            if (selectionManager != null)
            {
                foreach (var entity in session.SelectedEntities)
                    selectionManager.Deselect(entity);
            }

            sourceLayers.Clear();
            if (document != null)
            {
                foreach (var entity in session.SelectedEntities)
                {
                    var layer = document.GetLayerForEntity(entity);
                    if (layer != null)
                        sourceLayers[entity.Id] = layer.Id;
                }
            }
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (!session.HasBasePoint)
                return;

            var finalPoint = host.ResolveFinalPoint(session.BasePoint, rawPoint);
            host.ToolService.Viewport.GetRubberObject().Preview =
                session.BuildPreview(host.ToolService.GetService<ISelectionMovePreviewService>(), finalPoint);
            host.ToolService.Viewport.GetRubberObject().InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = session.HasBasePoint
                ? host.ResolveFinalPoint(session.BasePoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, session.HasBasePoint ? session.BasePoint : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "Copy command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Copy command ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!session.HasBasePoint)
            {
                session.BeginBasePoint(point);
                return InteractiveCommandResult.MoveToStep(TargetPointStep);
            }

            CommitCopy(host, point - session.BasePoint);
            return ContinueCopy(host);
        }

        private void CommitCopy(IInteractiveCommandHost host, Vector displacement)
        {
            var document = host.ToolService.GetService<ICadDocumentService>();
            if (document == null)
                return;

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
                return;

            var command = new AddEntitiesCommand(
                document,
                records,
                records.Count == 1 ? "Copy Entity" : "Copy Entities");

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update();
            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (rbo != null)
            {
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.InvalidateVisual();
            }
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            session.Reset();
            sourceLayers.Clear();

            return EndCommand(host, message, returnToSelectionMode: false);
        }

        private InteractiveCommandResult ContinueCopy(IInteractiveCommandHost host)
        {
            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (rbo != null)
            {
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.InvalidateVisual();
            }

            return InteractiveCommandResult.MoveToStep(TargetPointStep);
        }

    }
}
