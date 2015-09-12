using System.Windows.Input;

namespace Primusz.Cadves.Core
{
    /// <summary>
    /// Provides binding keyboard events support for tools
    /// </summary>
    public interface IKeyboardListener
    {
        /// <summary>
        /// Handles the keyboard down event.
        /// </summary>
        /// <param name="e">Event data</param>
        void KeyDown(KeyEventArgs e);

        /// <summary>
        /// Handles the keyboard up event.
        /// </summary>
        /// <param name="e">Event data</param>
        void KeyUp(KeyEventArgs e);
    }
}
