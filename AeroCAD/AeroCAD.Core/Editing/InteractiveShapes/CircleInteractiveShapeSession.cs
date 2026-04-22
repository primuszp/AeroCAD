using System;
using System.Windows;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class CircleInteractiveShapeSession
    {
        public bool HasCenterPoint { get; private set; }
        public Point CenterPoint { get; private set; }
        public bool UseDiameterInput { get; private set; }

        public void Reset()
        {
            HasCenterPoint = false;
            CenterPoint = default(Point);
            UseDiameterInput = false;
        }

        public void BeginCenter(Point point)
        {
            HasCenterPoint = true;
            CenterPoint = point;
            UseDiameterInput = false;
        }

        public void BeginDiameterInput()
        {
            UseDiameterInput = true;
        }

        public double GetRadiusFromPoint(Point point)
        {
            return Math.Abs((point - CenterPoint).Length);
        }

        public double GetRadiusFromScalar(double scalar)
        {
            return Math.Abs(scalar);
        }

        public double GetDiameterFromPoint(Point point)
        {
            return Math.Abs((point - CenterPoint).Length);
        }

        public double GetDiameterFromScalar(double scalar)
        {
            return Math.Abs(scalar);
        }
    }
}
