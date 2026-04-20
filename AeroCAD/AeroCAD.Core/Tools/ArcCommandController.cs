using System;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Tools
{
    /// <summary>
    /// Controller for the ARC command using the 3-point Start method.
    /// User flow:
    /// 1. Start point
    /// 2. Second point on arc
    /// 3. End point
    /// The three perimeter points determine the CW/CCW direction.
    /// </summary>
    public class ArcCommandController : CommandControllerBase
    {
        private static readonly CommandStep StartPointStep =
            new CommandStep("StartPoint", "Specify start point:");

        private static readonly CommandStep SecondPointStep =
            new CommandStep("SecondPoint", "Specify second point:");

        private static readonly CommandStep EndPointStep =
            new CommandStep("EndPoint", "Specify end point:");

        private readonly Func<Layer> activeLayerResolver;

        private Point startPoint;
        private Point secondPoint;
        private Point endPoint;
        private ArcPhase phase = ArcPhase.WaitingForStart;

        private enum ArcPhase
        {
            WaitingForStart,
            WaitingForSecondPoint,
            WaitingForEnd
        }

        public ArcCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "ARC";

        public override CommandStep InitialStep => StartPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            var rubberObject = host.ToolService.Viewport.GetRubberObject();

            switch (phase)
            {
                case ArcPhase.WaitingForStart:
                    break;

                case ArcPhase.WaitingForSecondPoint:
                    rubberObject.SetMove(host.ResolveFinalPoint(startPoint, rawPoint));
                    rubberObject.Preview = BuildLinePreview(startPoint, rubberObject.End);
                    rubberObject.InvalidateVisual();
                    break;

                case ArcPhase.WaitingForEnd:
                {
                    Point finalEndPoint = host.ResolveFinalPoint(startPoint, rawPoint);
                    rubberObject.Preview = BuildArcPreview(startPoint, secondPoint, finalEndPoint);
                    rubberObject.InvalidateVisual();
                    break;
                }
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final;
            switch (phase)
            {
                case ArcPhase.WaitingForStart:
                    final = host.ResolveFinalPoint(null, rawPoint);
                    break;

                case ArcPhase.WaitingForSecondPoint:
                case ArcPhase.WaitingForEnd:
                    final = host.ResolveFinalPoint(startPoint, rawPoint);
                    break;

                default:
                    final = host.ResolveFinalPoint(null, rawPoint);
                    break;
            }

            return SubmitPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point? origin = phase == ArcPhase.WaitingForSecondPoint || phase == ArcPhase.WaitingForEnd
                ? startPoint
                : (Point?)null;

            Point point;
            if (!host.TryResolvePointInput(token, origin, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "ARC ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "ARC canceled.");
        }

        private InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            switch (phase)
            {
                case ArcPhase.WaitingForStart:
                {
                    startPoint = point;
                    phase = ArcPhase.WaitingForSecondPoint;
                    var rubberObject = host.ToolService.Viewport.GetRubberObject();
                    rubberObject.CurrentStyle = RubberStyle.Line;
                    rubberObject.SetStart(startPoint);
                    return InteractiveCommandResult.MoveToStep(SecondPointStep);
                }

                case ArcPhase.WaitingForSecondPoint:
                {
                    secondPoint = point;
                    phase = ArcPhase.WaitingForEnd;
                    var rubberObject = host.ToolService.Viewport.GetRubberObject();
                    rubberObject.Cancel();
                    rubberObject.ClearPreview();
                    return InteractiveCommandResult.MoveToStep(EndPointStep);
                }

                case ArcPhase.WaitingForEnd:
                {
                    endPoint = point;
                    var arc = ComputeArcFrom3Points(startPoint, secondPoint, endPoint);
                    if (arc == null)
                    {
                        host.ToolService.GetService<ICommandFeedbackService>()?.LogMessage("Points are collinear - cannot create arc.");
                        return InteractiveCommandResult.HandledOnly();
                    }

                    var layer = activeLayerResolver?.Invoke();
                    if (layer != null)
                    {
                        var document = host.ToolService.GetService<ICadDocumentService>();
                        var command = new AddEntityCommand(document, layer.Id, arc);
                        host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
                    }

                    return Finish(host, "ARC created.");
                }

                default:
                    return InteractiveCommandResult.Unhandled();
            }
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            ResetRubberObject(host);
            Reset();
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }

        private void Reset()
        {
            phase = ArcPhase.WaitingForStart;
            startPoint = default(Point);
            secondPoint = default(Point);
            endPoint = default(Point);
        }

        private static Arc ComputeArcFrom3Points(Point p1, Point pMid, Point p2)
        {
            double ax = p1.X;
            double ay = p1.Y;
            double bx = pMid.X;
            double by = pMid.Y;
            double cx = p2.X;
            double cy = p2.Y;

            double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (Math.Abs(d) < 1e-10)
                return null;

            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

            var center = new Point(ux, uy);
            double radius = (center - p1).Length;

            double startAngle = CircularGeometry.GetAngle(center, p1);
            double midAngle = CircularGeometry.GetAngle(center, pMid);
            double endAngle = CircularGeometry.GetAngle(center, p2);

            double ccwStartToMid = CircularGeometry.GetDirectionalDistance(startAngle, midAngle, 1);
            double ccwStartToEnd = CircularGeometry.GetDirectionalDistance(startAngle, endAngle, 1);

            double sweep;
            if (ccwStartToMid <= ccwStartToEnd + 1e-10)
            {
                sweep = ccwStartToEnd;
                if (sweep < 1e-10)
                    sweep = CircularGeometry.TwoPi - 1e-9;
            }
            else
            {
                double cwStartToEnd = CircularGeometry.GetDirectionalDistance(startAngle, endAngle, -1);
                sweep = -cwStartToEnd;
                if (sweep > -1e-10)
                    sweep = -(CircularGeometry.TwoPi - 1e-9);
            }

            return new Arc(center, radius, startAngle, sweep);
        }

        private static GripPreview BuildLinePreview(Point from, Point to)
        {
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(from, to), Colors.Gray, 1.5d, DashStyles.Dash)
            });
        }

        private static GripPreview BuildArcPreview(Point start, Point second, Point end)
        {
            var arc = ComputeArcFrom3Points(start, second, end);
            if (arc == null)
                return GripPreview.Empty;

            var arcGeometry = Arc.BuildGeometry(arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle);

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(start, end), Colors.Gray, 1.0d, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(arcGeometry, Colors.White, 1.5d)
            });
        }
    }
}
