using System.Windows;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class InteractiveShapePreviewContext : IInteractiveShapePreviewContext
    {
        public InteractiveShapePreviewContext(Point? center, Point cursor, int sides, bool useInscribed, bool useEdgeMode, Point? edgeStart)
        {
            Center = center;
            Cursor = cursor;
            Sides = sides;
            UseInscribed = useInscribed;
            UseEdgeMode = useEdgeMode;
            EdgeStart = edgeStart;
        }

        public Point? Center { get; }
        public Point Cursor { get; }
        public int Sides { get; }
        public bool UseInscribed { get; }
        public bool UseEdgeMode { get; }
        public Point? EdgeStart { get; }
    }
}
