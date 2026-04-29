using System.Windows;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanVertex
    {
        public RoadPlanVertex(Point location, double radius = 0d, double inTransition = 0d, double outTransition = 0d)
        {
            Location = location;
            Radius = radius;
            InTransition = inTransition;
            OutTransition = outTransition;
        }

        public Point Location { get; }

        public double Radius { get; }

        public double InTransition { get; }

        public double OutTransition { get; }
    }
}
