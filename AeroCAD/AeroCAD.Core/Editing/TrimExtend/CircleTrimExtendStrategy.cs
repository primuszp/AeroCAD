using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class CircleTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private const double Epsilon = 1e-9;

        public bool CanTrim(Entity boundary, Entity target)
        {
            return target is Circle && IsSupportedBoundary(boundary);
        }

        public bool CanExtend(Entity boundary, Entity target)
        {
            return false;
        }

        public Entity CreateTrimmed(Entity boundary, Entity target, Point pickPoint)
        {
            var circle = target as Circle;
            if (circle == null)
                return null;

            var intersections = TrimExtendGeometry.GetCircularBoundaryIntersections(circle.Center, circle.Radius, boundary)
                .OrderBy(item => item.Angle)
                .ToList();

            if (intersections.Count < 2)
                return null;

            double pickAngle = CircularGeometry.GetAngle(circle.Center, pickPoint);
            int nextIndex = intersections.FindIndex(item => item.Angle > pickAngle + Epsilon);
            if (nextIndex < 0)
                nextIndex = 0;

            int previousIndex = (nextIndex - 1 + intersections.Count) % intersections.Count;
            double previousAngle = intersections[previousIndex].Angle;
            double nextAngle = intersections[nextIndex].Angle;

            double candidateSweepContainingPick = CircularGeometry.GetDirectionalDistance(previousAngle, nextAngle, 1);
            double oppositeSweep = CircularGeometry.TwoPi - candidateSweepContainingPick;
            if (candidateSweepContainingPick <= Epsilon || oppositeSweep <= Epsilon)
                return null;

            var candidateContainingPick = new Arc(circle.Center, circle.Radius, previousAngle, candidateSweepContainingPick)
            {
                Thickness = circle.Thickness
            };

            bool pickIsOnCandidate = CircularGeometry.IsAngleOnArc(
                pickAngle,
                candidateContainingPick.StartAngle,
                candidateContainingPick.SweepAngle);

            if (!pickIsOnCandidate)
                return candidateContainingPick;

            return new Arc(circle.Center, circle.Radius, nextAngle, oppositeSweep)
            {
                Thickness = circle.Thickness
            };
        }

        public Entity CreateExtended(Entity boundary, Entity target, Point pickPoint)
        {
            return null;
        }

        private static bool IsSupportedBoundary(Entity boundary)
        {
            return boundary is Line || boundary is Circle || boundary is Polyline || boundary is Arc;
        }
    }
}
