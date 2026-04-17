using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class LineCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption CloseKeyword =
            new CommandKeywordOption("CLOSE", new[] { "C" }, "Close the line back to the first point.");

        private static readonly CommandStep FirstPointStep =
            new CommandStep("FirstPoint", "Line first point:");

        private static readonly CommandStep NextPointStep =
            new CommandStep("NextPoint", "Line next point:", new[] { "ENTER" }, new[] { CloseKeyword });

        private readonly System.Func<Layer> activeLayerResolver;
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
            // Check for Close keyword first
            CommandKeywordOption keyword;
            if (drawing && host.CurrentStep != null && host.CurrentStep.TryResolveKeyword(token, out keyword))
            {
                if (keyword == CloseKeyword)
                    return CloseLine(host);
            }

            Point point;
            if (!host.TryResolvePointInput(token, drawing ? startPoint : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Cancel("Line command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Cancel("Line command ended.");
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
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = RubberStyle.Line;
                rbo.SetStart(startPoint);
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            CreateLineSegment(host, startPoint, point);
            startPoint = point;
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
        }

        private InteractiveCommandResult CloseLine(IInteractiveCommandHost host)
        {
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput("C");
            CreateLineSegment(host, startPoint, firstPoint);
            drawing = false;
            return InteractiveCommandResult.End("Line command ended.", deactivateTool: true, returnToSelectionMode: true);
        }

        private InteractiveCommandResult Cancel(string message)
        {
            drawing = false;
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }
    }
}

