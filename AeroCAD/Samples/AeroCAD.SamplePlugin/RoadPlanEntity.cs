using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanEntity : Entity
    {
        private readonly List<Point> controlVertices = new List<Point>();
        private readonly List<RoadPlanControlSegment> controlSegments = new List<RoadPlanControlSegment>();
        private readonly List<RoadPlanVertex> vertices = new List<RoadPlanVertex>();

        public RoadPlanEntity(IEnumerable<RoadPlanVertex> vertices)
        {
            if (vertices != null)
            {
                this.vertices.AddRange(vertices.Select(v => new RoadPlanVertex(v.Location, v.Radius, v.InTransition, v.OutTransition)));
                controlVertices.AddRange(this.vertices.Select(v => v.Location));
                RebuildDerivedGeometry();
            }
        }

        public IReadOnlyList<RoadPlanVertex> Vertices => vertices.AsReadOnly();

        public IReadOnlyList<RoadPlanControlSegment> ControlSegments => controlSegments.AsReadOnly();

        public override int GripCount => controlVertices.Count + controlSegments.Count;

        public override Point GetGripPoint(int index)
        {
            if (index < controlVertices.Count)
                return controlVertices[index];

            int segmentIndex = index - controlVertices.Count;
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return default;

            return RoadPlanGeometryBuilder.GetMidpoint(controlSegments[segmentIndex]);
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            if (index < controlVertices.Count)
                controlVertices[index] = newPosition;
            else
                MoveSegment(index - controlVertices.Count, newPosition);

            RebuildDerivedGeometry();
        }

        public override GripKind GetGripKind(int index) => index < controlVertices.Count ? GripKind.Endpoint : GripKind.Midpoint;

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            for (int i = 0; i < controlVertices.Count; i++)
            {
                int vertexIndex = i;
                yield return new GripDescriptor(this, vertexIndex, GripKind.Endpoint, () => controlVertices[vertexIndex]);
            }

            for (int i = 0; i < controlSegments.Count; i++)
            {
                int segmentIndex = i;
                int gripIndex = controlVertices.Count + segmentIndex;
                yield return new GripDescriptor(this, gripIndex, GripKind.Midpoint, () => RoadPlanGeometryBuilder.GetMidpoint(controlSegments[segmentIndex]));
            }
        }

        public override Geometry GetPreviewGeometry() => RoadPlanGeometryBuilder.BuildTangentGeometry(controlSegments);

        public override Entity Clone()
        {
            var clone = new RoadPlanEntity(vertices) { Thickness = Thickness };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate() => new RoadPlanEntity(vertices) { Thickness = Thickness };

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as RoadPlanEntity;
            if (source == null)
                return;

            vertices.Clear();
            vertices.AddRange(source.vertices.Select(v => new RoadPlanVertex(v.Location, v.Radius, v.InTransition, v.OutTransition)));
            controlVertices.Clear();
            controlVertices.AddRange(source.controlVertices);
            controlSegments.Clear();
            controlSegments.AddRange(source.controlSegments.Select(s => new RoadPlanControlSegment(s.Start, s.End)));
            RebuildDerivedGeometry();
            RestoreBaseFrom(source);
        }

        public override void Translate(Vector delta)
        {
            for (int i = 0; i < controlVertices.Count; i++)
                controlVertices[i] += delta;
            RebuildDerivedGeometry();
        }

        private void RebuildDerivedGeometry()
        {
            var sourceVertices = vertices.Count > 0
                ? vertices.Select(v => new RoadPlanVertex(v.Location, v.Radius, v.InTransition, v.OutTransition)).ToList()
                : null;

            BuildControlSegments();
            vertices.Clear();
            vertices.AddRange(RoadPlanGeometryBuilder.SolveVerticesFromSegments(controlSegments, sourceVertices));
            InvalidateGeometry();
        }

        private void MoveSegment(int segmentIndex, Point newMidpoint)
        {
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return;

            var segment = controlSegments[segmentIndex];
            Point currentMidpoint = RoadPlanGeometryBuilder.GetMidpoint(segment);
            Vector delta = newMidpoint - currentMidpoint;
            Point shiftedStart = segment.Start + delta;
            Point shiftedEnd = segment.End + delta;

            if (controlVertices.Count < 2)
                return;

            if (controlSegments.Count == 1)
            {
                controlVertices[0] = shiftedStart;
                controlVertices[1] = shiftedEnd;
                return;
            }

            if (segmentIndex == 0)
            {
                controlVertices[0] = shiftedStart;
                controlVertices[1] = TryIntersectWithNext(shiftedStart, shiftedEnd, 1, shiftedEnd);
                return;
            }

            if (segmentIndex == controlSegments.Count - 1)
            {
                controlVertices[controlVertices.Count - 1] = shiftedEnd;
                controlVertices[controlVertices.Count - 2] = TryIntersectWithPrevious(segmentIndex - 1, shiftedStart, shiftedEnd, shiftedStart);
                return;
            }

            controlVertices[segmentIndex] = TryIntersectWithPrevious(segmentIndex - 1, shiftedStart, shiftedEnd, shiftedStart);
            controlVertices[segmentIndex + 1] = TryIntersectWithNext(shiftedStart, shiftedEnd, segmentIndex + 1, shiftedEnd);
        }

        private Point TryIntersectWithPrevious(int previousSegmentIndex, Point shiftedStart, Point shiftedEnd, Point fallback)
        {
            var previous = controlSegments[previousSegmentIndex];
            return RoadPlanGeometryBuilder.TryIntersectSupportLines(previous.Start, previous.End, shiftedStart, shiftedEnd, out var intersection)
                ? intersection
                : fallback;
        }

        private Point TryIntersectWithNext(Point shiftedStart, Point shiftedEnd, int nextSegmentIndex, Point fallback)
        {
            var next = controlSegments[nextSegmentIndex];
            return RoadPlanGeometryBuilder.TryIntersectSupportLines(shiftedStart, shiftedEnd, next.Start, next.End, out var intersection)
                ? intersection
                : fallback;
        }

        private void BuildControlSegments()
        {
            controlSegments.Clear();
            controlSegments.AddRange(RoadPlanGeometryBuilder.BuildControlSegments(controlVertices));
        }
    }
}
