namespace Primusz.AeroCAD.Core.Editor
{
    /// <summary>
    /// Dispatches editor commands and manages the active command lifecycle.
    /// Implementations are responsible for resolving command names, validating
    /// selection policies, and activating the appropriate modal tool.
    /// </summary>
    public interface IEditorCommandRuntime
    {
        /// <summary>
        /// Executes the command with the given name or alias.
        /// Returns true if the command was recognised and handled.
        /// </summary>
        bool Execute(string commandName);

        /// <summary>
        /// Resolves <paramref name="input"/> through the command catalog and executes the result.
        /// Returns true if the command was recognised and handled.
        /// </summary>
        bool TryResolveAndExecute(string input);

        /// <summary>
        /// Sends a "complete" signal to the currently active interactive tool (equivalent to pressing Enter).
        /// Returns true if a tool was active and accepted the signal.
        /// </summary>
        bool CompleteActiveCommand();

        /// <summary>
        /// Cancels the currently active command or clears the selection,
        /// then returns the editor to selection mode.
        /// </summary>
        void CancelCurrentCommand();

        /// <summary>
        /// Deletes all currently selected entities via the undo stack.
        /// Returns true if at least one entity was deleted.
        /// </summary>
        bool DeleteSelectedEntities();
    }
}
