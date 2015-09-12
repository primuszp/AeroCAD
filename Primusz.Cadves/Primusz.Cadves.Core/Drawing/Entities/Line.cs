using System;
using System.Windows;
using System.Windows.Media;
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

        /// <summary>
        /// Get grip point by index
        /// </summary>
        public override Point GetGripPoint(int index)
        {
            return index == 0 ? StartPoint : EndPoint;
        }

        //public override void PutGrips()
        //{
        //    if (Children.Count > 0) return;

        //    Children.Add(new Grip(0));
        //    Children.Add(new Grip(1));
        //}

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