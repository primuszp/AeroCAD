using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class PolygonInteractiveShapePreviewFactory : IInteractiveShapePreviewFactory
    {
        public GripPreview CreatePreview(IInteractiveShapePreviewContext context)
        {
            if (context == null || !context.Center.HasValue || context.Sides < 3)
                return GripPreview.Empty;

            double previewRadius = (context.Cursor - context.Center.Value).Length;
            if (previewRadius <= 1e-9)
                return GripPreview.Empty;

            double rotationOffset = CircularGeometry.GetAngle(context.Center.Value, context.Cursor);
            if (!context.UseInscribed)
                rotationOffset -= Math.PI / context.Sides;

            var points = RegularPolygonGeometry.BuildClosedPolygon(
                context.Center.Value,
                context.Sides,
                previewRadius,
                rotationOffset,
                context.UseInscribed);

            if (points.Count < 4)
                return GripPreview.Empty;

            var polygonGeometry = BuildPolylineGeometry(points);
            var circleGeometry = new EllipseGeometry(context.Center.Value, previewRadius, previewRadius);

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(context.Center.Value, context.Cursor), Colors.Orange, 1.5d, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(circleGeometry, Colors.LightGray, 0.5d),
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
