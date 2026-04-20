using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Tools
{
    public class LineCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption CloseKeyword =
            new CommandKeywordOption("Close", new[] { "C" }, "Close the line back to the first point.");

        private static readonly CommandKeywordOption UndoKeyword =
            new CommandKeywordOption("Undo", new[] { "U" }, "Remove the last segment.");

        private static readonly CommandStep FirstPointStep =
            new CommandStep("FirstPoint", "Specify first point:");

        private static readonly CommandStep NextPointStep =
            new CommandStep("NextPoint", "Specify next point:", keywords: new[] { CloseKeyword, UndoKeyword });

        private readonly System.Func<Layer> activeLayerResolver;
        private readonly List<Point> vertices = new List<Point>();
        private readonly List<Line> createdSegments = new List<Line>();
        private bool drawing;
        private Point startPoint;
        private Point firstPoint;

        public LineCommandController(System.Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "LINE";

        public override CommandStep InitialStep => FirstPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            drawing = false;
            startPoint = default(Point);
            firstPoint = default(Point);
            vertices.Clear();
            createdSegments.Clear();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (drawing)
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(startPoint, rawPoint));
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = drawing
                ? host.ResolveFinalPoint(startPoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            CommandKeywordOption keyword;
            if (drawing && TryResolveKeyword(host, token, out keyword))
            {
                if (keyword == CloseKeyword)
                    return CloseLine(host);

                if (keyword == UndoKeyword)
                    return UndoLastSegment(host);
            }

            Point point;
            if (!host.TryResolvePointInput(token, drawing ? startPoint : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Cancel(host, "LINE ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Cancel(host, "LINE canceled.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            var feedback = host.ToolService.GetService<ICommandFeedbackService>();
            if (logInput)
                feedback?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!drawing)
            {
                drawing = true;
                startPoint = point;
                firstPoint = point;
                vertices.Add(point);
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = RubberStyle.Line;
                rbo.SetStart(startPoint);
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            CreateLineSegment(host, startPoint, point);
            startPoint = point;
            vertices.Add(point);
            host.ToolService.Viewport.GetRubberObject().SetStart(startPoint);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private void CreateLineSegment(IInteractiveCommandHost host, Point from, Point to)
        {
            var layer = activeLayerResolver?.Invoke();
            if (layer == null)
                return;

            var line = new Line(from, to);
            var document = host.ToolService.GetService<ICadDocumentService>();
            var cmd = new AddEntityCommand(document, layer.Id, line);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
            createdSegments.Add(line);
        }

        private InteractiveCommandResult CloseLine(IInteractiveCommandHost host)
        {
            if (createdSegments.Count < 2)
                return InteractiveCommandResult.MoveToStep(NextPointStep);

            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput("Close");
            CreateLineSegment(host, startPoint, firstPoint);
            return Finish(host, "LINE ended.");
        }

        private InteractiveCommandResult UndoLastSegment(IInteractiveCommandHost host)
        {
            if (createdSegments.Count == 0 || vertices.Count <= 1)
                return InteractiveCommandResult.MoveToStep(NextPointStep);

            var document = host.ToolService.GetService<ICadDocumentService>();
            var undoRedo = host.ToolService.GetService<IUndoRedoService>();
            var lastSegment = createdSegments[createdSegments.Count - 1];
            undoRedo?.Execute(new RemoveEntitiesCommand(document, new Entity[] { lastSegment }, "Undo Line Segment"));

            createdSegments.RemoveAt(createdSegments.Count - 1);
            vertices.RemoveAt(vertices.Count - 1);
            startPoint = vertices[vertices.Count - 1];
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput("Undo");
            host.ToolService.Viewport.GetRubberObject().SetStart(startPoint);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult Cancel(IInteractiveCommandHost host, string message)
        {
            return Finish(host, message);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            drawing = false;
            vertices.Clear();
            createdSegments.Clear();
            ResetRubberObject(host);
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }
    }
}

