using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.View.Tools
{
    public class DiamondCommandController : CommandControllerBase
    {
        private static readonly CommandStep CenterStep =
            new CommandStep("Center", "Specify center point:");

        private static readonly CommandStep RadiusStep =
            new CommandStep("Radius", "Specify a corner point:");

        private readonly Func<Layer> activeLayerResolver;
        private bool hasCenter;
        private Point center;

        public DiamondCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "DIAMOND";
        public override CommandStep InitialStep => CenterStep;
        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            hasCenter = false;
            center = default(Point);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (hasCenter)
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(center, rawPoint));
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = hasCenter
                ? host.ResolveFinalPoint(center, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitPoint(host, final);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, hasCenter ? center : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitPoint(host, point);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish("DIAMOND ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish("DIAMOND canceled.");
        }

        private InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point)
        {
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!hasCenter)
            {
                hasCenter = true;
                center = point;
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = RubberStyle.Line;
                rbo.SetStart(center);
                return InteractiveCommandResult.MoveToStep(RadiusStep);
            }

            CreateDiamond(host, center, point);
            return Finish("DIAMOND created.");
        }

        private void CreateDiamond(IInteractiveCommandHost host, Point diamondCenter, Point corner)
        {
            var layer = activeLayerResolver?.Invoke();
            if (layer == null)
                return;

            double dx = Math.Abs(corner.X - diamondCenter.X);
            double dy = Math.Abs(corner.Y - diamondCenter.Y);
            double halfSize = Math.Max(dx, dy);

            if (halfSize <= 0)
                return;

            var points = new[]
            {
                new Point(diamondCenter.X, diamondCenter.Y - halfSize),
                new Point(diamondCenter.X + halfSize, diamondCenter.Y),
                new Point(diamondCenter.X, diamondCenter.Y + halfSize),
                new Point(diamondCenter.X - halfSize, diamondCenter.Y),
                new Point(diamondCenter.X, diamondCenter.Y - halfSize)
            };

            var diamond = new Polyline(points);
            var document = host.ToolService.GetService<ICadDocumentService>();
            var cmd = new AddEntityCommand(document, layer.Id, diamond);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
        }

        private InteractiveCommandResult Finish(string message)
        {
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }
    }
}
