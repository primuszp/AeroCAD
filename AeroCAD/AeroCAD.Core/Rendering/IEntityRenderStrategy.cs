using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public interface IEntityRenderStrategy
    {
        bool CanHandle(Entity entity);

        void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context);
    }
}

