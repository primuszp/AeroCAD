using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class PolylineInteractiveShapeSession
    {
        private readonly List<Point> points = new List<Point>();

        public IReadOnlyList<Point> Points => points;
        public Polyline CurrentPolyline { get; private set; }
        public bool HasStarted => points.Count > 0;

        public void Reset()
        {
            points.Clear();
            CurrentPolyline = null;
        }

        public void AddPoint(Point point)
        {
            points.Add(point);
        }

        public void BeginPolyline(Point startPoint)
        {
            points.Clear();
            points.Add(startPoint);
        }

        public void CreateCurrentPolyline()
        {
            if (points.Count >= 2)
                CurrentPolyline = new Polyline(points);
        }

        public void AppendToPolyline(Point point)
        {
            CurrentPolyline?.AddPoint(point);
        }

        public bool CanClose()
        {
            return points.Count >= 2;
        }

        public void Close()
        {
            if (!CanClose())
                return;

            var firstPoint = points[0];
            var lastPoint = points[points.Count - 1];
            if (firstPoint != lastPoint)
            {
                points.Add(firstPoint);
                CurrentPolyline?.AddPoint(firstPoint);
            }
        }

        public bool CanUndo()
        {
            return points.Count > 0;
        }

        public bool UndoLastPoint(out bool removeDocumentEntity)
        {
            removeDocumentEntity = false;
            if (points.Count == 0)
                return false;

            if (points.Count == 1)
            {
                points.Clear();
                CurrentPolyline = null;
                return true;
            }

            if (points.Count == 2)
            {
                removeDocumentEntity = true;
                CurrentPolyline = null;
                points.RemoveAt(points.Count - 1);
                return true;
            }

            points.RemoveAt(points.Count - 1);
            CurrentPolyline?.RemoveLastPoint();
            return true;
        }

        public Polyline BuildPreview(Point rawPoint)
        {
            if (points.Count == 0)
                return null;

            var previewPoints = new List<Point>(points) { rawPoint };
            return new Polyline(previewPoints);
        }
    }
}
