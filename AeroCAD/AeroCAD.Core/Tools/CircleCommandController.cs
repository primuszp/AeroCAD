using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class CircleCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption DiameterKeyword = new CommandKeywordOption("DIAMETER", new[] { "D" }, "Switch to diameter input.");
        private static readonly CommandStep CenterPointStep = new CommandStep("CenterPoint", "Specify center point:");
        private static readonly CommandStep RadiusPointStep = new CommandStep("RadiusPoint", "Specify radius:", keywords: new[] { DiameterKeyword });
        private static readonly CommandStep DiameterPointStep = new CommandStep("DiameterPoint", "Specify diameter:");

        private readonly System.Func<Layer> activeLayerResolver;
        private readonly CircleInteractiveShapeSession session = new CircleInteractiveShapeSession();

        public CircleCommandController()
            : this(null)
        {
        }

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
            session.Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (session.HasCenterPoint)
            {
                Point final = host.ResolveFinalPoint(session.CenterPoint, rawPoint);
                host.ToolService.Viewport.GetRubberObject().SetMove(final);
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = session.HasCenterPoint
                ? host.ResolveFinalPoint(session.CenterPoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, session.HasCenterPoint ? session.CenterPoint : (Point?)null, out point))
            {
                CommandKeywordOption keyword;
            if (session.HasCenterPoint && TryResolveKeyword(host, token, out keyword))
            {
                if (keyword == DiameterKeyword)
                {
                    session.BeginDiameterInput();
                    host.ToolService.Viewport.GetRubberObject().CurrentStyle = RubberStyle.CircleDiameter;
                    return InteractiveCommandResult.MoveToStep(DiameterPointStep);
                }
            }

            double scalar;
            if (session.HasCenterPoint && host.TryResolveScalarInput(token, out scalar))
                return session.UseDiameterInput
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

            if (!session.HasCenterPoint)
            {
                session.BeginCenter(point);
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.CurrentStyle = RubberStyle.Circle;
                rbo.SetStart(session.CenterPoint);
                return InteractiveCommandResult.MoveToStep(RadiusPointStep);
            }

            return session.UseDiameterInput
                ? SubmitDiameter(host, session.GetDiameterFromPoint(point), false)
                : SubmitRadius(host, session.GetRadiusFromPoint(point), false);
        }

        private InteractiveCommandResult SubmitRadius(IInteractiveCommandHost host, double radius, bool logInput)
        {
            radius = session.GetRadiusFromScalar(radius);

            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(radius.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));

            if (radius > double.Epsilon)
            {
                var layer = ResolveActiveLayer(host);
                if (layer != null)
                {
                    var circle = new Circle(session.CenterPoint, radius);
                    var document = host.ToolService.GetService<ICadDocumentService>();
                    var cmd = new AddEntityCommand(document, layer.Id, circle);
                    host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
                }
            }

            return Finish(host, "CIRCLE created.");
        }

        private InteractiveCommandResult SubmitDiameter(IInteractiveCommandHost host, double diameter, bool logInput)
        {
            diameter = session.GetDiameterFromScalar(diameter);

            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(diameter.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));

            return SubmitRadius(host, diameter / 2.0d, false);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            session.Reset();
            return EndCommand(host, message);
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
    }
}
