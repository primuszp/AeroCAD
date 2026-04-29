using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanEntity : CustomEntityBase
    {
        private const double IntersectionOverhang = 20.0d;

        internal enum ControlGripKind
        {
            SegmentStart,
            SegmentEnd,
            Intersection,
            SegmentOffset
        }

        internal readonly struct ControlGrip
        {
            public ControlGrip(ControlGripKind kind, int index)
            {
                Kind = kind;
                Index = index;
            }

            public ControlGripKind Kind { get; }

            public int Index { get; }
        }

        private readonly List<RoadPlanControlSegment> controlSegments = new List<RoadPlanControlSegment>();
        private readonly ReadOnlyCollection<RoadPlanControlSegment> readOnlyControlSegments;
        private readonly ReadOnlyCollection<RoadPlanVertex> readOnlyVertices;
        private readonly List<RoadPlanVertex> vertices = new List<RoadPlanVertex>();

        private RoadPlanEntity()
        {
            readOnlyControlSegments = controlSegments.AsReadOnly();
            readOnlyVertices = this.vertices.AsReadOnly();
        }

        public RoadPlanEntity(IEnumerable<RoadPlanVertex> vertices)
            : this()
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            var sourceVertices = vertices
                .Select(v => new RoadPlanVertex(v.Location, v.Radius, v.InTransition, v.OutTransition))
                .ToList();

            this.vertices.AddRange(sourceVertices);
            BuildIndependentControlSegments(sourceVertices);
            RebuildDerivedGeometry();
        }

        public IReadOnlyList<RoadPlanVertex> Vertices => readOnlyVertices;

        public IReadOnlyList<RoadPlanControlSegment> ControlSegments => readOnlyControlSegments;

        public override int GripCount => BuildGripMap().Count;

        public override Point GetGripPoint(int index)
        {
            var grips = BuildGripMap();
            if (index < 0 || index >= grips.Count)
                return default;

            return GetGripLocation(grips[index]);
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            var grips = BuildGripMap();
            if (index < 0 || index >= grips.Count)
                return;

            var grip = grips[index];
            switch (grip.Kind)
            {
                case ControlGripKind.SegmentStart:
                    MoveSegmentStart(grip.Index, newPosition);
                    RebuildDerivedGeometry();
                    break;
                case ControlGripKind.SegmentEnd:
                    MoveSegmentEnd(grip.Index, newPosition);
                    RebuildDerivedGeometry();
                    break;
                case ControlGripKind.Intersection:
                    MoveIntersection(grip.Index, newPosition);
                    RebuildDerivedGeometry();
                    break;
                case ControlGripKind.SegmentOffset:
                    MoveSegment(grip.Index, newPosition);
                    RebuildDerivedGeometry();
                    break;
            }
        }

        public override GripKind GetGripKind(int index)
        {
            var grips = BuildGripMap();
            if (index < 0 || index >= grips.Count)
                return GripKind.Endpoint;

            return grips[index].Kind switch
            {
                ControlGripKind.SegmentOffset => GripKind.Midpoint,
                _ => GripKind.Endpoint
            };
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            var grips = BuildGripMap();
            for (int i = 0; i < grips.Count; i++)
            {
                int gripIndex = i;
                GripKind gripKind = GetGripKind(gripIndex);
                yield return new GripDescriptor(this, gripIndex, gripKind, () => GetGripLocation(BuildGripMap()[gripIndex]));
            }
        }

        public override Geometry GetPreviewGeometry() => RoadPlanGeometryBuilder.BuildTangentGeometry(controlSegments);

        public GripPreview CreateGripPreview(int gripIndex, Point newPosition)
        {
            var grips = BuildGripMap();
            if (gripIndex < 0 || gripIndex >= grips.Count)
                return GripPreview.Empty;

            var helper = new LineGeometry(GetGripLocation(grips[gripIndex]), newPosition);
            if (helper.CanFreeze)
                helper.Freeze();

            Geometry controlGeometry = CreateLocalPreviewGeometry(grips[gripIndex], newPosition, out var previewSegments);
            Geometry axisGeometry = null;
            if (controlGeometry == null || previewSegments == null)
            {
                var previewEntity = Duplicate() as RoadPlanEntity;
                if (previewEntity == null)
                    return GripPreview.Empty;

                previewEntity.MoveGrip(gripIndex, newPosition);
                controlGeometry = GetEditedGeometry(previewEntity, grips[gripIndex]) ?? previewEntity.GetPreviewGeometry();
                axisGeometry = RoadPlanGeometryBuilder.BuildGeometry(previewEntity.Vertices);
            }
            else
            {
                var previewVertices = RoadPlanGeometryBuilder.SolveVerticesFromSegments(previewSegments, Vertices);
                axisGeometry = RoadPlanGeometryBuilder.BuildGeometry(previewVertices);
            }

            var strokes = new List<GripPreviewStroke>
            {
                GripPreviewStroke.CreateScreenConstant(helper, Colors.Orange, 1.5d, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(controlGeometry, Colors.White, 1.5d)
            };

            if (axisGeometry != null && !axisGeometry.IsEmpty())
                strokes.Add(GripPreviewStroke.CreateScreenConstant(axisGeometry, Colors.LightGray, 1.25d));

            return new GripPreview(strokes);
        }

        protected override CustomEntityBase CreateInstanceCore()
        {
            return new RoadPlanEntity();
        }

        protected override void CopyGeometryTo(CustomEntityBase target)
        {
            if (target is RoadPlanEntity roadPlan)
                roadPlan.ReplaceGeometryFrom(this);
        }

        protected override void CopyGeometryFrom(CustomEntityBase source)
        {
            if (source is RoadPlanEntity roadPlan)
                ReplaceGeometryFrom(roadPlan);
        }

        public override void Translate(Vector delta)
        {
            foreach (var segment in controlSegments)
            {
                segment.Start += delta;
                segment.End += delta;
            }

            RebuildDerivedGeometry();
        }

        internal Point GetGripLocation(ControlGrip grip)
        {
            return grip.Kind switch
            {
                ControlGripKind.SegmentStart => controlSegments[grip.Index].Start,
                ControlGripKind.SegmentEnd => controlSegments[grip.Index].End,
                ControlGripKind.Intersection => GetIntersectionGripLocation(grip.Index),
                ControlGripKind.SegmentOffset => RoadPlanGeometryBuilder.GetMidpoint(controlSegments[grip.Index]),
                _ => default
            };
        }

        private void MoveIntersection(int intersectionIndex, Point newPosition)
        {
            if (intersectionIndex < 0 || intersectionIndex >= controlSegments.Count - 1)
                return;

            var current = controlSegments[intersectionIndex];
            var next = controlSegments[intersectionIndex + 1];

            Vector currentDirection = newPosition - current.Start;
            Vector nextDirection = next.End - newPosition;
            if (currentDirection.LengthSquared < 1e-9 || nextDirection.LengthSquared < 1e-9)
                return;

            currentDirection.Normalize();
            nextDirection.Normalize();

            current.End = newPosition + currentDirection * IntersectionOverhang;
            next.Start = newPosition - nextDirection * IntersectionOverhang;
        }

        private void MoveSegment(int segmentIndex, Point newMidpoint)
        {
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return;

            var segment = controlSegments[segmentIndex];
            Point currentMidpoint = RoadPlanGeometryBuilder.GetMidpoint(segment);
            Vector delta = newMidpoint - currentMidpoint;
            segment.Start += delta;
            segment.End += delta;
        }

        private void MoveSegmentStart(int segmentIndex, Point newPosition)
        {
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return;

            controlSegments[segmentIndex].Start = newPosition;
        }

        private void MoveSegmentEnd(int segmentIndex, Point newPosition)
        {
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return;

            controlSegments[segmentIndex].End = newPosition;
        }

        private Point GetIntersectionGripLocation(int intersectionIndex)
        {
            if (intersectionIndex < 0 || intersectionIndex >= controlSegments.Count - 1)
                return default;

            var current = controlSegments[intersectionIndex];
            var next = controlSegments[intersectionIndex + 1];
            return RoadPlanGeometryBuilder.TryIntersectSupportLines(current.Start, current.End, next.Start, next.End, out var intersection)
                ? intersection
                : current.End;
        }

        private void BuildIndependentControlSegments(IReadOnlyList<RoadPlanVertex> sourceVertices)
        {
            if (sourceVertices == null || sourceVertices.Count < 2)
                return;

            for (int i = 0; i < sourceVertices.Count - 1; i++)
            {
                Point start = sourceVertices[i].Location;
                Point end = sourceVertices[i + 1].Location;

                if (i > 0)
                    start = RoadPlanGeometryBuilder.GetDirectionPoint(start, end, -IntersectionOverhang);

                if (i < sourceVertices.Count - 2)
                    end = RoadPlanGeometryBuilder.GetDirectionPoint(end, start, -IntersectionOverhang);

                controlSegments.Add(new RoadPlanControlSegment(start, end));
            }

            NormalizeIntersections();
        }

        private void RebuildDerivedGeometry(ControlGrip? preservedGrip = null)
        {
            var sourceVertices = vertices.Count > 0
                ? vertices.Select(v => new RoadPlanVertex(v.Location, v.Radius, v.InTransition, v.OutTransition)).ToList()
                : null;

            NormalizeIntersections(preservedGrip);
            vertices.Clear();
            vertices.AddRange(RoadPlanGeometryBuilder.SolveVerticesFromSegments(controlSegments, sourceVertices));
            InvalidateEntityGeometry();
        }

        private void ReplaceGeometryFrom(RoadPlanEntity source)
        {
            controlSegments.Clear();
            controlSegments.AddRange(source.controlSegments.Select(segment => new RoadPlanControlSegment(segment.Start, segment.End)));

            vertices.Clear();
            vertices.AddRange(source.vertices.Select(v => new RoadPlanVertex(v.Location, v.Radius, v.InTransition, v.OutTransition)));
        }

        private void NormalizeIntersections(ControlGrip? preservedGrip = null)
        {
            for (int i = 0; i < controlSegments.Count - 1; i++)
            {
                var current = controlSegments[i];
                var next = controlSegments[i + 1];
                if (!RoadPlanGeometryBuilder.TryIntersectSupportLines(current.Start, current.End, next.Start, next.End, out var intersection))
                    continue;

                Vector currentDirection = current.End - current.Start;
                Vector nextDirection = next.End - next.Start;
                if (currentDirection.LengthSquared < 1e-9 || nextDirection.LengthSquared < 1e-9)
                    continue;

                currentDirection.Normalize();
                nextDirection.Normalize();

                bool preserveCurrentEnd = preservedGrip.HasValue
                    && preservedGrip.Value.Kind == ControlGripKind.SegmentEnd
                    && preservedGrip.Value.Index == i;
                bool preserveNextStart = preservedGrip.HasValue
                    && preservedGrip.Value.Kind == ControlGripKind.SegmentStart
                    && preservedGrip.Value.Index == i + 1;

                if (!preserveCurrentEnd)
                    current.End = intersection + currentDirection * IntersectionOverhang;

                if (!preserveNextStart)
                    next.Start = intersection - nextDirection * IntersectionOverhang;

                if (preserveCurrentEnd)
                {
                    Point preservedIntersection = current.End - currentDirection * IntersectionOverhang;
                    next.Start = preservedIntersection - nextDirection * IntersectionOverhang;
                }

                if (preserveNextStart)
                {
                    Point preservedIntersection = next.Start + nextDirection * IntersectionOverhang;
                    current.End = preservedIntersection + currentDirection * IntersectionOverhang;
                }
            }
        }

        private static Geometry GetEditedGeometry(RoadPlanEntity entity, ControlGrip grip)
        {
            switch (grip.Kind)
            {
                case ControlGripKind.SegmentStart:
                    return GetNeighborSegmentGeometry(entity, grip.Index, includePrevious: true, includeNext: false);
                case ControlGripKind.SegmentEnd:
                    return GetNeighborSegmentGeometry(entity, grip.Index, includePrevious: false, includeNext: true);
                case ControlGripKind.SegmentOffset:
                    return CreateSegmentGeometry(entity.controlSegments[grip.Index]);
                case ControlGripKind.Intersection:
                    if (grip.Index < 0 || grip.Index >= entity.controlSegments.Count - 1)
                        return null;

                    var group = new GeometryGroup();
                    group.Children.Add(CreateSegmentGeometry(entity.controlSegments[grip.Index]));
                    group.Children.Add(CreateSegmentGeometry(entity.controlSegments[grip.Index + 1]));
                    if (group.CanFreeze)
                        group.Freeze();
                    return group;
                default:
                    return null;
            }
        }

        private Geometry CreateLocalPreviewGeometry(ControlGrip grip, Point newPosition, out IReadOnlyList<RoadPlanControlSegment> previewSegments)
        {
            previewSegments = null;
            Geometry geometry = grip.Kind switch
            {
                ControlGripKind.SegmentStart => CreateSegmentStartEndpointPreview(grip.Index, newPosition, out previewSegments),
                ControlGripKind.SegmentEnd => CreateSegmentEndEndpointPreview(grip.Index, newPosition, out previewSegments),
                ControlGripKind.Intersection => CreateIntersectionPreview(grip.Index, newPosition, out previewSegments),
                _ => null
            };
            return geometry;
        }

        private Geometry CreateSegmentStartEndpointPreview(int segmentIndex, Point newPosition, out IReadOnlyList<RoadPlanControlSegment> previewSegments)
        {
            previewSegments = null;
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return null;

            var segments = CloneControlSegments();
            segments[segmentIndex] = new RoadPlanControlSegment(newPosition, controlSegments[segmentIndex].End);
            previewSegments = segments;
            return CreateSegmentGeometry(segments[segmentIndex]);
        }

        private Geometry CreateSegmentEndEndpointPreview(int segmentIndex, Point newPosition, out IReadOnlyList<RoadPlanControlSegment> previewSegments)
        {
            previewSegments = null;
            if (segmentIndex < 0 || segmentIndex >= controlSegments.Count)
                return null;

            var segments = CloneControlSegments();
            segments[segmentIndex] = new RoadPlanControlSegment(controlSegments[segmentIndex].Start, newPosition);
            previewSegments = segments;
            return CreateSegmentGeometry(segments[segmentIndex]);
        }

        private Geometry CreateIntersectionPreview(int intersectionIndex, Point newPosition, out IReadOnlyList<RoadPlanControlSegment> previewSegments)
        {
            previewSegments = null;
            if (intersectionIndex < 0 || intersectionIndex >= controlSegments.Count - 1)
                return null;

            var segments = CloneControlSegments();
            var current = segments[intersectionIndex];
            var next = segments[intersectionIndex + 1];

            Vector currentDirection = newPosition - current.Start;
            Vector nextDirection = next.End - newPosition;
            if (currentDirection.LengthSquared < 1e-9 || nextDirection.LengthSquared < 1e-9)
                return null;

            currentDirection.Normalize();
            nextDirection.Normalize();

            current.End = newPosition + currentDirection * IntersectionOverhang;
            next.Start = newPosition - nextDirection * IntersectionOverhang;

            var group = new GeometryGroup();
            group.Children.Add(CreateSegmentGeometry(current));
            group.Children.Add(CreateSegmentGeometry(next));

            if (group.CanFreeze)
                group.Freeze();

            previewSegments = segments;
            return group;
        }

        private static Geometry GetNeighborSegmentGeometry(RoadPlanEntity entity, int segmentIndex, bool includePrevious, bool includeNext)
        {
            if (segmentIndex < 0 || segmentIndex >= entity.controlSegments.Count)
                return null;

            var group = new GeometryGroup();
            group.Children.Add(CreateSegmentGeometry(entity.controlSegments[segmentIndex]));

            if (includePrevious && segmentIndex > 0)
                group.Children.Add(CreateSegmentGeometry(entity.controlSegments[segmentIndex - 1]));

            if (includeNext && segmentIndex < entity.controlSegments.Count - 1)
                group.Children.Add(CreateSegmentGeometry(entity.controlSegments[segmentIndex + 1]));

            if (group.CanFreeze)
                group.Freeze();
            return group;
        }

        private static LineGeometry CreateSegmentGeometry(RoadPlanControlSegment segment)
        {
            var geometry = new LineGeometry(segment.Start, segment.End);
            if (geometry.CanFreeze)
                geometry.Freeze();
            return geometry;
        }

        private List<RoadPlanControlSegment> CloneControlSegments()
        {
            return controlSegments
                .Select(segment => new RoadPlanControlSegment(segment.Start, segment.End))
                .ToList();
        }

        private List<ControlGrip> BuildGripMap()
        {
            var grips = new List<ControlGrip>();

            for (int i = 0; i < controlSegments.Count; i++)
            {
                grips.Add(new ControlGrip(ControlGripKind.SegmentStart, i));
                grips.Add(new ControlGrip(ControlGripKind.SegmentEnd, i));
            }

            for (int i = 0; i < controlSegments.Count - 1; i++)
                grips.Add(new ControlGrip(ControlGripKind.Intersection, i));

            for (int i = 0; i < controlSegments.Count; i++)
                grips.Add(new ControlGrip(ControlGripKind.SegmentOffset, i));

            return grips;
        }
    }
}
