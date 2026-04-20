using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class LineTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private const double Epsilon = 1e-9;

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Line && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Line && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var line = target as Line;
            if (line == null)
                return Array.Empty<Entity>();

            var intersections = boundaries
                .Where(TrimExtendSupport.IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(line, boundary))
                .Where(item => item.Parameter > Epsilon && item.Parameter < 1d - Epsilon)
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            double clickParameter = ProjectParameter(line, pickPoint);

            if (intersections.Count == 1)
            {
                return clickParameter <= intersections[0].Parameter
                    ? new[] { CreateLine(intersections[0].Point, line.EndPoint, line.Thickness) }
                    : new[] { CreateLine(line.StartPoint, intersections[0].Point, line.Thickness) };
            }

            if (clickParameter < intersections[0].Parameter)
                return new[] { CreateLine(intersections[0].Point, line.EndPoint, line.Thickness) };

            if (clickParameter > intersections[intersections.Count - 1].Parameter)
                return new[] { CreateLine(line.StartPoint, intersections[intersections.Count - 1].Point, line.Thickness) };

            // Click is between intersections — trim out the segment containing the click, return both remaining pieces
            var left = intersections.LastOrDefault(item => item.Parameter <= clickParameter);
            var right = intersections.FirstOrDefault(item => item.Parameter >= clickParameter);
            if (left == null || right == null || left == right)
                return Array.Empty<Entity>();

            var results = new List<Entity>(2);

            if ((left.Point - line.StartPoint).LengthSquared > Epsilon)
                results.Add(CreateLine(line.StartPoint, left.Point, line.Thickness));

            if ((line.EndPoint - right.Point).LengthSquared > Epsilon)
                results.Add(CreateLine(right.Point, line.EndPoint, line.Thickness));

            return results;
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var line = target as Line;
            if (line == null)
                return Array.Empty<Entity>();

            var intersections = boundaries
                .Where(TrimExtendSupport.IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(line, boundary, restrictTargetToSegment: false))
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            double clickParameter = ProjectParameter(line, pickPoint);
            bool extendStart = clickParameter <= 0.5d;

            if (extendStart)
            {
                var candidate = intersections
                    .Where(item => item.Parameter < -Epsilon)
                    .OrderByDescending(item => item.Parameter)
                    .FirstOrDefault();

                return candidate == null
                    ? Array.Empty<Entity>()
                    : new[] { CreateLine(candidate.Point, line.EndPoint, line.Thickness) };
            }

            var endCandidate = intersections
                .Where(item => item.Parameter > 1d + Epsilon)
                .OrderBy(item => item.Parameter)
                .FirstOrDefault();

            return endCandidate == null
                ? Array.Empty<Entity>()
                : new[] { CreateLine(line.StartPoint, endCandidate.Point, line.Thickness) };
        }

        private static Line CreateLine(Point start, Point end, double thickness)
        {
            return new Line(start, end) { Thickness = thickness };
        }

        private static double ProjectParameter(Line line, Point point)
        {
            Vector direction = line.EndPoint - line.StartPoint;
            double lengthSq = direction.X * direction.X + direction.Y * direction.Y;
            if (lengthSq <= Epsilon)
                return 0d;

            Vector toPoint = point - line.StartPoint;
            return ((toPoint.X * direction.X) + (toPoint.Y * direction.Y)) / lengthSq;
        }

    }
}
