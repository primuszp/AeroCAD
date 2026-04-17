using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Commands
{
    public class AddEntitiesCommand : IUndoableCommand
    {
        private readonly ICadDocumentService document;
        private readonly IReadOnlyList<AddedEntityRecord> records;

        public AddEntitiesCommand(ICadDocumentService document, IEnumerable<AddedEntityRecord> records, string description = "Add Entities")
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            this.records = records
                .Where(record => record != null && record.Entity != null && record.LayerId != Guid.Empty)
                .ToList()
                .AsReadOnly();

            Description = description;
        }

        public string Description { get; }

        public void Execute()
        {
            foreach (var record in records)
                document.AddEntity(record.LayerId, record.Entity);
        }

        public void Undo()
        {
            foreach (var record in records)
                document.RemoveEntity(record.Entity);
        }

        public void Redo()
        {
            Execute();
        }

        public sealed class AddedEntityRecord
        {
            public AddedEntityRecord(Guid layerId, Entity entity)
            {
                LayerId = layerId;
                Entity = entity;
            }

            public Guid LayerId { get; }

            public Entity Entity { get; }
        }
    }
}
