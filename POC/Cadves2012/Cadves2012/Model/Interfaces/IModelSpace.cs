using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cadves2012.Model.Geometry;
using OpenTK;

namespace Cadves2012.Model.Interfaces
{
    public interface IModelSpace
    {
        double Zoom { get; set; }
        IEnumerable<IDrawEntity> SelectedEntities { get; }

        bool IsSelected(IDrawEntity entity);
        void AddSelectedEntity(IDrawEntity entity);
        void RemoveSelectedEntity(IDrawEntity entity);
        IEnumerable<IDrawEntity> GetHitEntities(ICanvas canvas, BoundingBox selection, bool anyPoint);
        IEnumerable<IDrawEntity> GetHitEntities(ICanvas canvas, Vector3d point);
        void ClearSelectedEntities();
    }
}
