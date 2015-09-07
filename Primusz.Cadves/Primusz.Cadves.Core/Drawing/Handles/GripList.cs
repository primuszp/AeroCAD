using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Primusz.Cadves.Core.Drawing.Handles
{
    public class GripList : List<Grip>
    {
        #region Public Properties

        public Handles.Grip GripItem { get; set; }

        #endregion

        #region Methodes

        public void SubscribeToMouseMoveEvent(Viewport viewport)
        {
            viewport.MouseMove += (s, e) =>
            {
                foreach (Grip grip in this)
                {
                    if (grip.Contains(e.GetPosition(viewport), viewport.Project))
                    {
                        if (grip.Color == Colors.Blue)
                            grip.Color = Colors.Red;
                        else
                        {
                            grip.Color = Colors.Blue;
                        }
                        grip.Draw(viewport.Project);
                    }
                }
            };
        }

        public void Draw(Func<Point, Point> project)
        {
            foreach (Grip grip in this)
            {
                grip.Draw(project);
            }
        }

        #endregion
    }
}
