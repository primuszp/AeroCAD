using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Rendering
{
    public interface IEntityRenderService
    {
        void Render(Entity entity, Layer layer, EntityVisual visual);

        void InvalidateLayerCache(Layer layer);
    }
}

