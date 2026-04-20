using System.Windows;

using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapResult
    {
        public SnapResult(Point point, SnapType type, Point? sourcePoint = null, Entity sourceEntity = null, int? sourceGripIndex = null)
        {
            Point = point;
            Type = type;
            SourcePoint = sourcePoint;
            SourceEntity = sourceEntity;
            SourceGripIndex = sourceGripIndex;
        }

        public Point Point { get; }

        public SnapType Type { get; }

        public Point? SourcePoint { get; }

        public Entity SourceEntity { get; }

        public int? SourceGripIndex { get; }
    }
}

