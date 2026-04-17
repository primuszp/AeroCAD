using System.Windows;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public abstract class InteractiveCommandToolBase : BaseTool, ICommandInteractiveTool, IInteractiveCommandHost, IModalTool
    {
        private readonly CommandStepMachine stepMachine = new CommandStepMachine();
        private string activeCommandName;

        protected InteractiveCommandToolBase(string name)
            : base(name)
        {
        }

        protected CommandStep CurrentStep => stepMachine.CurrentStep;

        public override int InputPriority => 150;

        protected void BeginInteractiveSession(string commandName, CommandStep initialStep, EditorMode mode)
        {
            activeCommandName = commandName;
            stepMachine.MoveTo(initialStep);
            ToolService.GetService<IEditorStateService>()?.SetMode(mode);
            ToolService.GetService<ICommandFeedbackService>()?.BeginCommand(
                new CommandSession(commandName, initialStep?.Prompt ?? CommandPrompt.Default));
            UpdateCursorForStep(initialStep);
        }

        protected void MoveToStep(CommandStep step)
        {
            stepMachine.MoveTo(step);
            ToolService.GetService<ICommandFeedbackService>()?.SetPrompt(step?.Prompt ?? CommandPrompt.Default);
            UpdateCursorForStep(step);
        }

        protected void EndInteractiveSession(string closingMessage = null)
        {
            activeCommandName = null;
            stepMachine.Reset();
            ToolService.GetService<ICommandFeedbackService>()?.EndCommand(closingMessage);
        }

        public new static string FormatPoint(Point point)
        {
            return BaseTool.FormatPoint(point);
        }

        public virtual bool TrySubmitText(string input)
        {
            return TrySubmitToken(ToolService.GetService<ICommandFeedbackService>()?.ParseInput(input));
        }

        public abstract bool TrySubmitToken(CommandInputToken token);

        public abstract bool TrySubmitPoint(Point point);

        public abstract bool TryComplete();

        public abstract bool TryCancel();

        bool IInteractiveCommandHost.TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point)
        {
            return TryResolvePointInput(token, basePoint, out point);
        }

        bool IInteractiveCommandHost.TryResolveScalarInput(CommandInputToken token, out double scalar)
        {
            return TryResolveScalarInput(token, out scalar);
        }

        Point IInteractiveCommandHost.ResolveFinalPoint(Point? basePoint, Point rawPos)
        {
            return GetFinalPoint(basePoint, rawPos);
        }

        void IInteractiveCommandHost.MoveToStep(CommandStep step)
        {
            MoveToStep(step);
        }

        void IInteractiveCommandHost.EndSession(string closingMessage)
        {
            EndInteractiveSession(closingMessage);
        }

        void IInteractiveCommandHost.DeactivateTool()
        {
            Deactivate();
        }

        void IInteractiveCommandHost.ReturnToSelectionMode()
        {
            ReturnToSelectionMode();
        }

        bool IInteractiveCommandHost.ApplyResult(InteractiveCommandResult result)
        {
            return ApplyResult(result);
        }

        CommandStep IInteractiveCommandHost.CurrentStep => CurrentStep;

        protected bool ApplyResult(InteractiveCommandResult result)
        {
            if (result == null || !result.Handled)
                return false;

            if (result.NextStep != null)
                MoveToStep(result.NextStep);

            if (result.DeactivateTool)
                Deactivate();

            if (result.EndSession)
                EndInteractiveSession(result.ClosingMessage);

            if (result.ReturnToSelectionMode)
                ReturnToSelectionMode();

            return true;
        }

        private void UpdateCursorForStep(CommandStep step)
        {
            if (ToolService?.Viewport == null)
                return;

            ToolService.Viewport.ActiveCursorType = step != null && step.InputMode == CommandInputMode.Selection
                ? Drawing.CadCursorType.PickboxOnly
                : Drawing.CadCursorType.CrosshairOnly;

            ToolService.Viewport.GetRubberObject()?.InvalidateVisual();
            (ToolService.Viewport as UIElement)?.InvalidateVisual();
        }
    }
}

