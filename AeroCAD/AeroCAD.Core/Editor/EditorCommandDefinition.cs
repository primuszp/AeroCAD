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
            bool assignActiveLayer = false,
            string menuGroup = null,
            string menuLabel = null,
            string modalToolName = null)
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
            ModalToolName = string.IsNullOrWhiteSpace(modalToolName) ? null : modalToolName.Trim();
            AssignActiveLayer = assignActiveLayer;
            MenuGroup = string.IsNullOrWhiteSpace(menuGroup) ? null : menuGroup.Trim();
            MenuLabel = string.IsNullOrWhiteSpace(menuLabel) ? description : menuLabel;
        }

        public string Name { get; }

        public IReadOnlyList<string> Aliases { get; }

        public string Description { get; }

        public EditorCommandPolicy Policy { get; }

        public Type ModalToolType { get; }

        public string ModalToolName { get; }

        public bool AssignActiveLayer { get; }

        /// <summary>
        /// The menu group this command belongs to (e.g. "Draw", "Modify", "Edit", "View").
        /// Null means the command is not shown in any menu.
        /// </summary>
        public string MenuGroup { get; }

        /// <summary>
        /// The display label for this command in menus, with WPF access-key prefix (e.g. "_Line").
        /// Falls back to Description when null.
        /// </summary>
        public string MenuLabel { get; }
    }
}
