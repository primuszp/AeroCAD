using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class CircleCommandController : CommandControllerBase
    {
        private static readonly CommandStep CenterPointStep = new CommandStep("CenterPoint", "Circle center point:");
        private static readonly CommandStep RadiusPointStep = new CommandStep("RadiusPoint", "Circle radius point or radius:", new[] { "ENTER" });

        private readonly System.Func<Layer> activeLayerResolver;
        private bool hasCenterPoint;
        private Point centerPoint;

        public CircleCommandController(System.Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "CIRCLE";

        public override CommandStep InitialStep => CenterPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (hasCenterPoint)
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(centerPoint, rawPoint));
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = hasCenterPoint
                ? host.ResolveFinalPoint(centerPoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, hasCenterPoint ? centerPoint : (Point?)null, out point))
            {
                double scalar;
                if (hasCenterPoint && host.TryResolveScalarInput(token, out scalar))
                    return SubmitRadius(host, scalar, true);

                return InteractiveCommandResult.Unhandled();
            }

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "Circle command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Circle command ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            var feedback = host.ToolService.GetService<ICommandFeedbackService>();
            if (logInput)
                feedback?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!hasCenterPoint)
            {
                hasCenterPoint = true;
                centerPoint = point;
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = RubberStyle.Circle;
                rbo.SetStart(centerPoint);
                return InteractiveCommandResult.MoveToStep(RadiusPointStep);
            }

            return SubmitRadius(host, (point - centerPoint).Length, false);
        }

        private InteractiveCommandResult SubmitRadius(IInteractiveCommandHost host, double radius, bool logInput)
        {
            radius = System.Math.Abs(radius);

            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(radius.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));

            if (radius > double.Epsilon)
            {
                var layer = activeLayerResolver?.Invoke();
                if (layer != null)
                {
                    var circle = new Circle(centerPoint, radius);
                    var document = host.ToolService.GetService<ICadDocumentService>();
                    var cmd = new AddEntityCommand(document, layer.Id, circle);
                    host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
                }
            }

            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject.Cancel();
            rubberObject.SnapPoint = null;
            hasCenterPoint = false;
            centerPoint = default(Point);
            return InteractiveCommandResult.MoveToStep(CenterPointStep);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            host.ToolService.Viewport.GetRubberObject().SnapPoint = null;
            host.ToolService.Viewport.GetRubberObject().Cancel();
            Reset();
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }

        private void Reset()
        {
            hasCenterPoint = false;
            centerPoint = default(Point);
        }
    }
}
