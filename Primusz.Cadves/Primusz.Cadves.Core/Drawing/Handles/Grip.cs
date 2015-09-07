using System;
using System.Windows;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Entities;

namespace Primusz.Cadves.Core.Drawing.Handles
{
    public class Grip : DrawingVisual
    {
        #region Members

        private Brush brush;
        private Color color;

        #endregion

        #region Properties

        public Entity EntityParent { get; private set; }

        public int Index { get; private set; }

        public double Size { get; set; }

        public Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    brush = new SolidColorBrush(color);
                }
            }
        }

        #endregion

        #region Constructors

        public Grip(Entity parent, int index)
        {
            Size = 10;
            Index = index;
            Color = Colors.MediumBlue;
            EntityParent = parent;
        }

        #endregion

        #region Methods

        public void Draw(Func<Point, Point> project)
        {
            if (EntityParent != null)
            {
                Point point = project(EntityParent.GetGripPoint(Index));

                using (DrawingContext dc = RenderOpen())
                {
                    dc.DrawRectangle(brush, new Pen(Brushes.LightGray, 1.5), GetGripRect(point));
                }
            }
        }

        /// <summary>
        /// Test whether grip contains point
        /// </summary>
        public bool Contains(Point point, Func<Point, Point> project)
        {
            if (EntityParent != null)
            {
                Rect rect = GetGripRect(project(EntityParent.GetGripPoint(Index)));
                return rect.Contains(point);
            }
            return false;
        }

        private Rect GetGripRect(Point point)
        {
            return new Rect
            {
                X = point.X - Size / 2.0,
                Y = point.Y - Size / 2.0,
                Width = Size,
                Height = Size
            };
        }

        #endregion
    }
}
