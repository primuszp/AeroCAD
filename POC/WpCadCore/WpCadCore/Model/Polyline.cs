using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WpCadCore.Model
{
    class Polyline : EntityBase
    {
        public bool IsClosed { get; set; }

        public Polyline()
            : base("PolyLine")
        {
            this.Point3dCollection = new List<IPoint>();
        }

        public override void Render(DrawingContext dc)
        {
            if (Point3dCollection == null || Point3dCollection.Count < 2) return;

            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure((Point3d)InitialPoint, false, IsClosed);

                List<Point> points = ((List<IPoint>)(Point3dCollection)).ConvertAll<Point>(delegate(IPoint p) { return p as Point3d; });

                ctx.PolyLineTo(points, true, true);
            }

            dc.DrawGeometry(null, pen, geometry);

            base.Render(dc);
        }

        public override void PutGrips()
        {
            if (this.Children.Count > 0) return;

            for (int i = 0; i < Point3dCollection.Count; i++)
            {
                Grip grip = new Grip(i);
                this.Children.Add(grip);
                this.RenderGrip(grip);
            }
        }

        public override IList<Grip> GetGrips()
        {
            if (this.Children.Count == 0) return null;

            List<Grip> grips = new List<Grip>();

            foreach (Grip grip in this.Children)
            {
                grips.Add(grip);
            }
            return grips;
        }

        public override void RenderGrip(Grip grip)
        {
            grip.Render(scaleTransform);
        }

        /// <summary>
        /// Move handle to the point
        /// </summary>
        public override void MoveHandleTo(IPoint point, int handle)
        {
            this.ChechkHandle(ref handle);
            this.Point3dCollection[handle] = point;
        }

        /// <summary>
        /// Get handle point by number
        /// </summary>
        public override IPoint GetHandlePoint(int handle)
        {
            this.ChechkHandle(ref handle);
            return Point3dCollection[handle];
        }

        public override void CalcBounds()
        {
            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double zmin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;
            double zmax = double.MinValue;

            for (int i = 0; i < Point3dCollection.Count; i++)
            {
                if (Point3dCollection[i].X < xmin) xmin = Point3dCollection[i].X;
                if (Point3dCollection[i].X > xmax) xmax = Point3dCollection[i].X;
                if (Point3dCollection[i].Y < ymin) ymin = Point3dCollection[i].Y;
                if (Point3dCollection[i].Y > ymax) ymax = Point3dCollection[i].Y;
                if (Point3dCollection[i].Z < zmin) zmin = Point3dCollection[i].Z;
                if (Point3dCollection[i].Z > zmax) zmax = Point3dCollection[i].Z;
            }
            this.Bounds = new BoundingBox(xmin, ymin, xmax, ymax, zmin, zmax);
        }
    }
}
