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
            return target is Line && boundaries.Any(IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Line && boundaries.Any(IsSupportedBoundary);
        }

        public Entity CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var line = target as Line;
            if (line == null)
                return null;

            var intersections = boundaries
                .Where(IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(line, boundary))
                .Where(item => item.Parameter > Epsilon && item.Parameter < 1d - Epsilon)
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return null;

            double clickParameter = ProjectParameter(line, pickPoint);

            if (intersections.Count == 1)
            {
                return clickParameter <= intersections[0].Parameter
                    ? CreateLine(intersections[0].Point, line.EndPoint, line.Thickness)
                    : CreateLine(line.StartPoint, intersections[0].Point, line.Thickness);
            }

            if (clickParameter < intersections[0].Parameter)
                return CreateLine(intersections[0].Point, line.EndPoint, line.Thickness);

            if (clickParameter > intersections[intersections.Count - 1].Parameter)
                return CreateLine(line.StartPoint, intersections[intersections.Count - 1].Point, line.Thickness);

            // Click is between intersections — trim out the segment containing the click
            var left = intersections.Last(item => item.Parameter <= clickParameter);
            var right = intersections.First(item => item.Parameter >= clickParameter);
            if (left == null || right == null || left == right)
                return null;

            // Return the longer remaining piece
            double leftLength = left.Parameter;
            double rightLength = 1d - right.Parameter;
            return leftLength >= rightLength
                ? CreateLine(line.StartPoint, left.Point, line.Thickness)
                : CreateLine(right.Point, line.EndPoint, line.Thickness);
        }

        public Entity CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var line = target as Line;
            if (line == null)
                return null;

            var intersections = boundaries
                .Where(IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(line, boundary))
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return null;

            double clickParameter = ProjectParameter(line, pickPoint);
            bool extendStart = clickParameter <= 0.5d;

            if (extendStart)
            {
                var candidate = intersections
                    .Where(item => item.Parameter < -Epsilon)
                    .OrderByDescending(item => item.Parameter)
                    .FirstOrDefault();

                return candidate == null
                    ? null
                    : CreateLine(candidate.Point, line.EndPoint, line.Thickness);
            }

            var endCandidate = intersections
                .Where(item => item.Parameter > 1d + Epsilon)
                .OrderBy(item => item.Parameter)
                .FirstOrDefault();

            return endCandidate == null
                ? null
                : CreateLine(line.StartPoint, endCandidate.Point, line.Thickness);
        }

        private static Line CreateLine(Point start, Point end, double thickness)
        {
            return new Line(start, end)
            {
                Thickness = thickness
            };
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

        private static bool IsSupportedBoundary(Entity boundary)
        {
            return boundary is Line || boundary is Circle || boundary is Polyline || boundary is Arc;
        }
    }
}
