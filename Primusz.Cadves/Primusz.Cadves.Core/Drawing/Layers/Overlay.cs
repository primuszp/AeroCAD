using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using Primusz.Cadves.Core.Helpers;
using Primusz.Cadves.Core.Drawing.Handles;
using Primusz.Cadves.Core.Drawing.Entities;

namespace Primusz.Cadves.Core.Drawing.Layers
{
    class Overlay : VisualHost
    {
        #region Constructors

        public Overlay(Viewport viewport)
        {
            viewport.Children.Add(this);
            Panel.SetZIndex(this, 1001);
        }

        #endregion

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            IEnumerable<Grip> grips = Visuals.OfType<Grip>();

            foreach (Grip grip in grips)
            {
                HitTestResult result = grip.HitTest(e.GetPosition(this));

                if (result != null)
                {
                    grip.Select();
                }
            }
        }

        public void Update()
        {
            Viewport viewport = VisualTreeHelpers.FindAncestor<Viewport>(this);

            if (viewport != null)
            {
                Visuals.Clear();

                IEnumerable<Layer> layers = viewport.Children.OfType<Layer>();

                foreach (Layer layer in layers)
                {
                    foreach (Entity entity in layer.Entities)
                    {
                        if (entity.IsSelected)
                        {
                            Line line = entity as Line;

                            Grip grip1 = new Grip(line, 0);
                            Grip grip2 = new Grip(line, 1);

                            Visuals.Add(grip1);
                            Visuals.Add(grip2);
                        }
                    }
                }
            }
        }
    }
}
