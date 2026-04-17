using System;
using System.Windows;

namespace Primusz.AeroCAD.Core.GeometryMath
{
    public static class CircularGeometry
    {
        public const double TwoPi = global::System.Math.PI * 2d;
        private const double Epsilon = 1e-9;

        public static double NormalizeAngle(double angle)
        {
            angle %= TwoPi;
            if (angle < 0d)
                angle += TwoPi;

            return angle;
        }

        public static Point GetPoint(Point center, double radius, double angle)
        {
            return new Point(
                center.X + (radius * global::System.Math.Cos(angle)),
                center.Y + (radius * global::System.Math.Sin(angle)));
        }

        public static double GetAngle(Point center, Point point)
        {
            return NormalizeAngle(global::System.Math.Atan2(point.Y - center.Y, point.X - center.X));
        }

        public static double GetDirectionalDistance(double startAngle, double endAngle, int directionSign)
        {
            startAngle = NormalizeAngle(startAngle);
            endAngle = NormalizeAngle(endAngle);

            if (directionSign >= 0)
            {
                double delta = endAngle - startAngle;
                return delta < 0d ? delta + TwoPi : delta;
            }

            double reverseDelta = startAngle - endAngle;
            return reverseDelta < 0d ? reverseDelta + TwoPi : reverseDelta;
        }

        public static double GetArcParameter(double startAngle, double sweepAngle, double angle)
        {
            double absSweep = global::System.Math.Abs(sweepAngle);
            if (absSweep <= Epsilon)
                return 0d;

            double distance = GetDirectionalDistance(startAngle, angle, sweepAngle >= 0d ? 1 : -1);
            return distance / absSweep;
        }

        public static bool IsAngleOnArc(double angle, double startAngle, double sweepAngle, double tolerance = 1e-9)
        {
            double absSweep = global::System.Math.Abs(sweepAngle);
            if (absSweep <= tolerance)
                return false;

            if (absSweep >= TwoPi - tolerance)
                return true;

            double parameter = GetArcParameter(startAngle, sweepAngle, angle);
            return parameter >= -tolerance && parameter <= 1d + tolerance;
        }

        public static double NormalizeSweep(double sweepAngle)
        {
            if (global::System.Math.Abs(sweepAngle) >= TwoPi)
            {
                sweepAngle %= TwoPi;
                if (global::System.Math.Abs(sweepAngle) <= Epsilon)
                    sweepAngle = (sweepAngle >= 0d ? 1d : -1d) * (TwoPi - Epsilon);
            }

            return sweepAngle;
        }
    }
}
