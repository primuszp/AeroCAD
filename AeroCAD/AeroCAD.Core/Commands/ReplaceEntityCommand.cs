using System;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Commands
{
    public class ReplaceEntityCommand : IUndoableCommand
    {
        private readonly ICadDocumentService documentService;
        private readonly ISelectionManager selectionManager;
        private readonly Entity originalEntity;
        private readonly Entity replacementEntity;
        private readonly Guid ownerLayerId;
        private readonly Action afterChange;

        public ReplaceEntityCommand(
            ICadDocumentService documentService,
            Entity originalEntity,
            Entity replacementEntity,
            string description = "Replace Entity",
            Action afterChange = null,
            ISelectionManager selectionManager = null)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            this.originalEntity = originalEntity ?? throw new ArgumentNullException(nameof(originalEntity));
            this.replacementEntity = replacementEntity ?? throw new ArgumentNullException(nameof(replacementEntity));
            this.afterChange = afterChange;
            this.selectionManager = selectionManager;
            ownerLayerId = documentService.GetLayerForEntity(originalEntity)?.Id
                ?? throw new InvalidOperationException("Original entity is not attached to a layer.");
            Description = description;
        }

        public string Description { get; }

        public void Execute()
        {
            Replace(originalEntity, replacementEntity);
        }

        public void Undo()
        {
            Replace(replacementEntity, originalEntity);
        }

        public void Redo()
        {
            Execute();
        }

        private void Replace(Entity remove, Entity add)
        {
            bool wasSelected = selectionManager?.IsSelected(remove) ?? false;
            if (wasSelected)
                selectionManager.Deselect(remove);

            documentService.RemoveEntity(remove);
            documentService.AddEntity(ownerLayerId, add);

            if (wasSelected)
                selectionManager?.Select(add);

            afterChange?.Invoke();
        }
    }
}
