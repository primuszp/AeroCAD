using System;
using System.Collections.Generic;
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
    public sealed class EntityPluginBuilder
    {
        private readonly string name;
        private IEntityRenderStrategy renderStrategy;
        private IEntityBoundsStrategy boundsStrategy;
        private IGripPreviewStrategy gripPreviewStrategy;
        private ISelectionMovePreviewStrategy selectionMovePreviewStrategy;
        private ITransientEntityPreviewStrategy transientEntityPreviewStrategy;
        private IEntityOffsetStrategy offsetStrategy;
        private IEntityTrimExtendStrategy trimExtendStrategy;
        private readonly List<ITool> tools = new List<ITool>();
        private readonly List<InteractiveCommandRegistration> interactiveCommands = new List<InteractiveCommandRegistration>();
        private readonly List<EditorCommandDefinition> commands = new List<EditorCommandDefinition>();

        private EntityPluginBuilder(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Plugin name is required.", nameof(name));

            this.name = name.Trim();
        }

        public static EntityPluginBuilder Create(string name)
        {
            return new EntityPluginBuilder(name);
        }

        public EntityPluginBuilder WithRenderStrategy(IEntityRenderStrategy strategy)
        {
            renderStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithBoundsStrategy(IEntityBoundsStrategy strategy)
        {
            boundsStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithGripPreviewStrategy(IGripPreviewStrategy strategy)
        {
            gripPreviewStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithSelectionMovePreviewStrategy(ISelectionMovePreviewStrategy strategy)
        {
            selectionMovePreviewStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithTransientEntityPreviewStrategy(ITransientEntityPreviewStrategy strategy)
        {
            transientEntityPreviewStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithOffsetStrategy(IEntityOffsetStrategy strategy)
        {
            offsetStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithTrimExtendStrategy(IEntityTrimExtendStrategy strategy)
        {
            trimExtendStrategy = strategy;
            return this;
        }

        public EntityPluginBuilder WithTool(ITool tool)
        {
            if (tool != null)
                tools.Add(tool);
            return this;
        }

        public EntityPluginBuilder WithInteractiveCommand(InteractiveCommandRegistration registration)
        {
            if (registration != null)
                interactiveCommands.Add(registration);
            return this;
        }

        public EntityPluginBuilder WithInteractiveCommand(Func<InteractiveCommandRegistrationBuilder, InteractiveCommandRegistrationBuilder> configure)
        {
            if (configure == null)
                return this;

            var registration = configure(InteractiveCommandRegistrationBuilder.Create(name))?.Build();
            return WithInteractiveCommand(registration);
        }

        public EntityPluginBuilder WithCommand(EditorCommandDefinition definition)
        {
            if (definition != null)
                commands.Add(definition);
            return this;
        }

        public EntityPluginDescriptor BuildDescriptor()
        {
            return new EntityPluginDescriptor(
                name,
                renderStrategy,
                boundsStrategy,
                gripPreviewStrategy: gripPreviewStrategy,
                selectionMovePreviewStrategy: selectionMovePreviewStrategy,
                transientEntityPreviewStrategy: transientEntityPreviewStrategy,
                offsetStrategy: offsetStrategy,
                trimExtendStrategy: trimExtendStrategy,
                tools: tools,
                interactiveCommands: interactiveCommands,
                commands: commands);
        }

        public IEntityPlugin BuildPlugin()
        {
            return new BuiltEntityPlugin(BuildDescriptor());
        }

        private sealed class BuiltEntityPlugin : IEntityPlugin
        {
            public BuiltEntityPlugin(EntityPluginDescriptor descriptor)
            {
                Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            }

            public EntityPluginDescriptor Descriptor { get; }
        }
    }
}
