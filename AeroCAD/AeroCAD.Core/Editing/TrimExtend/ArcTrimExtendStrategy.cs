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

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Arc && boundaries.Any(IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Arc && boundaries.Any(IsSupportedBoundary);
        }

        public Entity CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var arc = target as Arc;
            if (arc == null)
                return null;

            var intersections = boundaries
                .Where(IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetCircularBoundaryIntersections(arc.Center, arc.Radius, boundary))
                .Where(item => CircularGeometry.IsAngleOnArc(item.Angle, arc.StartAngle, arc.SweepAngle))
                .Select(item => new { item.Angle, Parameter = CircularGeometry.GetArcParameter(arc.StartAngle, arc.SweepAngle, item.Angle) })
                .Where(item => item.Parameter > Epsilon && item.Parameter < 1d - Epsilon)
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return null;

            double clickParameter = CircularGeometry.GetArcParameter(
                arc.StartAngle,
                arc.SweepAngle,
                CircularGeometry.GetAngle(arc.Center, pickPoint));

            bool trimStart = clickParameter <= 0.5d;
            if (trimStart)
            {
                var replacement = intersections
                    .Where(item => item.Parameter >= clickParameter - Epsilon)
                    .OrderBy(item => item.Parameter)
                    .FirstOrDefault();
                if (replacement == null)
                    return null;

                double newSweep = System.Math.Sign(arc.SweepAngle) * (System.Math.Abs(arc.SweepAngle) * (1d - replacement.Parameter));
                return CreateArc(arc, replacement.Angle, newSweep);
            }

            var endReplacement = intersections
                .Where(item => item.Parameter <= clickParameter + Epsilon)
                .OrderByDescending(item => item.Parameter)
                .FirstOrDefault();
            if (endReplacement == null)
                return null;

            double trimmedSweep = System.Math.Sign(arc.SweepAngle) * (System.Math.Abs(arc.SweepAngle) * endReplacement.Parameter);
            return CreateArc(arc, arc.StartAngle, trimmedSweep);
        }

        public Entity CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var arc = target as Arc;
            if (arc == null)
                return null;

            var intersections = boundaries
                .Where(IsSupportedBoundary)
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
                return null;

            double clickParameter = CircularGeometry.GetArcParameter(
                arc.StartAngle,
                arc.SweepAngle,
                CircularGeometry.GetAngle(arc.Center, pickPoint));
            bool extendStart = clickParameter <= 0.5d;
            double currentSweep = System.Math.Abs(arc.SweepAngle);
            int directionSign = System.Math.Sign(arc.SweepAngle);

            if (extendStart)
            {
                var candidate = intersections
                    .Where(item => item.ReverseDistance > Epsilon)
                    .OrderBy(item => item.ReverseDistance)
                    .FirstOrDefault();
                if (candidate == null)
                    return null;

                double newSweep = arc.SweepAngle + (directionSign * candidate.ReverseDistance);
                return CreateArc(arc, candidate.Angle, newSweep);
            }

            var endCandidate = intersections
                .Where(item => item.ForwardDistance > currentSweep + Epsilon)
                .OrderBy(item => item.ForwardDistance)
                .FirstOrDefault();
            if (endCandidate == null)
                return null;

            double extension = endCandidate.ForwardDistance - currentSweep;
            return CreateArc(arc, arc.StartAngle, arc.SweepAngle + (directionSign * extension));
        }

        private static Arc CreateArc(Arc source, double startAngle, double sweepAngle)
        {
            if (System.Math.Abs(sweepAngle) <= Epsilon)
                return null;

            return new Arc(source.Center, source.Radius, startAngle, sweepAngle)
            {
                Thickness = source.Thickness
            };
        }

        private static bool IsSupportedBoundary(Entity boundary)
        {
            return boundary is Line || boundary is Circle || boundary is Polyline || boundary is Arc || boundary is Rectangle;
        }
    }
}
