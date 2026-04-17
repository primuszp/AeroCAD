using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editor
{
    public sealed class EditorCommandDefinition
    {
        public EditorCommandDefinition(
            string name,
            IEnumerable<string> aliases = null,
            string description = null,
            EditorCommandPolicy policy = null,
            Type modalToolType = null,
            bool assignActiveLayer = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Command name is required.", nameof(name));

            Name = name.Trim().ToUpperInvariant();
            Aliases = (aliases ?? Enumerable.Empty<string>())
                .Append(Name)
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Select(alias => alias.Trim().ToUpperInvariant())
                .Distinct()
                .ToList()
                .AsReadOnly();
            Description = description ?? string.Empty;
            Policy = policy ?? EditorCommandPolicy.Default;
            ModalToolType = modalToolType;
            AssignActiveLayer = assignActiveLayer;
        }

        public string Name { get; }

        public IReadOnlyList<string> Aliases { get; }

        public string Description { get; }

        public EditorCommandPolicy Policy { get; }

        public Type ModalToolType { get; }

        public bool AssignActiveLayer { get; }
    }
}

