using System;
using System.Windows;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Handles;
using Primusz.Cadves.Core.Drawing.Layers;

namespace Primusz.Cadves.Core.Drawing.Entities
{
    public class Line : Entity
    {
        public Point StartPoint { get; set; }

        public Point EndPoint { get; set; }

        public Layer Layer
        {
            get { return Parent as Layer; }
        }

        public Line(Point start, Point end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        public override GripList GetGrips()
        {
            GripList grips = new GripList
            {
                new Grip(this, 0), 
                new Grip(this, 1)
            };

            return grips;
        }

        /// <summary>
        /// Get grip point by index
        /// </summary>
        public override Point GetGripPoint(int index)
        {
            return index == 0 ? StartPoint : EndPoint;
        }

        public override void Render(Transform transform = null)
        {
            base.Render(transform);

            using (DrawingContext context = RenderOpen())
            {
                LineGeometry geometry = new LineGeometry(StartPoint, EndPoint);

                if (Layer != null)
                {
                    Pen.Brush = new SolidColorBrush(Layer.Color);
                    context.DrawGeometry(null, Pen, geometry);
                }
            }
        }
    }
}