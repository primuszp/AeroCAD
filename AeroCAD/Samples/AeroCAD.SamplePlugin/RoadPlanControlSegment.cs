using System.Windows;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanControlSegment
    {
        public RoadPlanControlSegment(Point start, Point end)
        {
            Start = start;
            End = end;
        }

        public Point Start { get; set; }

        public Point End { get; set; }
    }
}
