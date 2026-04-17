using System;
using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editor
{
    public class EditorCommandCatalog : IEditorCommandCatalog
    {
        private readonly Dictionary<string, EditorCommandDefinition> lookup = new Dictionary<string, EditorCommandDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EditorCommandDefinition> definitions = new Dictionary<string, EditorCommandDefinition>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<EditorCommandDefinition> Commands => definitions.Values.OrderBy(definition => definition.Name).ToList().AsReadOnly();

        public void Register(EditorCommandDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            definitions[definition.Name] = definition;

            foreach (var alias in definition.Aliases)
                lookup[alias] = definition;
        }

        public bool TryResolve(string input, out EditorCommandDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return lookup.TryGetValue(input.Trim().ToUpperInvariant(), out definition);
        }
    }
}

