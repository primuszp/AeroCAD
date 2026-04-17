using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface IEditorCommandCatalog
    {
        IReadOnlyCollection<EditorCommandDefinition> Commands { get; }

        void Register(EditorCommandDefinition definition);

        bool TryResolve(string input, out EditorCommandDefinition definition);
    }
}

