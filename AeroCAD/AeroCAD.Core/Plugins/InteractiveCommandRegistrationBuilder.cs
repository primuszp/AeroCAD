using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveCommandRegistrationBuilder
    {
        private readonly string commandName;
        private CommandStep initialStep;
        private string toolName;
        private IEnumerable<string> aliases;
        private string description;
        private EditorCommandPolicy policy;
        private bool assignActiveLayer = true;
        private bool replaceExistingCommand;
        private string menuGroup;
        private string menuLabel;
        private Action<InteractiveCommandContext> onActivated;
        private Action<InteractiveCommandContext, System.Windows.Point> onPointerMove;
        private Func<InteractiveCommandContext, System.Windows.Point, InteractiveCommandResult> onViewportPoint;
        private Func<InteractiveCommandContext, CommandInputToken, InteractiveCommandResult> onToken;
        private Func<InteractiveCommandContext, InteractiveCommandResult> onComplete;
        private Func<InteractiveCommandContext, InteractiveCommandResult> onCancel;

        private InteractiveCommandRegistrationBuilder(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name is required.", nameof(commandName));

            this.commandName = commandName.Trim().ToUpperInvariant();
        }

        public static InteractiveCommandRegistrationBuilder Create(string commandName)
        {
            return new InteractiveCommandRegistrationBuilder(commandName);
        }

        public InteractiveCommandRegistrationBuilder WithInitialStep(CommandStep step)
        {
            initialStep = step;
            return this;
        }

        public InteractiveCommandRegistrationBuilder WithToolName(string name)
        {
            toolName = name;
            return this;
        }

        public InteractiveCommandRegistrationBuilder WithAliases(params string[] commandAliases)
        {
            aliases = commandAliases;
            return this;
        }

        public InteractiveCommandRegistrationBuilder WithDescription(string text)
        {
            description = text;
            return this;
        }

        public InteractiveCommandRegistrationBuilder WithPolicy(EditorCommandPolicy commandPolicy)
        {
            policy = commandPolicy;
            return this;
        }

        public InteractiveCommandRegistrationBuilder AssignActiveLayer(bool assign = true)
        {
            assignActiveLayer = assign;
            return this;
        }

        public InteractiveCommandRegistrationBuilder ReplaceExistingCommand(bool replace = true)
        {
            replaceExistingCommand = replace;
            return this;
        }

        public InteractiveCommandRegistrationBuilder InMenu(string group, string label)
        {
            menuGroup = group;
            menuLabel = label;
            return this;
        }

        public InteractiveCommandRegistrationBuilder OnActivated(Action<InteractiveCommandContext> callback)
        {
            onActivated = callback;
            return this;
        }

        public InteractiveCommandRegistrationBuilder OnPointerMove(Action<InteractiveCommandContext, System.Windows.Point> callback)
        {
            onPointerMove = callback;
            return this;
        }

        public InteractiveCommandRegistrationBuilder OnPoint(Func<InteractiveCommandContext, System.Windows.Point, InteractiveCommandResult> callback)
        {
            onViewportPoint = callback;
            return this;
        }

        public InteractiveCommandRegistrationBuilder PromptPoint(
            CommandStep step,
            Func<InteractiveCommandContext, System.Windows.Point, InteractiveCommandResult> callback,
            Func<InteractiveCommandContext, System.Windows.Point?> basePoint = null)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            initialStep = step ?? initialStep;
            onViewportPoint = (context, point) =>
            {
                context.LogInput(point);
                return callback(context, point);
            };
            onToken = (context, token) =>
            {
                if (!context.TryResolvePoint(token, basePoint?.Invoke(context), out var point))
                    return context.Unhandled();

                context.LogInput(point);
                return callback(context, point);
            };

            return this;
        }

        public InteractiveCommandRegistrationBuilder PromptDistance(
            CommandStep step,
            Func<InteractiveCommandContext, double, InteractiveCommandResult> callback,
            Func<InteractiveCommandContext, System.Windows.Point?> basePoint)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (basePoint == null)
                throw new ArgumentNullException(nameof(basePoint));

            initialStep = step ?? initialStep;
            onViewportPoint = (context, point) =>
            {
                var origin = basePoint(context);
                if (!origin.HasValue)
                    return context.Unhandled();

                var distance = context.ResolveDistance(origin.Value, point);
                context.LogInput(distance);
                return callback(context, distance);
            };
            onToken = (context, token) =>
            {
                if (!context.TryResolveDistance(token, basePoint(context), out var distance))
                    return context.Unhandled();

                context.LogInput(distance);
                return callback(context, distance);
            };

            return this;
        }

        public InteractiveCommandRegistrationBuilder CreateEntityOnPoint(Func<InteractiveCommandContext, System.Windows.Point, Entity> factory, string createdMessage = null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            onViewportPoint = (context, point) =>
            {
                var entity = factory(context, point);
                if (entity == null)
                    return context.Handled();

                context.LogInput(point);
                context.AddEntity(entity);
                return context.End(createdMessage ?? $"{commandName} created.");
            };

            onToken = (context, token) =>
            {
                System.Windows.Point point;
                if (!context.TryResolvePoint(token, null, out point))
                    return context.Unhandled();

                var entity = factory(context, point);
                if (entity == null)
                    return context.Handled();

                context.LogInput(point);
                context.AddEntity(entity);
                return context.End(createdMessage ?? $"{commandName} created.");
            };

            return this;
        }

        public InteractiveCommandRegistrationBuilder OnToken(Func<InteractiveCommandContext, CommandInputToken, InteractiveCommandResult> callback)
        {
            onToken = callback;
            return this;
        }

        public InteractiveCommandRegistrationBuilder OnComplete(Func<InteractiveCommandContext, InteractiveCommandResult> callback)
        {
            onComplete = callback;
            return this;
        }

        public InteractiveCommandRegistrationBuilder OnCancel(Func<InteractiveCommandContext, InteractiveCommandResult> callback)
        {
            onCancel = callback;
            return this;
        }

        public InteractiveCommandRegistration Build()
        {
            return new InteractiveCommandRegistration(
                commandName,
                CreateController,
                toolName,
                aliases,
                description,
                policy,
                assignActiveLayer,
                menuGroup,
                menuLabel,
                replaceExistingCommand);
        }

        private IInteractiveCommandController CreateController()
        {
            return new DelegateInteractiveCommandController(
                commandName,
                initialStep,
                onActivated,
                onPointerMove,
                onViewportPoint,
                onToken,
                onComplete,
                onCancel);
        }
    }
}
