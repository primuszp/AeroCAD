using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class LineTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private const double Epsilon = 1e-9;

        public bool CanTrim(Entity boundary, Entity target)
        {
            return target is Line && IsSupportedBoundary(boundary);
        }

        public bool CanExtend(Entity boundary, Entity target)
        {
            return target is Line && IsSupportedBoundary(boundary);
        }

        public Entity CreateTrimmed(Entity boundary, Entity target, Point pickPoint)
        {
            var line = target as Line;
            if (line == null)
                return null;

            var intersections = TrimExtendGeometry.GetLineBoundaryIntersections(line, boundary)
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

            if (clickParameter > intersections[1].Parameter)
                return CreateLine(line.StartPoint, intersections[1].Point, line.Thickness);

            return null;
        }

        public Entity CreateExtended(Entity boundary, Entity target, Point pickPoint)
        {
            var line = target as Line;
            if (line == null)
                return null;

            var intersections = TrimExtendGeometry.GetLineBoundaryIntersections(line, boundary)
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
