using System;
using System.Runtime.InteropServices;

namespace Primusz.Cadves.Core.Helpers
{
    static class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);
    }
}
