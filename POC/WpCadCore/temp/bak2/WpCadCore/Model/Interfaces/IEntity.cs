using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpCadCore.Model
{
    interface IEntity
    {
        IList<IPoint> Point3dCollection { get; set; }
        BoundingBox Bounds { get; set; }
        IPoint InitialPoint { get; }
        IPoint FinalPoint { get; }
        String Name { get; }
        Guid Id { get; }
    }
}
