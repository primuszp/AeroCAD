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
            EntityPluginCapability capabilities = EntityPluginCapability.None,
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
            Capabilities = capabilities != EntityPluginCapability.None
                ? capabilities
                : InferCapabilities(renderStrategy, boundsStrategy, gripPreviewStrategy, selectionMovePreviewStrategy, transientEntityPreviewStrategy, offsetStrategy, trimExtendStrategy, tools, interactiveCommands, commands);
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
        public EntityPluginCapability Capabilities { get; }
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

        private static EntityPluginCapability InferCapabilities(
            IEntityRenderStrategy renderStrategy,
            IEntityBoundsStrategy boundsStrategy,
            IGripPreviewStrategy gripPreviewStrategy,
            ISelectionMovePreviewStrategy selectionMovePreviewStrategy,
            ITransientEntityPreviewStrategy transientEntityPreviewStrategy,
            IEntityOffsetStrategy offsetStrategy,
            IEntityTrimExtendStrategy trimExtendStrategy,
            IEnumerable<ITool> tools,
            IEnumerable<InteractiveCommandRegistration> interactiveCommands,
            IEnumerable<EditorCommandDefinition> commands)
        {
            var capabilities = EntityPluginCapability.None;
            if (renderStrategy != null) capabilities |= EntityPluginCapability.Render;
            if (boundsStrategy != null) capabilities |= EntityPluginCapability.Bounds;
            if (gripPreviewStrategy != null) capabilities |= EntityPluginCapability.GripPreview;
            if (selectionMovePreviewStrategy != null) capabilities |= EntityPluginCapability.SelectionMovePreview;
            if (transientEntityPreviewStrategy != null) capabilities |= EntityPluginCapability.TransientPreview;
            if (offsetStrategy != null) capabilities |= EntityPluginCapability.Offset;
            if (trimExtendStrategy != null) capabilities |= EntityPluginCapability.TrimExtend;
            if (tools != null) capabilities |= EntityPluginCapability.Tool;
            if (interactiveCommands != null) capabilities |= EntityPluginCapability.InteractiveCommand;
            if (commands != null) capabilities |= EntityPluginCapability.Command;
            return capabilities;
        }
    }
}
