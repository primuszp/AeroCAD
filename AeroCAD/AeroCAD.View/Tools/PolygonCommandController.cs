using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.View.Tools
{
    public class PolygonCommandController : CommandControllerBase
    {
        private static readonly CommandStep SidesStep = new CommandStep("Sides", "Enter number of sides [3-1024] <4>:");
        private static readonly CommandStep CenterModeStep = new CommandStep("CenterMode", "Enter an option [Inscribed in circle/Circumscribed about circle] <Inscribed in circle>:", keywords: new[] { InscribedKeyword, CircumscribedKeyword });
        private static readonly CommandStep PlacementStep = new CommandStep("Placement", "Specify center point or [Edge]:", keywords: new[] { EdgeKeyword });
        private static readonly CommandStep RadiusStep = new CommandStep("Radius", "Specify radius of circle:");
        private static readonly CommandStep FirstEdgeStep = new CommandStep("FirstEdge", "Specify first endpoint of edge:");
        private static readonly CommandStep SecondEdgeStep = new CommandStep("SecondEdge", "Specify second endpoint of edge:");

        private static readonly CommandKeywordOption EdgeKeyword = new CommandKeywordOption("EDGE", new[] { "E" }, "Create polygon by edge.");
        private static readonly CommandKeywordOption InscribedKeyword = new CommandKeywordOption("INSCRIBED", new[] { "I" }, "Vertices touch the circle.");
        private static readonly CommandKeywordOption CircumscribedKeyword = new CommandKeywordOption("CIRCUMSCRIBED", new[] { "C" }, "Sides touch the circle.");

        private readonly Func<Layer> activeLayerResolver;
        private readonly PolygonInteractiveShapeSession session = new PolygonInteractiveShapeSession();

        public PolygonCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "POLYGON";
        public override CommandStep InitialStep => SidesStep;
        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            ResetRubberObject(host);
            session.Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (session.UseEdgeMode)
            {
                if (session.HasFirstEdgePoint)
                {
                    var final = host.ResolveFinalPoint(session.FirstEdgePoint, rawPoint);
                    rbo.CurrentStyle = RubberStyle.Line;
                    rbo.SetMove(final);
                    SetPreview(host, rbo, session.BuildPreview(final));
                }
            }
            else if (session.HasCenter && session.HasCenterModeChoice)
            {
                var final = host.ResolveFinalPoint(session.Center, rawPoint);
                rbo.Cancel();
                SetPreview(rbo, session.BuildCenterPreview(final));
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            if (!session.UseEdgeMode && host.CurrentStep == CenterModeStep)
                return InteractiveCommandResult.HandledOnly();

            Point final = ResolvePoint(host, rawPoint);
            return SubmitPoint(host, final);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (host.CurrentStep == SidesStep)
            {
                if (token == null || token.IsEmpty)
                    return SubmitSides(host, 4d);
            }

            if (host.CurrentStep == CenterModeStep)
            {
                var modeText = (token?.TextValue ?? token?.RawText ?? string.Empty).Trim().ToUpperInvariant();

                if (token == null || token.IsEmpty || modeText.Length == 0)
                {
                    session.ChooseCenterMode(true);
                    return InteractiveCommandResult.MoveToStep(RadiusStep);
                }

                if (modeText == "I" || modeText == "INSCRIBED")
                {
                    session.ChooseCenterMode(true);
                    return InteractiveCommandResult.MoveToStep(RadiusStep);
                }

                if (modeText == "C" || modeText == "CIRCUMSCRIBED")
                {
                    session.ChooseCenterMode(false);
                    return InteractiveCommandResult.MoveToStep(RadiusStep);
                }

                return InteractiveCommandResult.HandledOnly();
            }

            if (host.CurrentStep == PlacementStep)
            {
                var placementText = (token?.TextValue ?? token?.RawText ?? string.Empty).Trim().ToUpperInvariant();
                if (placementText == "E" || placementText == "EDGE")
                {
                    session.BeginEdgeMode();
                    var rbo = host.ToolService.Viewport.GetRubberObject();
                    rbo.ClearPreview();
                    rbo.SnapPoint = null;
                    rbo.CurrentStyle = RubberStyle.Line;
                    return InteractiveCommandResult.MoveToStep(FirstEdgeStep);
                }
            }

            CommandKeywordOption keyword;
            if (TryResolveKeyword(host, token, out keyword))
            {
                if (keyword == EdgeKeyword && !session.HasCenter)
                {
                    session.BeginEdgeMode();
                    var rbo = host.ToolService.Viewport.GetRubberObject();
                    rbo.ClearPreview();
                    rbo.CurrentStyle = RubberStyle.Line;
                    return InteractiveCommandResult.MoveToStep(FirstEdgeStep);
                }
            }

            if (token?.ScalarValue != null && !session.HasSides)
                return SubmitSides(host, token.ScalarValue.Value);

            Point point;
            var basePoint = session.UseEdgeMode
                ? (session.HasFirstEdgePoint ? session.FirstEdgePoint : (Point?)null)
                : (session.HasCenter ? session.Center : (Point?)null);

            if (!host.TryResolvePointInput(token, basePoint, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitPoint(host, point);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "POLYGON ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "POLYGON canceled.");
        }

        private InteractiveCommandResult SubmitSides(IInteractiveCommandHost host, double value)
        {
            int count = (int)Math.Round(value);
            if (count < 3 || count > 1024)
                return InteractiveCommandResult.HandledOnly();

            if (!session.TrySetSides(value))
                return InteractiveCommandResult.HandledOnly();

            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(count.ToString());
            return InteractiveCommandResult.MoveToStep(PlacementStep);
        }

        private InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point)
        {
            if (!session.HasSides)
                return InteractiveCommandResult.HandledOnly();

            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (session.UseEdgeMode)
            {
                if (!session.HasFirstEdgePoint)
                {
                    session.BeginFirstEdgePoint(point);
                    host.ToolService.Viewport.GetRubberObject().SetStart(session.FirstEdgePoint);
                    return InteractiveCommandResult.MoveToStep(SecondEdgeStep);
                }

                return CreatePolygon(host, point);
            }

            if (!session.HasCenter)
            {
                session.BeginCenterMode(point);
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.Cancel();
                return InteractiveCommandResult.MoveToStep(CenterModeStep);
            }

            return CreatePolygon(host, point);
        }

        private InteractiveCommandResult CreatePolygon(IInteractiveCommandHost host, Point point)
        {
            var layer = activeLayerResolver?.Invoke();
            if (layer == null)
                return InteractiveCommandResult.HandledOnly();

            if (!session.TryBuildCurrentPolygon(point, out var points))
                return InteractiveCommandResult.HandledOnly();

            AddPolygon(host, layer, points);
            return Finish(host, "POLYGON created.");
        }

        private void AddPolygon(IInteractiveCommandHost host, Layer layer, IReadOnlyList<Point> points)
        {
            if (points == null || points.Count < 4)
                return;

            var polygon = new Polyline(points);
            var document = host.ToolService.GetService<ICadDocumentService>();
            var cmd = new AddEntityCommand(document, layer.Id, polygon);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
        }

        private void SetPreview(IInteractiveCommandHost host, Primusz.AeroCAD.Core.Drawing.Layers.RubberObject rubberObject, Polyline previewEntity)
        {
            if (rubberObject == null)
                return;

            if (previewEntity == null)
            {
                rubberObject.Preview = null;
                return;
            }

            var transientPreviewService = host.ToolService.GetService<Primusz.AeroCAD.Core.Editing.TransientPreviews.ITransientEntityPreviewService>();
            var color = activeLayerResolver?.Invoke()?.Color ?? System.Windows.Media.Colors.White;
            rubberObject.Preview = transientPreviewService?.CreatePreview(previewEntity, color);
        }

        private void SetPreview(Primusz.AeroCAD.Core.Drawing.Layers.RubberObject rubberObject, GripPreview preview)
        {
            if (rubberObject == null)
                return;

            rubberObject.Preview = preview ?? GripPreview.Empty;
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            return EndCommand(host, message);
        }

        private Point ResolvePoint(IInteractiveCommandHost host, Point rawPoint)
        {
            if (session.UseEdgeMode)
                return host.ResolveFinalPoint(session.HasFirstEdgePoint ? session.FirstEdgePoint : (Point?)null, rawPoint);

            return host.ResolveFinalPoint(session.HasCenter ? session.Center : (Point?)null, rawPoint);
        }
    }
}
