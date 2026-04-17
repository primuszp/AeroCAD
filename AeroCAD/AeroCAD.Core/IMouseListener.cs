using System.Windows.Input;

namespace Primusz.AeroCAD.Core
{
    /// <summary>
    /// Provides binding mouse events support for tools
    /// </summary>
    public interface IMouseListener
    {
        /// <summary>
        /// Handles the mouse-down event.
        /// </summary>
        /// <param name="e">Event data</param>
        void MouseButtonDown(MouseEventArgs e);

        /// <summary>
        /// Handles the mouse-move event.
        /// </summary>
        /// <param name="e">Event data</param>
        void MouseMove(MouseEventArgs e);

        /// <summary>
        /// Handles the mouse-up event.
        /// </summary>
        /// <param name="e">Event data</param>
        void MouseButtonUp(MouseEventArgs e);

        /// <summary>
        /// Handles the mouse-wheel event.
        /// </summary>
        /// <param name="e">Event data</param>
        void MouseWheel(MouseWheelEventArgs e);
    }
}
