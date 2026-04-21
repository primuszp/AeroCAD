using System;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class RectangleCommandController : CommandControllerBase
    {
        private static readonly CommandStep FirstCornerStep =
            new CommandStep("FirstCorner", "Specify first corner:");

        private static readonly CommandStep OppositeCornerStep =
            new CommandStep("OppositeCorner", "Specify opposite corner:");

        private readonly Func<Layer> activeLayerResolver;
        private bool hasFirstCorner;
        private Point firstCorner;

        public RectangleCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "RECTANGLE";
        public override CommandStep InitialStep => FirstCornerStep;
        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            hasFirstCorner = false;
            firstCorner = default(Point);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (hasFirstCorner)
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(firstCorner, rawPoint));
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = hasFirstCorner
                ? host.ResolveFinalPoint(firstCorner, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitPoint(host, final);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, hasFirstCorner ? firstCorner : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitPoint(host, point);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "RECTANGLE ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "RECTANGLE canceled.");
        }

        private InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point)
        {
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!hasFirstCorner)
            {
                hasFirstCorner = true;
                firstCorner = point;
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = RubberStyle.Rectangle;
                rbo.SetStart(firstCorner);
                return InteractiveCommandResult.MoveToStep(OppositeCornerStep);
            }

            CreateRectangle(host, firstCorner, point);
            return Finish(host, "RECTANGLE created.");
        }

        private void CreateRectangle(IInteractiveCommandHost host, Point corner1, Point corner2)
        {
            var layer = activeLayerResolver?.Invoke();
            if (layer == null) return;

            var rectangle = new Rectangle(corner1, corner2);
            var document = host.ToolService.GetService<ICadDocumentService>();
            var cmd = new AddEntityCommand(document, layer.Id, rectangle);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            return EndCommand(host, message);
        }
    }
}
