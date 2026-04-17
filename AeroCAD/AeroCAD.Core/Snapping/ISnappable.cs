using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Snapping
{
    public interface ISnappable
    {
        IEnumerable<ISnapDescriptor> GetSnapDescriptors();
    }
}

