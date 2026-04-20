using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Commands
{
    /// <summary>
    /// Removes one entity and adds multiple replacement entities (e.g. trim splitting a line into two).
    /// Undo restores the original entity and removes the replacements.
    /// </summary>
    public class SplitEntityCommand : IUndoableCommand
    {
        private readonly ICadDocumentService documentService;
        private readonly ISelectionManager selectionManager;
        private readonly Entity originalEntity;
        private readonly IReadOnlyList<Entity> replacements;
        private readonly Guid ownerLayerId;
        private readonly Action afterChange;

        public SplitEntityCommand(
            ICadDocumentService documentService,
            Entity originalEntity,
            IReadOnlyList<Entity> replacements,
            string description = "Split Entity",
            Action afterChange = null,
            ISelectionManager selectionManager = null)
        {
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            this.originalEntity = originalEntity ?? throw new ArgumentNullException(nameof(originalEntity));
            this.replacements = replacements ?? throw new ArgumentNullException(nameof(replacements));
            this.afterChange = afterChange;
            this.selectionManager = selectionManager;
            ownerLayerId = documentService.GetLayerForEntity(originalEntity)?.Id
                ?? throw new InvalidOperationException("Original entity is not attached to a layer.");
            Description = description;
        }

        public string Description { get; }

        public void Execute()
        {
            bool wasSelected = selectionManager?.IsSelected(originalEntity) ?? false;
            if (wasSelected)
                selectionManager.Deselect(originalEntity);

            documentService.RemoveEntity(originalEntity);

            foreach (var replacement in replacements)
                documentService.AddEntity(ownerLayerId, replacement);

            afterChange?.Invoke();
        }

        public void Undo()
        {
            foreach (var replacement in replacements)
            {
                if (selectionManager?.IsSelected(replacement) ?? false)
                    selectionManager.Deselect(replacement);
                documentService.RemoveEntity(replacement);
            }

            documentService.AddEntity(ownerLayerId, originalEntity);
            afterChange?.Invoke();
        }

        public void Redo()
        {
            Execute();
        }
    }
}
