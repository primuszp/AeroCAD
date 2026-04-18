using System;
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
    public sealed class EntityPluginDescriptor
    {
        public EntityPluginDescriptor(
            string name,
            IEntityRenderStrategy renderStrategy,
            IEntityBoundsStrategy boundsStrategy,
            IGripPreviewStrategy gripPreviewStrategy = null,
            ISelectionMovePreviewStrategy selectionMovePreviewStrategy = null,
            ITransientEntityPreviewStrategy transientEntityPreviewStrategy = null,
            IEntityOffsetStrategy offsetStrategy = null,
            IEntityTrimExtendStrategy trimExtendStrategy = null,
            IEnumerable<ITool> tools = null,
            IEnumerable<InteractiveCommandRegistration> interactiveCommands = null,
            IEnumerable<EditorCommandDefinition> commands = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Plugin name is required.", nameof(name)) : name;
            RenderStrategy = renderStrategy ?? throw new ArgumentNullException(nameof(renderStrategy));
            BoundsStrategy = boundsStrategy ?? throw new ArgumentNullException(nameof(boundsStrategy));
            GripPreviewStrategy = gripPreviewStrategy;
            SelectionMovePreviewStrategy = selectionMovePreviewStrategy;
            TransientEntityPreviewStrategy = transientEntityPreviewStrategy;
            OffsetStrategy = offsetStrategy;
            TrimExtendStrategy = trimExtendStrategy;
            Tools = (tools ?? Enumerable.Empty<ITool>()).ToArray();
            InteractiveCommands = (interactiveCommands ?? Enumerable.Empty<InteractiveCommandRegistration>()).ToArray();
            Commands = (commands ?? Enumerable.Empty<EditorCommandDefinition>()).ToArray();
        }

        public string Name { get; }
        public IEntityRenderStrategy RenderStrategy { get; }
        public IEntityBoundsStrategy BoundsStrategy { get; }
        public IGripPreviewStrategy GripPreviewStrategy { get; }
        public ISelectionMovePreviewStrategy SelectionMovePreviewStrategy { get; }
        public ITransientEntityPreviewStrategy TransientEntityPreviewStrategy { get; }
        public IEntityOffsetStrategy OffsetStrategy { get; }
        public IEntityTrimExtendStrategy TrimExtendStrategy { get; }
        public IReadOnlyList<ITool> Tools { get; }
        public IReadOnlyList<InteractiveCommandRegistration> InteractiveCommands { get; }
        public IReadOnlyList<EditorCommandDefinition> Commands { get; }
    }
}
