using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class CircleTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private const double Epsilon = 1e-9;
        private const double AngleDedupTolerance = 1e-6;

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Circle && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return false;
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var circle = target as Circle;
            if (circle == null)
                return Array.Empty<Entity>();

            var intersections = TrimExtendSupport.GetSupportedBoundaries(boundaries)
                .SelectMany(boundary => TrimExtendGeometry.GetCircularBoundaryIntersections(circle.Center, circle.Radius, boundary))
                .OrderBy(item => item.Angle)
                .GroupBy(item => Math.Round(item.Angle / AngleDedupTolerance))
                .Select(g => g.First())
                .OrderBy(item => item.Angle)
                .ToList();

            if (intersections.Count < 2)
                return Array.Empty<Entity>();

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
                return Array.Empty<Entity>();

            var candidateContainingPick = new Arc(circle.Center, circle.Radius, previousAngle, candidateSweepContainingPick)
            {
                Thickness = circle.Thickness
            };

            bool pickIsOnCandidate = CircularGeometry.IsAngleOnArc(
                pickAngle,
                candidateContainingPick.StartAngle,
                candidateContainingPick.SweepAngle);

            if (!pickIsOnCandidate)
                return new[] { (Entity)candidateContainingPick };

            return new[] { (Entity)new Arc(circle.Center, circle.Radius, nextAngle, oppositeSweep) { Thickness = circle.Thickness } };
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            return Array.Empty<Entity>();
        }

    }
}
