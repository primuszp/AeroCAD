using System;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class EntityVisual : DrawingVisual
    {
        public EntityVisual(Entity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public Entity Entity { get; }

        public void Redraw(Action<DrawingContext> draw)
        {
            using (DrawingContext context = RenderOpen())
            {
                draw?.Invoke(context);
            }
        }
    }
}

