using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class PolylineCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption UndoKeyword = new CommandKeywordOption("UNDO", new[] { "U" }, "Undo last point.");
        private static readonly CommandStep FirstPointStep = new CommandStep("FirstPoint", "Polyline first point:");
        private static readonly CommandStep NextPointStep = new CommandStep("NextPoint", "Polyline next point:", new[] { "ENTER" }, new[] { UndoKeyword });

        private readonly System.Func<Layer> activeLayerResolver;
        private readonly List<Point> points = new List<Point>();
        private Polyline currentPolyline;

        public PolylineCommandController(System.Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "PLINE";

        public override CommandStep InitialStep => FirstPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            points.Clear();
            currentPolyline = null;
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (points.Count > 0)
            {
                Point lastPoint = points[points.Count - 1];
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(lastPoint, rawPoint));
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = points.Count > 0
                ? host.ResolveFinalPoint(points[points.Count - 1], rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            CommandKeywordOption keyword;
            if (host.CurrentStep != null && host.CurrentStep.TryResolveKeyword(token, out keyword))
            {
                if (keyword == UndoKeyword)
                    return UndoLastPoint(host);
            }

            Point point;
            if (!host.TryResolvePointInput(token, points.Count > 0 ? points[points.Count - 1] : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Cancel("Polyline command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Cancel("Polyline command ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            points.Add(point);
            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (points.Count == 1)
            {
                rbo.CurrentStyle = RubberStyle.Line;
                rbo.SetStart(point);
            }
            else if (points.Count == 2)
            {
                var layer = activeLayerResolver?.Invoke();
                if (layer != null)
                {
                    currentPolyline = new Polyline(points);
                    var document = host.ToolService.GetService<ICadDocumentService>();
                    var cmd = new AddEntityCommand(document, layer.Id, currentPolyline);
                    host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
                }

                rbo.SetStart(point);
            }
            else if (currentPolyline != null)
            {
                currentPolyline.AddPoint(point);
                rbo.SetStart(point);
            }

            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult UndoLastPoint(IInteractiveCommandHost host)
        {
            if (points.Count == 0)
                return InteractiveCommandResult.Unhandled();

            if (points.Count == 1)
            {
                // First point only — no entity was created yet, just reset.
                points.Clear();
                currentPolyline = null;
                host.ToolService.Viewport.GetRubberObject().Cancel();
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            if (points.Count == 2)
            {
                // The polyline was added to the undo stack via AddEntityCommand.
                // Undo that command to remove it cleanly from the document and undo stack.
                host.ToolService.GetService<IUndoRedoService>()?.Undo();
                currentPolyline = null;
                points.RemoveAt(points.Count - 1);
                host.ToolService.Viewport.GetRubberObject().SetStart(points[points.Count - 1]);
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            // N > 2: polyline was mutated in-session — just remove the last point directly.
            points.RemoveAt(points.Count - 1);
            currentPolyline?.RemoveLastPoint();
            host.ToolService.Viewport.GetRubberObject().SetStart(points[points.Count - 1]);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult Cancel(string message)
        {
            points.Clear();
            currentPolyline = null;
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }
    }
}
