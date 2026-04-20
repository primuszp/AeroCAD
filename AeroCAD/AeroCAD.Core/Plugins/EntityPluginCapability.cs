using System;

namespace Primusz.AeroCAD.Core.Plugins
{
    [Flags]
    public enum EntityPluginCapability
    {
        None = 0,
        Render = 1 << 0,
        Bounds = 1 << 1,
        GripPreview = 1 << 2,
        SelectionMovePreview = 1 << 3,
        TransientPreview = 1 << 4,
        Offset = 1 << 5,
        TrimExtend = 1 << 6,
        Tool = 1 << 7,
        InteractiveCommand = 1 << 8,
        Command = 1 << 9
    }
}
