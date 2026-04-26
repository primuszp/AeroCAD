using System;
using System.Windows;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class DelegateInteractiveCommandController : CommandControllerBase
    {
        private readonly Action<InteractiveCommandContext> onActivated;
        private readonly Action<InteractiveCommandContext, Point> onPointerMove;
        private readonly Func<InteractiveCommandContext, Point, InteractiveCommandResult> onViewportPoint;
        private readonly Func<InteractiveCommandContext, CommandInputToken, InteractiveCommandResult> onToken;
        private readonly Func<InteractiveCommandContext, InteractiveCommandResult> onComplete;
        private readonly Func<InteractiveCommandContext, InteractiveCommandResult> onCancel;

        public DelegateInteractiveCommandController(
            string commandName,
            CommandStep initialStep,
            Action<InteractiveCommandContext> onActivated = null,
            Action<InteractiveCommandContext, Point> onPointerMove = null,
            Func<InteractiveCommandContext, Point, InteractiveCommandResult> onViewportPoint = null,
            Func<InteractiveCommandContext, CommandInputToken, InteractiveCommandResult> onToken = null,
            Func<InteractiveCommandContext, InteractiveCommandResult> onComplete = null,
            Func<InteractiveCommandContext, InteractiveCommandResult> onCancel = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name is required.", nameof(commandName));

            CommandName = commandName.Trim().ToUpperInvariant();
            InitialStep = initialStep ?? new CommandStep("Input", "Specify point:");
            this.onActivated = onActivated;
            this.onPointerMove = onPointerMove;
            this.onViewportPoint = onViewportPoint;
            this.onToken = onToken;
            this.onComplete = onComplete;
            this.onCancel = onCancel;
        }

        public override string CommandName { get; }

        public override CommandStep InitialStep { get; }

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            onActivated?.Invoke(new InteractiveCommandContext(host));
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);
            onPointerMove?.Invoke(new InteractiveCommandContext(host), rawPoint);
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            if (onViewportPoint == null)
                return InteractiveCommandResult.Unhandled();

            var context = new InteractiveCommandContext(host);
            return onViewportPoint(context, context.ResolveFinalPoint(null, rawPoint)) ?? InteractiveCommandResult.HandledOnly();
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (onToken == null)
                return InteractiveCommandResult.Unhandled();

            return onToken(new InteractiveCommandContext(host), token) ?? InteractiveCommandResult.HandledOnly();
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return onComplete?.Invoke(new InteractiveCommandContext(host))
                ?? InteractiveCommandResult.End($"{CommandName} ended.", deactivateTool: true, returnToSelectionMode: true);
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return onCancel?.Invoke(new InteractiveCommandContext(host))
                ?? InteractiveCommandResult.End($"{CommandName} canceled.", deactivateTool: true, returnToSelectionMode: true);
        }
    }
}
