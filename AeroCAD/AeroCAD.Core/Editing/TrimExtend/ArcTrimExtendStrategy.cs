using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class ArcTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private const double Epsilon = 1e-9;
        private const double ParameterDedupTolerance = 1e-6;

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Arc && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Arc && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var arc = target as Arc;
            if (arc == null)
                return Array.Empty<Entity>();

            var intersections = TrimExtendSupport.GetSupportedBoundaries(boundaries)
                .SelectMany(boundary => TrimExtendGeometry.GetCircularBoundaryIntersections(arc.Center, arc.Radius, boundary))
                .Where(item => CircularGeometry.IsAngleOnArc(item.Angle, arc.StartAngle, arc.SweepAngle))
                .Select(item => new IntersectionPoint(item.Angle, CircularGeometry.GetArcParameter(arc.StartAngle, arc.SweepAngle, item.Angle)))
                .Where(item => item.Parameter > Epsilon && item.Parameter < 1d - Epsilon)
                .GroupBy(item => Math.Round(item.Parameter / ParameterDedupTolerance))
                .Select(g => g.First())
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            double clickParameter = CircularGeometry.GetArcParameter(
                arc.StartAngle,
                arc.SweepAngle,
                CircularGeometry.GetAngle(arc.Center, pickPoint));

            if (intersections.Count == 1)
            {
                var only = intersections[0];
                return clickParameter <= only.Parameter + Epsilon
                    ? ToResult(CreateTailArc(arc, only))
                    : ToResult(CreateHeadArc(arc, only));
            }

            if (clickParameter <= intersections[0].Parameter + Epsilon)
                return ToResult(CreateTailArc(arc, intersections[0]));

            if (clickParameter >= intersections[intersections.Count - 1].Parameter - Epsilon)
                return ToResult(CreateHeadArc(arc, intersections[intersections.Count - 1]));

            var left = intersections.LastOrDefault(item => item.Parameter < clickParameter - Epsilon);
            var right = intersections.FirstOrDefault(item => item.Parameter > clickParameter + Epsilon);
            if (left == null || right == null || ReferenceEquals(left, right))
                return Array.Empty<Entity>();

            var results = new List<Entity>(2);
            var head = CreateHeadArc(arc, left);
            if (head != null)
                results.Add(head);

            var tail = CreateTailArc(arc, right);
            if (tail != null)
                results.Add(tail);

            return results
                .OfType<Arc>()
                .OrderBy(item => item.StartAngle)
                .Cast<Entity>()
                .ToList();
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var arc = target as Arc;
            if (arc == null)
                return Array.Empty<Entity>();

            var intersections = TrimExtendSupport.GetSupportedBoundaries(boundaries)
                .SelectMany(boundary => TrimExtendGeometry.GetCircularBoundaryIntersections(arc.Center, arc.Radius, boundary))
                .Select(item => new
                {
                    item.Angle,
                    ForwardDistance = CircularGeometry.GetDirectionalDistance(
                        arc.StartAngle,
                        item.Angle,
                        arc.SweepAngle >= 0d ? 1 : -1),
                    ReverseDistance = CircularGeometry.GetDirectionalDistance(
                        arc.StartAngle,
                        item.Angle,
                        arc.SweepAngle >= 0d ? -1 : 1)
                })
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            double clickParameter = CircularGeometry.GetArcParameter(
                arc.StartAngle,
                arc.SweepAngle,
                CircularGeometry.GetAngle(arc.Center, pickPoint));
            bool extendStart = clickParameter <= 0.5d;
            double currentSweep = Math.Abs(arc.SweepAngle);
            int directionSign = Math.Sign(arc.SweepAngle);

            if (extendStart)
            {
                var candidate = intersections
                    .Where(item => item.ReverseDistance > Epsilon)
                    .OrderBy(item => item.ReverseDistance)
                    .FirstOrDefault();
                if (candidate == null)
                    return Array.Empty<Entity>();

                double newSweep = arc.SweepAngle + (directionSign * candidate.ReverseDistance);
                var result = CreateArc(arc, candidate.Angle, newSweep);
                return result == null ? Array.Empty<Entity>() : new[] { (Entity)result };
            }
            else
            {
                var endCandidate = intersections
                    .Where(item => item.ForwardDistance > currentSweep + Epsilon)
                    .OrderBy(item => item.ForwardDistance)
                    .FirstOrDefault();
                if (endCandidate == null)
                    return Array.Empty<Entity>();

                double extension = endCandidate.ForwardDistance - currentSweep;
                var result = CreateArc(arc, arc.StartAngle, arc.SweepAngle + (directionSign * extension));
                return result == null ? Array.Empty<Entity>() : new[] { (Entity)result };
            }
        }

        private static Arc CreateArc(Arc source, double startAngle, double sweepAngle)
        {
            if (Math.Abs(sweepAngle) <= Epsilon)
                return null;

            return new Arc(source.Center, source.Radius, startAngle, sweepAngle)
            {
                Thickness = source.Thickness
            };
        }

        private static Arc CreateHeadArc(Arc source, IntersectionPoint intersection)
        {
            double sweep = source.SweepAngle * intersection.Parameter;
            return CreateArc(source, source.StartAngle, sweep);
        }

        private static Arc CreateTailArc(Arc source, IntersectionPoint intersection)
        {
            double sweep = source.SweepAngle * (1d - intersection.Parameter);
            return CreateArc(source, intersection.Angle, sweep);
        }

        private static IReadOnlyList<Entity> ToResult(Arc arc)
        {
            return arc == null ? Array.Empty<Entity>() : new[] { (Entity)arc };
        }

        private sealed class IntersectionPoint
        {
            public IntersectionPoint(double angle, double parameter)
            {
                Angle = angle;
                Parameter = parameter;
            }

            public double Angle { get; }

            public double Parameter { get; }
        }

    }
}
