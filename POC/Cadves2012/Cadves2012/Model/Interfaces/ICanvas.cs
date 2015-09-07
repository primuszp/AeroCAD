using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cadves2012.Model.Interfaces
{
    interface ICanvas
    {
        IDrawEntity CurrentObject { get; }
        void Invalidate();
    }
}
