using System;

namespace Primusz.AeroCAD.Core.Commands
{
    public interface IUndoRedoService
    {
        bool CanUndo { get; }
        bool CanRedo { get; }
        string UndoDescription { get; }
        string RedoDescription { get; }

        event EventHandler StateChanged;

        /// <summary>Executes the command and pushes it onto the undo stack.</summary>
        void Execute(IUndoableCommand command);

        /// <summary>
        /// Pushes an already-executed command onto the undo stack without calling Execute() again.
        /// Use this when the state change already happened (e.g. live grip drag).
        /// </summary>
        void PushCompleted(IUndoableCommand command);

        void Undo();
        void Redo();
        void Clear();
    }
}

