using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.GeometryMath;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class ArcInteractiveShapeSession
    {
        public enum ArcPhase
        {
            WaitingForStart,
            WaitingForSecondPoint,
            WaitingForEnd
        }

        public ArcPhase Phase { get; private set; } = ArcPhase.WaitingForStart;
        public Point StartPoint { get; private set; }
        public Point SecondPoint { get; private set; }
        public Point EndPoint { get; private set; }

        public void Reset()
        {
            Phase = ArcPhase.WaitingForStart;
            StartPoint = default(Point);
            SecondPoint = default(Point);
            EndPoint = default(Point);
        }

        public void BeginStart(Point point)
        {
            StartPoint = point;
            Phase = ArcPhase.WaitingForSecondPoint;
        }

        public void BeginSecond(Point point)
        {
            SecondPoint = point;
            Phase = ArcPhase.WaitingForEnd;
        }

        public Arc BuildArc(Point endPoint)
        {
            if (Phase != ArcPhase.WaitingForEnd)
                return null;

            return ComputeArcFrom3Points(StartPoint, SecondPoint, endPoint);
        }

        public GripPreview BuildLinePreview(Point rawPoint)
        {
            if (Phase != ArcPhase.WaitingForSecondPoint)
                return GripPreview.Empty;

            var final = rawPoint;
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(StartPoint, final), Colors.Orange, 1.5d, DashStyles.Dash)
            });
        }

        public GripPreview BuildArcPreview(Point rawPoint)
        {
            if (Phase != ArcPhase.WaitingForEnd)
                return GripPreview.Empty;

            var arc = ComputeArcFrom3Points(StartPoint, SecondPoint, rawPoint);
            if (arc == null)
                return GripPreview.Empty;

            var arcGeometry = Arc.BuildGeometry(arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle);
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(StartPoint, rawPoint), Colors.Orange, 1.0d, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(arcGeometry, Colors.White, 1.5d)
            });
        }

        private static Arc ComputeArcFrom3Points(Point p1, Point pMid, Point p2)
        {
            double ax = p1.X;
            double ay = p1.Y;
            double bx = pMid.X;
            double by = pMid.Y;
            double cx = p2.X;
            double cy = p2.Y;

            double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (Math.Abs(d) < 1e-10)
                return null;

            double ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
            double uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

            var center = new Point(ux, uy);
            double radius = (center - p1).Length;

            double startAngle = CircularGeometry.GetAngle(center, p1);
            double midAngle = CircularGeometry.GetAngle(center, pMid);
            double endAngle = CircularGeometry.GetAngle(center, p2);

            double ccwStartToMid = CircularGeometry.GetDirectionalDistance(startAngle, midAngle, 1);
            double ccwStartToEnd = CircularGeometry.GetDirectionalDistance(startAngle, endAngle, 1);

            double sweep;
            if (ccwStartToMid <= ccwStartToEnd + 1e-10)
            {
                sweep = ccwStartToEnd;
                if (sweep < 1e-10)
                    sweep = CircularGeometry.TwoPi - 1e-9;
            }
            else
            {
                double cwStartToEnd = CircularGeometry.GetDirectionalDistance(startAngle, endAngle, -1);
                sweep = -cwStartToEnd;
                if (sweep > -1e-10)
                    sweep = -(CircularGeometry.TwoPi - 1e-9);
            }

            return new Arc(center, radius, startAngle, sweep);
        }
    }
}
