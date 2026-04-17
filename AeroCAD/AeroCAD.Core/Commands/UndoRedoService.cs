using System;
using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Commands
{
    public class UndoRedoService : IUndoRedoService
    {
        private readonly Stack<IUndoableCommand> undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> redoStack = new Stack<IUndoableCommand>();

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;
        public string UndoDescription => CanUndo ? undoStack.Peek().Description : null;
        public string RedoDescription => CanRedo ? redoStack.Peek().Description : null;

        public event EventHandler StateChanged;

        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PushCompleted(IUndoableCommand command)
        {
            undoStack.Push(command);
            redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = redoStack.Pop();
            cmd.Redo();
            undoStack.Push(cmd);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

