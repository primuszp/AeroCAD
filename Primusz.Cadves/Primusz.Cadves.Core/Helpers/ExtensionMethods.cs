using System;
using System.Windows;
using System.Windows.Threading;

namespace Primusz.Cadves.Core.Helpers
{
    public static class ExtensionMethods
    {
        private static readonly Action Method = delegate() { };

        public static void Refresh(this UIElement element)
        {
            element.Dispatcher.Invoke(DispatcherPriority.Render, Method);
        }
    }
}
