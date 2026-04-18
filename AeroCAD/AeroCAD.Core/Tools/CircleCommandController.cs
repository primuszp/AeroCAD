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
        private static readonly CommandKeywordOption DiameterKeyword = new CommandKeywordOption("DIAMETER", new[] { "DIA", "D" }, "Switch to diameter input.");
        private static readonly CommandStep CenterPointStep = new CommandStep("CenterPoint", "Specify center point:");
        private static readonly CommandStep RadiusPointStep = new CommandStep("RadiusPoint", "Specify radius or [Diameter]:", new[] { "ENTER" }, new[] { DiameterKeyword });
        private static readonly CommandStep DiameterPointStep = new CommandStep("DiameterPoint", "Specify diameter or second point:");

        private readonly System.Func<Layer> activeLayerResolver;
        private bool hasCenterPoint;
        private Point centerPoint;
        private bool useDiameterInput;

        public CircleCommandController(System.Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "CIRCLE";

        public override CommandStep InitialStep => CenterPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject?.ClearPreview();
            if (rubberObject != null)
            {
                rubberObject.SnapPoint = null;
                rubberObject.Cancel();
            }
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
                CommandKeywordOption keyword;
                if (hasCenterPoint && host.CurrentStep != null && host.CurrentStep.TryResolveKeyword(token, out keyword))
                {
                    if (keyword == DiameterKeyword)
                    {
                        useDiameterInput = true;
                        return InteractiveCommandResult.MoveToStep(DiameterPointStep);
                    }
                }

                double scalar;
                if (hasCenterPoint && host.TryResolveScalarInput(token, out scalar))
                    return useDiameterInput
                        ? SubmitDiameter(host, scalar, true)
                        : SubmitRadius(host, scalar, true);

                return InteractiveCommandResult.Unhandled();
            }

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "CIRCLE ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "CIRCLE canceled.");
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
                useDiameterInput = false;
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.CurrentStyle = RubberStyle.Circle;
                rbo.SetStart(centerPoint);
                return InteractiveCommandResult.MoveToStep(RadiusPointStep);
            }

            return useDiameterInput
                ? SubmitDiameter(host, (point - centerPoint).Length * 2.0d, false)
                : SubmitRadius(host, (point - centerPoint).Length, false);
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

            return Finish(host, "CIRCLE created.");
        }

        private InteractiveCommandResult SubmitDiameter(IInteractiveCommandHost host, double diameter, bool logInput)
        {
            diameter = System.Math.Abs(diameter);

            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(diameter.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));

            return SubmitRadius(host, diameter / 2.0d, false);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject.SnapPoint = null;
            rubberObject.ClearPreview();
            rubberObject.Cancel();
            rubberObject.InvalidateVisual();
            Reset();
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }

        private void Reset()
        {
            hasCenterPoint = false;
            centerPoint = default(Point);
            useDiameterInput = false;
        }
    }
}
