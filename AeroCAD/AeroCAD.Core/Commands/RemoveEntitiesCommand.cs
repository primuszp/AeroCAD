using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Commands
{
    public class RemoveEntitiesCommand : IUndoableCommand
    {
        private readonly ICadDocumentService document;
        private readonly List<RemovedEntityRecord> removedEntities;

        public RemoveEntitiesCommand(ICadDocumentService document, IEnumerable<Entity> entities, string description = "Delete Entities")
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            removedEntities = entities
                .Where(entity => entity != null)
                .Select(entity => new RemovedEntityRecord(entity, document.GetLayerForEntity(entity)?.Id ?? Guid.Empty))
                .Where(record => record.LayerId != Guid.Empty)
                .ToList();

            Description = description;
        }

        public string Description { get; }

        public void Execute()
        {
            foreach (var record in removedEntities)
                document.RemoveEntity(record.Entity);
        }

        public void Undo()
        {
            foreach (var record in removedEntities)
                document.AddEntity(record.LayerId, record.Entity);
        }

        public void Redo()
        {
            Execute();
        }

        private sealed class RemovedEntityRecord
        {
            public RemovedEntityRecord(Entity entity, Guid layerId)
            {
                Entity = entity;
                LayerId = layerId;
            }

            public Entity Entity { get; }

            public Guid LayerId { get; }
        }
    }
}

