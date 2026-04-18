using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// Bundles all strategies and runtime registrations needed to integrate a new entity type.
    /// Implement this interface and call ModelSpace.RegisterPlugin() to add a new entity.
    /// </summary>
    public interface IEntityPlugin
    {
        // Rendering and spatial strategies (required)
        IEntityRenderStrategy RenderStrategy { get; }
        IEntityBoundsStrategy BoundsStrategy { get; }

        // Editing strategies (optional — return null to skip)
        IGripPreviewStrategy GripPreviewStrategy { get; }
        ISelectionMovePreviewStrategy SelectionMovePreviewStrategy { get; }
        ITransientEntityPreviewStrategy TransientEntityPreviewStrategy { get; }
        IEntityOffsetStrategy OffsetStrategy { get; }
        IEntityTrimExtendStrategy TrimExtendStrategy { get; }

        /// <summary>
        /// Returns tools to register with IToolService during bootstrapping.
        /// Override to provide the interactive drawing tool for this entity type.
        /// </summary>
        IEnumerable<ITool> CreateTools() => Enumerable.Empty<ITool>();

        /// <summary>
        /// Returns command definitions to register with IEditorCommandCatalog during bootstrapping.
        /// Override to expose this entity's draw command to the command line.
        /// </summary>
        IEnumerable<EditorCommandDefinition> CreateCommands() => Enumerable.Empty<EditorCommandDefinition>();
    }
}
