using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class LineInteractiveShapeSession : IInteractiveShapeSession
    {
        private readonly List<Point> vertices = new List<Point>();
        private readonly List<Line> createdSegments = new List<Line>();

        public bool Drawing { get; private set; }
        public Point StartPoint { get; private set; }
        public Point FirstPoint { get; private set; }

        public IReadOnlyList<Point> Vertices => vertices;
        public IReadOnlyList<Line> CreatedSegments => createdSegments;

        public void Reset()
        {
            Drawing = false;
            StartPoint = default(Point);
            FirstPoint = default(Point);
            vertices.Clear();
            createdSegments.Clear();
        }

        public void Begin(Point point)
        {
            Drawing = true;
            StartPoint = point;
            FirstPoint = point;
            vertices.Add(point);
        }

        public void AddVertex(Point point)
        {
            vertices.Add(point);
            StartPoint = point;
        }

        public void AddSegment(Line line)
        {
            if (line != null)
                createdSegments.Add(line);
        }

        public bool CanClose()
        {
            return createdSegments.Count >= 2 && vertices.Count > 0;
        }

        public bool CanUndo()
        {
            return createdSegments.Count > 0 || vertices.Count > 0;
        }

        public bool UndoLast(out bool undoDocumentEntity)
        {
            undoDocumentEntity = false;

            if (vertices.Count == 0)
                return false;

            if (vertices.Count == 1)
            {
                vertices.Clear();
                createdSegments.Clear();
                Drawing = false;
                return true;
            }

            if (vertices.Count == 2)
            {
                undoDocumentEntity = true;
                createdSegments.Clear();
                vertices.RemoveAt(vertices.Count - 1);
                StartPoint = vertices[vertices.Count - 1];
                Drawing = true;
                return true;
            }

            vertices.RemoveAt(vertices.Count - 1);
            if (createdSegments.Count > 0)
                createdSegments.RemoveAt(createdSegments.Count - 1);
            StartPoint = vertices[vertices.Count - 1];
            Drawing = true;
            return true;
        }

        public void Close()
        {
            if (vertices.Count == 0)
                return;

            var firstPoint = vertices[0];
            var lastPoint = vertices[vertices.Count - 1];
            if (firstPoint != lastPoint)
            {
                vertices.Add(firstPoint);
                StartPoint = firstPoint;
            }
        }
    }
}
