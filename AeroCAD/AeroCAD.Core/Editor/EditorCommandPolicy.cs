using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class EditorCommandPolicy
    {
        public static EditorCommandPolicy Default { get; } = new EditorCommandPolicy();

        public EditorCommandPolicy(
            CommandSelectionRequirement selectionRequirement = CommandSelectionRequirement.None,
            IEnumerable<Type> supportedSelectionEntityTypes = null,
            string selectionFailureMessage = null,
            string supportedTypesFailureMessage = null)
        {
            SelectionRequirement = selectionRequirement;
            SupportedSelectionEntityTypes = (supportedSelectionEntityTypes ?? Enumerable.Empty<Type>())
                .Where(type => type != null)
                .Distinct()
                .ToList()
                .AsReadOnly();
            SelectionFailureMessage = selectionFailureMessage ?? string.Empty;
            SupportedTypesFailureMessage = supportedTypesFailureMessage ?? string.Empty;
        }

        public CommandSelectionRequirement SelectionRequirement { get; }

        public IReadOnlyList<Type> SupportedSelectionEntityTypes { get; }

        public string SelectionFailureMessage { get; }

        public string SupportedTypesFailureMessage { get; }
    }
}
