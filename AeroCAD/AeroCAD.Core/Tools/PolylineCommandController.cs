using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class PolylineCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption CloseKeyword = new CommandKeywordOption("CLOSE", new[] { "C" }, "Close the polyline.");
        private static readonly CommandKeywordOption UndoKeyword = new CommandKeywordOption("UNDO", new[] { "U" }, "Undo last point.");
        private static readonly CommandStep FirstPointStep = new CommandStep("FirstPoint", "Specify start point:");
        private static readonly CommandStep NextPointStep = new CommandStep("NextPoint", "Specify next point:", keywords: new[] { CloseKeyword, UndoKeyword });

        private readonly System.Func<Layer> activeLayerResolver;
        private readonly PolylineInteractiveShapeSession session = new PolylineInteractiveShapeSession();

        public PolylineCommandController()
            : this(null)
        {
        }

        public PolylineCommandController(System.Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "PLINE";

        public override CommandStep InitialStep => FirstPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            session.Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (session.HasStarted)
            {
                Point lastPoint = session.Points[session.Points.Count - 1];
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(lastPoint, rawPoint));
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = session.HasStarted
                ? host.ResolveFinalPoint(session.Points[session.Points.Count - 1], rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            CommandKeywordOption keyword;
            if (TryResolveKeyword(host, token, out keyword))
            {
                if (keyword == CloseKeyword)
                    return ClosePolyline(host);

                if (keyword == UndoKeyword)
                    return UndoLastPoint(host);
            }

            Point point;
            if (!host.TryResolvePointInput(token, session.HasStarted ? session.Points[session.Points.Count - 1] : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "PLINE ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "PLINE canceled.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            session.AddPoint(point);
            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (session.Points.Count == 1)
            {
                rbo.CurrentStyle = RubberStyle.Line;
                rbo.SetStart(point);
            }
            else if (session.Points.Count == 2)
            {
                var layer = ResolveActiveLayer(host);
                if (layer != null)
                {
                    session.CreateCurrentPolyline();
                    var document = host.ToolService.GetService<ICadDocumentService>();
                    var cmd = new AddEntityCommand(document, layer.Id, session.CurrentPolyline);
                    host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
                }

                rbo.SetStart(point);
            }
            else if (session.CurrentPolyline != null)
            {
                session.AppendToPolyline(point);
                rbo.SetStart(point);
            }

            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult ClosePolyline(IInteractiveCommandHost host)
        {
            if (!session.CanClose())
                return InteractiveCommandResult.Unhandled();

            session.Close();
            host.ToolService.Viewport.GetRubberObject().SetStart(session.Points[0]);

            return Finish(host, "PLINE created.");
        }

        private InteractiveCommandResult UndoLastPoint(IInteractiveCommandHost host)
        {
            if (!session.CanUndo())
                return InteractiveCommandResult.Unhandled();

            if (session.Points.Count == 1)
            {
                session.Reset();
                host.ToolService.Viewport.GetRubberObject().Cancel();
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            if (session.Points.Count == 2)
            {
                // The polyline was added to the undo stack via AddEntityCommand.
                host.ToolService.GetService<IUndoRedoService>()?.Undo();
                session.UndoLastPoint(out _);
                host.ToolService.Viewport.GetRubberObject().SetStart(session.Points[0]);
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            session.UndoLastPoint(out _);
            host.ToolService.Viewport.GetRubberObject().SetStart(session.Points[session.Points.Count - 1]);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private Layer ResolveActiveLayer(IInteractiveCommandHost host)
        {
            if (activeLayerResolver != null)
                return activeLayerResolver();

            var editorState = host?.ToolService?.GetService<IEditorStateService>();
            if (editorState?.ActiveLayer != null)
                return editorState.ActiveLayer;

            var document = host?.ToolService?.GetService<ICadDocumentService>();
            return document?.Layers?.Count > 0 ? document.Layers[0] : null;
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            session.Reset();
            return EndCommand(host, message);
        }
    }
}
