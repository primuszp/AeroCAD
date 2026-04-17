using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Drawing
{
    public interface IViewport : IInputElement
    {
        Point Position { get; set; }

        CadCursorType ActiveCursorType { get; set; }

        Cursor Cursor { get; set; }

        double Zoom { get; set; }

        ScaleTransform Scale { get; set; }

        TranslateTransform Translate { get; set; }

        TransformGroup ViewTransform { get; }

        RubberObject GetRubberObject();

        IList<Layer> GetLayers();

        Point Project(Point point);

        Point Unproject(Point point);

        IList<Entity> QueryHitEntities(Point point);

        IList<Entity> QueryHitEntities(Point point, double toleranceWorld);

        IList<Entity> QueryHitEntities(Point point, IEnumerable<Entity> candidates);

        IList<Entity> QueryHitEntities(Point point, double toleranceWorld, IEnumerable<Entity> candidates);

        IList<Entity> QueryHitEntities(Rect rect, bool requireFullyInside = false);

        IList<Entity> QueryHitEntities(Rect rect, bool requireFullyInside, IEnumerable<Entity> candidates);

        void RefreshView();

        void InvalidateVisual();
    }
}

