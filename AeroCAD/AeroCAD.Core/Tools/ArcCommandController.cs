using System;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
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
        private readonly ArcInteractiveShapeSession session = new ArcInteractiveShapeSession();

        public ArcCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "ARC";

        public override CommandStep InitialStep => StartPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            session.Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            var rubberObject = host.ToolService.Viewport.GetRubberObject();

            switch (session.Phase)
            {
                case ArcInteractiveShapeSession.ArcPhase.WaitingForStart:
                    break;

                case ArcInteractiveShapeSession.ArcPhase.WaitingForSecondPoint:
                    rubberObject.Preview = session.BuildLinePreview(host.ResolveFinalPoint(session.StartPoint, rawPoint));
                    break;

                case ArcInteractiveShapeSession.ArcPhase.WaitingForEnd:
                {
                    Point finalEndPoint = host.ResolveFinalPoint(session.StartPoint, rawPoint);
                    rubberObject.Preview = session.BuildArcPreview(finalEndPoint);
                    break;
                }
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final;
            switch (session.Phase)
            {
                case ArcInteractiveShapeSession.ArcPhase.WaitingForStart:
                    final = host.ResolveFinalPoint(null, rawPoint);
                    break;

                case ArcInteractiveShapeSession.ArcPhase.WaitingForSecondPoint:
                case ArcInteractiveShapeSession.ArcPhase.WaitingForEnd:
                    final = host.ResolveFinalPoint(session.StartPoint, rawPoint);
                    break;

                default:
                    final = host.ResolveFinalPoint(null, rawPoint);
                    break;
            }

            return SubmitPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point? origin = session.Phase == ArcInteractiveShapeSession.ArcPhase.WaitingForSecondPoint || session.Phase == ArcInteractiveShapeSession.ArcPhase.WaitingForEnd
                ? session.StartPoint
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

            switch (session.Phase)
            {
                case ArcInteractiveShapeSession.ArcPhase.WaitingForStart:
                {
                    session.BeginStart(point);
                    var rubberObject = host.ToolService.Viewport.GetRubberObject();
                    rubberObject.Cancel();
                    rubberObject.ClearPreview();
                    return InteractiveCommandResult.MoveToStep(SecondPointStep);
                }

                case ArcInteractiveShapeSession.ArcPhase.WaitingForSecondPoint:
                {
                    session.BeginSecond(point);
                    var rubberObject = host.ToolService.Viewport.GetRubberObject();
                    rubberObject.Cancel();
                    rubberObject.ClearPreview();
                    return InteractiveCommandResult.MoveToStep(EndPointStep);
                }

                case ArcInteractiveShapeSession.ArcPhase.WaitingForEnd:
                {
                    var arc = session.BuildArc(point);
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
            session.Reset();
            return EndCommand(host, message);
        }
    }
}
