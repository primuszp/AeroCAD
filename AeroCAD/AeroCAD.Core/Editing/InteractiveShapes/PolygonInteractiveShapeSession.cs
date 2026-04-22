using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.GeometryMath;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class PolygonInteractiveShapeSession
    {
        public int Sides { get; private set; }
        public bool HasSides { get; private set; }
        public bool UseEdgeMode { get; private set; }
        public bool HasCenter { get; private set; }
        public bool HasCenterModeChoice { get; private set; }
        public bool UseInscribed { get; private set; } = true;
        public Point Center { get; private set; }
        public bool HasFirstEdgePoint { get; private set; }
        public Point FirstEdgePoint { get; private set; }

        public void Reset()
        {
            Sides = 0;
            HasSides = false;
            UseEdgeMode = false;
            HasCenter = false;
            HasCenterModeChoice = false;
            UseInscribed = true;
            Center = default(Point);
            HasFirstEdgePoint = false;
            FirstEdgePoint = default(Point);
        }

        public bool TrySetSides(double value)
        {
            int count = (int)Math.Round(value);
            if (count < 3 || count > 1024)
                return false;

            Sides = count;
            HasSides = true;
            return true;
        }

        public void ChooseCenterMode(bool useInscribed)
        {
            UseInscribed = useInscribed;
            HasCenterModeChoice = true;
        }

        public void BeginEdgeMode()
        {
            UseEdgeMode = true;
            HasCenter = false;
            HasCenterModeChoice = false;
            HasFirstEdgePoint = false;
            FirstEdgePoint = default(Point);
        }

        public void SetCenter(Point center)
        {
            HasCenter = true;
            Center = center;
        }

        public void SetFirstEdgePoint(Point point)
        {
            HasFirstEdgePoint = true;
            FirstEdgePoint = point;
        }

        public Polyline BuildPreview(Point cursorPoint)
        {
            if (!HasSides)
                return null;

            if (UseEdgeMode)
            {
                if (!HasFirstEdgePoint)
                    return null;

                Point centerPoint;
                double rotation;
                var points = RegularPolygonGeometry.BuildSidePolygon(FirstEdgePoint, cursorPoint, Sides, out centerPoint, out rotation);
                return points.Length >= 4 ? new Polyline(points) : null;
            }

            if (!HasCenter || !HasCenterModeChoice)
                return null;

            double previewRadius = (cursorPoint - Center).Length;
            if (previewRadius <= 1e-9)
                return null;

            double rotationOffset = CircularGeometry.GetAngle(Center, cursorPoint);
            if (!UseInscribed)
                rotationOffset -= Math.PI / Sides;

            var polygonPoints = RegularPolygonGeometry.BuildClosedPolygon(Center, Sides, previewRadius, rotationOffset, UseInscribed);
            return polygonPoints.Count >= 4 ? new Polyline(polygonPoints) : null;
        }

        public GripPreview BuildCenterPreview(Point cursorPoint)
        {
            if (!HasSides || !HasCenter || !HasCenterModeChoice)
                return GripPreview.Empty;

            var polygonPreview = BuildPreview(cursorPoint);
            if (polygonPreview == null)
                return GripPreview.Empty;

            double previewRadius = (cursorPoint - Center).Length;
            var circleGeometry = new EllipseGeometry(Center, previewRadius, previewRadius);

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(Center, cursorPoint), Colors.Orange, 1.5d, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(circleGeometry, Colors.LightGray, 0.1d),
                GripPreviewStroke.CreateScreenConstant(Polyline.BuildGeometry(polygonPreview.Points), Colors.White, 1.5d)
            });
        }

        public bool TryBuildCenterPolygon(Point radiusPoint, out IReadOnlyList<Point> points)
        {
            points = null;
            if (!HasSides || !HasCenter)
                return false;

            double radius = (radiusPoint - Center).Length;
            if (radius <= 1e-9)
                return false;

            double rotationOffset = CircularGeometry.GetAngle(Center, radiusPoint);
            if (!UseInscribed)
                rotationOffset -= Math.PI / Sides;

            points = RegularPolygonGeometry.BuildClosedPolygon(Center, Sides, radius, rotationOffset, UseInscribed);
            return points != null && points.Count >= 4;
        }

        public bool TryBuildEdgePolygon(Point secondPoint, out IReadOnlyList<Point> points)
        {
            points = null;
            if (!HasSides || !HasFirstEdgePoint)
                return false;

            Point centerPoint;
            double rotationOffset;
            points = RegularPolygonGeometry.BuildSidePolygon(FirstEdgePoint, secondPoint, Sides, out centerPoint, out rotationOffset);
            return points != null && points.Count >= 4;
        }
    }
}
