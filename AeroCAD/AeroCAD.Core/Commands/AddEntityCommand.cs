using System;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Commands
{
    public class AddEntityCommand : IUndoableCommand
    {
        private readonly ICadDocumentService document;
        private readonly Guid layerId;
        private readonly Entity entity;

        public AddEntityCommand(ICadDocumentService document, Guid layerId, Entity entity)
        {
            this.document = document;
            this.layerId = layerId;
            this.entity = entity;
            Description = "Add Entity";
        }

        public string Description { get; }

        public void Execute() => document.AddEntity(layerId, entity);
        public void Undo() => document.RemoveEntity(entity);
        public void Redo() => Execute();
    }
}

