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

        public Point Location { get; set; }

        public double Radius { get; set; }

        public double InTransition { get; set; }

        public double OutTransition { get; set; }
    }
}
