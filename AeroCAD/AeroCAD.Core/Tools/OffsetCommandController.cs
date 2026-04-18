using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class OffsetCommandController : CommandControllerBase
    {
        private static readonly CommandStep OffsetInputStep = new CommandStep("OffsetInput", "Specify offset distance or through point:");
        private static readonly CommandStep SidePointStep = new CommandStep("SidePoint", "Specify point on side to offset:");

        private Entity sourceEntity;
        private System.Guid sourceLayerId;
        private System.Windows.Media.Color sourceColor;
        private double? fixedDistance;

        public override string CommandName => "OFFSET";

        public override CommandStep InitialStep => OffsetInputStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();
            var document = host.ToolService.GetService<ICadDocumentService>();

            sourceEntity = selectionManager?.SelectedEntities.Count == 1 ? selectionManager.SelectedEntities[0] : null;
            var sourceLayer = document?.GetLayerForEntity(sourceEntity);
            sourceLayerId = sourceLayer?.Id ?? System.Guid.Empty;
            sourceColor = sourceLayer?.Color ?? System.Windows.Media.Colors.White;
            fixedDistance = null;
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (sourceEntity == null)
                return;

            var previewPoint = host.ResolveFinalPoint(null, rawPoint);
            var previewEntity = fixedDistance.HasValue
                ? host.ToolService.GetService<IEntityOffsetService>()?.CreateOffsetByDistance(sourceEntity, fixedDistance.Value, previewPoint)
                : host.ToolService.GetService<IEntityOffsetService>()?.CreateOffsetThroughPoint(sourceEntity, previewPoint);

            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            var previewService = host.ToolService.GetService<ITransientEntityPreviewService>();
            rubberObject.Preview = previewService?.CreatePreview(previewEntity, sourceColor);
            rubberObject.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            return SubmitResolvedPoint(host, host.ResolveFinalPoint(null, rawPoint), true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (!fixedDistance.HasValue)
            {
                double scalar;
                if (host.TryResolveScalarInput(token, out scalar))
                {
                    fixedDistance = System.Math.Abs(scalar);
                    host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(fixedDistance.Value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                    return InteractiveCommandResult.MoveToStep(SidePointStep);
                }
            }

            Point point;
            if (!host.TryResolvePointInput(token, null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "Offset command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Offset command ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            if (sourceEntity == null || sourceLayerId == System.Guid.Empty)
                return Finish(host, "Offset command ended.");

            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            var offsetService = host.ToolService.GetService<IEntityOffsetService>();
            var resultEntity = fixedDistance.HasValue
                ? offsetService?.CreateOffsetByDistance(sourceEntity, fixedDistance.Value, point)
                : offsetService?.CreateOffsetThroughPoint(sourceEntity, point);

            if (resultEntity == null)
                return InteractiveCommandResult.HandledOnly();

            var document = host.ToolService.GetService<ICadDocumentService>();
            var command = new AddEntityCommand(document, sourceLayerId, resultEntity);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);

            return InteractiveCommandResult.MoveToStep(fixedDistance.HasValue ? SidePointStep : OffsetInputStep);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            if (rubberObject != null)
            {
                rubberObject.SnapPoint = null;
                rubberObject.ClearPreview();
                rubberObject.Cancel();
                rubberObject.InvalidateVisual();
            }

            sourceEntity = null;
            sourceLayerId = System.Guid.Empty;
            fixedDistance = null;
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }
    }
}
