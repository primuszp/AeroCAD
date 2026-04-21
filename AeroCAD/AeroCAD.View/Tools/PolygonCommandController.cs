using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.GeometryMath;
using Primusz.AeroCAD.Core.Plugins;
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
        private int sides;
        private bool hasSides;
        private bool useEdgeMode;
        private bool hasCenter;
        private bool hasCenterModeChoice;
        private bool useInscribed = true;
        private Point center;
        private bool hasFirstEdgePoint;
        private Point firstEdgePoint;

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
            sides = 0;
            hasSides = false;
            useEdgeMode = false;
            hasCenter = false;
            hasCenterModeChoice = false;
            useInscribed = true;
            hasFirstEdgePoint = false;
            firstEdgePoint = default(Point);
            center = default(Point);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (useEdgeMode)
            {
                if (hasFirstEdgePoint)
                {
                    var final = host.ResolveFinalPoint(firstEdgePoint, rawPoint);
                    rbo.CurrentStyle = RubberStyle.Line;
                    rbo.SetMove(final);
                    SetPreview(host, rbo, CreatePreviewPolygon(null, firstEdgePoint, final));
                }
            }
            else if (hasCenter && hasCenterModeChoice)
            {
                var final = host.ResolveFinalPoint(center, rawPoint);
                rbo.CurrentStyle = RubberStyle.Circle;
                rbo.SetMove(final);
                SetPreview(rbo, CreateCenterPreview(final));
            }
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            if (!useEdgeMode && host.CurrentStep == CenterModeStep)
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
                    useInscribed = true;
                    hasCenterModeChoice = true;
                    return InteractiveCommandResult.MoveToStep(RadiusStep);
                }

                if (modeText == "I" || modeText == "INSCRIBED")
                {
                    useInscribed = true;
                    hasCenterModeChoice = true;
                    return InteractiveCommandResult.MoveToStep(RadiusStep);
                }

                if (modeText == "C" || modeText == "CIRCUMSCRIBED")
                {
                    useInscribed = false;
                    hasCenterModeChoice = true;
                    return InteractiveCommandResult.MoveToStep(RadiusStep);
                }

                return InteractiveCommandResult.HandledOnly();
            }

            if (host.CurrentStep == PlacementStep)
            {
                var placementText = (token?.TextValue ?? token?.RawText ?? string.Empty).Trim().ToUpperInvariant();
                if (placementText == "E" || placementText == "EDGE")
                {
                    useEdgeMode = true;
                    hasCenter = false;
                    hasCenterModeChoice = false;
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
                if (keyword == EdgeKeyword && !hasCenter)
                {
                    useEdgeMode = true;
                    var rbo = host.ToolService.Viewport.GetRubberObject();
                    rbo.ClearPreview();
                    rbo.CurrentStyle = RubberStyle.Line;
                    return InteractiveCommandResult.MoveToStep(FirstEdgeStep);
                }
            }

            if (token?.ScalarValue != null && !hasSides)
                return SubmitSides(host, token.ScalarValue.Value);

            Point point;
            var basePoint = useEdgeMode
                ? (hasFirstEdgePoint ? firstEdgePoint : (Point?)null)
                : (hasCenter ? center : (Point?)null);

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

            sides = count;
            hasSides = true;
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(count.ToString());
            return InteractiveCommandResult.MoveToStep(PlacementStep);
        }

        private InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point)
        {
            if (!hasSides)
                return InteractiveCommandResult.HandledOnly();

            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (useEdgeMode)
            {
                if (!hasFirstEdgePoint)
                {
                    hasFirstEdgePoint = true;
                    firstEdgePoint = point;
                    host.ToolService.Viewport.GetRubberObject().SetStart(firstEdgePoint);
                    return InteractiveCommandResult.MoveToStep(SecondEdgeStep);
                }

                CreatePolygonFromEdge(host, firstEdgePoint, point);
                return Finish(host, "POLYGON created.");
            }

            if (!hasCenter)
            {
                hasCenter = true;
                center = point;
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.CurrentStyle = RubberStyle.Circle;
                rbo.SetStart(center);
                return InteractiveCommandResult.MoveToStep(CenterModeStep);
            }

            CreatePolygonFromCenter(host, center, point);
            return Finish(host, "POLYGON created.");
        }

        private void CreatePolygonFromCenter(IInteractiveCommandHost host, Point polygonCenter, Point radiusPoint)
        {
            var layer = activeLayerResolver?.Invoke();
            if (layer == null)
                return;

            double radius = (radiusPoint - polygonCenter).Length;
            if (radius <= 1e-9)
                return;

            double rotationOffset = CircularGeometry.GetAngle(polygonCenter, radiusPoint);
            if (!useInscribed)
                rotationOffset -= Math.PI / sides;

            var points = RegularPolygonGeometry.BuildClosedPolygon(polygonCenter, sides, radius, rotationOffset, useInscribed);
            AddPolygon(host, layer, points);
        }

        private void CreatePolygonFromEdge(IInteractiveCommandHost host, Point first, Point second)
        {
            var layer = activeLayerResolver?.Invoke();
            if (layer == null)
                return;

            Point centerPoint;
            double rotationOffset;
            var points = RegularPolygonGeometry.BuildSidePolygon(first, second, sides, out centerPoint, out rotationOffset);
            AddPolygon(host, layer, points);
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
            if (useEdgeMode)
                return host.ResolveFinalPoint(hasFirstEdgePoint ? firstEdgePoint : (Point?)null, rawPoint);

            return host.ResolveFinalPoint(hasCenter ? center : (Point?)null, rawPoint);
        }

        private Polyline CreatePreviewPolygon(Point? centerPoint, Point? edgeStart, Point cursorPoint)
        {
            if (useEdgeMode && edgeStart.HasValue)
            {
                Point c;
                double rotation;
                var points = RegularPolygonGeometry.BuildSidePolygon(edgeStart.Value, cursorPoint, sides, out c, out rotation);
                return points.Length >= 4 ? new Polyline(points) : null;
            }

            if (centerPoint.HasValue)
            {
                double previewRadius = (cursorPoint - centerPoint.Value).Length;
                if (previewRadius <= 1e-9)
                    return null;

                double rotationOffset = CircularGeometry.GetAngle(centerPoint.Value, cursorPoint);
                if (!useInscribed)
                    rotationOffset -= Math.PI / sides;

                var points = RegularPolygonGeometry.BuildClosedPolygon(centerPoint.Value, sides, previewRadius, rotationOffset, useInscribed);
                return points.Count >= 4 ? new Polyline(points) : null;
            }

            return null;
        }

        private GripPreview CreateCenterPreview(Point cursorPoint)
        {
            if (!hasCenter)
                return GripPreview.Empty;

            double previewRadius = (cursorPoint - center).Length;
            if (previewRadius <= 1e-9)
                return GripPreview.Empty;

            double rotationOffset = CircularGeometry.GetAngle(center, cursorPoint);
            if (!useInscribed)
                rotationOffset -= Math.PI / sides;

            var points = RegularPolygonGeometry.BuildClosedPolygon(center, sides, previewRadius, rotationOffset, useInscribed);
            if (points.Count < 4)
                return GripPreview.Empty;

            var polygonGeometry = BuildPolylineGeometry(points);
            var circleGeometry = new EllipseGeometry(center, previewRadius, previewRadius);

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(circleGeometry, Colors.Gray, 1.0d),
                GripPreviewStroke.CreateScreenConstant(polygonGeometry, Colors.White, 1.5d)
            });
        }

        private static Geometry BuildPolylineGeometry(IReadOnlyList<Point> points)
        {
            if (points == null || points.Count < 2)
                return Geometry.Empty;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(points[0], false, false);
                for (int i = 1; i < points.Count; i++)
                    context.LineTo(points[i], true, false);
            }

            if (geometry.CanFreeze)
                geometry.Freeze();

            return geometry;
        }
    }
}
