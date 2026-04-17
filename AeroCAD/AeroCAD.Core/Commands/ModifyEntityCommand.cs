using System;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Commands
{
    public class ModifyEntityCommand : IUndoableCommand
    {
        private readonly Entity targetEntity;
        private readonly Entity stateBefore;
        private readonly Entity stateAfter;
        private readonly Action afterRestore;

        public ModifyEntityCommand(Entity targetEntity, Entity stateBefore, Entity stateAfter, string description = "Modify Entity", Action afterRestore = null)
        {
            this.targetEntity = targetEntity;
            this.stateBefore = stateBefore;
            this.stateAfter = stateAfter;
            this.afterRestore = afterRestore;
            Description = description;
        }

        public string Description { get; }

        public void Execute()
        {
            RestoreState(stateAfter);
        }

        public void Undo()
        {
            RestoreState(stateBefore);
        }

        public void Redo()
        {
            Execute();
        }

        private void RestoreState(Entity sourceState)
        {
            targetEntity.RestoreState(sourceState);
            afterRestore?.Invoke();
        }
    }
}

