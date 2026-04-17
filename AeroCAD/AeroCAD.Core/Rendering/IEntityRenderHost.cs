using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public interface IEntityRenderHost
    {
        void RenderEntity(Entity entity);
    }
}

