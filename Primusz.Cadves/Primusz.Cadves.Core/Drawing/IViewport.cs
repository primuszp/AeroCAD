using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Layers;

namespace Primusz.Cadves.Core.Drawing
{
    public interface IViewport : IInputElement
    {
        Point Position { get; set; }

        Cursor Cursor { get; set; }

        double Zoom { get; set; }

        ScaleTransform Scale { get; set; }

        TranslateTransform Translate { get; set; }

        TransformGroup ViewTransform { get; }

        RubberObject GetRubberObject();

        IList<Layer> GetLayers();

        Point Project(Point point);

        Point Unproject(Point point);

        bool HitTest(Point point);

        bool HitTest(Rect rect);

        void InvalidateVisual();
    }
}