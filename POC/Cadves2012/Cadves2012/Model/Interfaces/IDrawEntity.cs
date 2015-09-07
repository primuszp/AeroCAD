using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Cadves2012.Model.Interfaces
{
    public interface IDrawEntity
    {
        string Id { get; }
        IDrawEntity Clone();

        void Move(Vector3d offset);
    }
}
