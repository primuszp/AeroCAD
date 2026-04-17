using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Commands
{
    public class ModifyEntitiesCommand : IUndoableCommand
    {
        private readonly IReadOnlyList<EntityStateRecord> records;
        private readonly Action afterRestore;

        public ModifyEntitiesCommand(IEnumerable<EntityStateRecord> records, string description = "Modify Entities", Action afterRestore = null)
        {
            this.records = (records ?? Enumerable.Empty<EntityStateRecord>())
                .Where(record => record != null)
                .ToList()
                .AsReadOnly();
            this.afterRestore = afterRestore;
            Description = description;
        }

        public string Description { get; }

        public void Execute()
        {
            Restore(useAfterState: true);
        }

        public void Undo()
        {
            Restore(useAfterState: false);
        }

        public void Redo()
        {
            Execute();
        }

        private void Restore(bool useAfterState)
        {
            foreach (var record in records)
                record.Target.RestoreState(useAfterState ? record.After : record.Before);

            afterRestore?.Invoke();
        }

        public sealed class EntityStateRecord
        {
            public EntityStateRecord(Entity target, Entity before, Entity after)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                Before = before ?? throw new ArgumentNullException(nameof(before));
                After = after ?? throw new ArgumentNullException(nameof(after));
            }

            public Entity Target { get; }

            public Entity Before { get; }

            public Entity After { get; }
        }
    }
}

