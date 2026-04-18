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
    /// Base class for entity plugins. Subclasses override normalized capability members and the
    /// base class materializes a single descriptor consumed by the engine runtime.
    /// </summary>
    public abstract class EntityPluginBase : IEntityPlugin
    {
        private EntityPluginDescriptor descriptor;

        public EntityPluginDescriptor Descriptor => descriptor ??= BuildDescriptor();

        protected virtual string PluginName => GetType().Name;
        protected abstract IEntityRenderStrategy RenderStrategy { get; }
        protected abstract IEntityBoundsStrategy BoundsStrategy { get; }
        protected virtual IGripPreviewStrategy GripPreviewStrategy => null;
        protected virtual ISelectionMovePreviewStrategy SelectionMovePreviewStrategy => null;
        protected virtual ITransientEntityPreviewStrategy TransientEntityPreviewStrategy => null;
        protected virtual IEntityOffsetStrategy OffsetStrategy => null;
        protected virtual IEntityTrimExtendStrategy TrimExtendStrategy => null;
        protected virtual IEnumerable<ITool> CreateTools() => Enumerable.Empty<ITool>();
        protected virtual IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands() => Enumerable.Empty<InteractiveCommandRegistration>();
        protected virtual IEnumerable<EditorCommandDefinition> CreateCommands() => Enumerable.Empty<EditorCommandDefinition>();

        protected virtual EntityPluginDescriptor BuildDescriptor()
        {
            return new EntityPluginDescriptor(
                PluginName,
                RenderStrategy,
                BoundsStrategy,
                gripPreviewStrategy: GripPreviewStrategy,
                selectionMovePreviewStrategy: SelectionMovePreviewStrategy,
                transientEntityPreviewStrategy: TransientEntityPreviewStrategy,
                offsetStrategy: OffsetStrategy,
                trimExtendStrategy: TrimExtendStrategy,
                tools: CreateTools(),
                interactiveCommands: CreateInteractiveCommands(),
                commands: CreateCommands());
        }
    }
}
