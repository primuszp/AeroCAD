using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Snapping;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Tools
{
    /// <summary>
    /// Abstract base class for interactive command controllers.
    /// Provides shared snap update and entity enumeration helpers,
    /// eliminating code duplication across all concrete controllers.
    /// </summary>
    public abstract class CommandControllerBase : IInteractiveCommandController
    {
        public abstract string CommandName { get; }

        public abstract CommandStep InitialStep { get; }

        public abstract EditorMode EditorMode { get; }

        public abstract void OnActivated(IInteractiveCommandHost host);

        public abstract void OnPointerMove(IInteractiveCommandHost host, Point rawPoint);

        public abstract InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint);

        public abstract InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token);

        public virtual InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host)
        {
            return InteractiveCommandResult.Unhandled();
        }

        public abstract InteractiveCommandResult TryComplete(IInteractiveCommandHost host);

        public abstract InteractiveCommandResult TryCancel(IInteractiveCommandHost host);

        /// <summary>
        /// Resolves a keyword token against the host's current step.
        /// </summary>
        protected bool TryResolveKeyword(IInteractiveCommandHost host, CommandInputToken token, out CommandKeywordOption keyword)
        {
            keyword = null;
            return host?.CurrentStep != null && host.CurrentStep.TryResolveKeyword(token, out keyword);
        }

        /// <summary>
        /// Clears the viewport rubber object and snaps after a command completes or is canceled.
        /// </summary>
        protected void ResetRubberObject(IInteractiveCommandHost host)
        {
            var rubberObject = host?.ToolService?.Viewport?.GetRubberObject();
            if (rubberObject == null)
                return;

            rubberObject.SnapPoint = null;
            rubberObject.ClearPreview();
            rubberObject.Cancel();
            rubberObject.InvalidateVisual();
        }

        /// <summary>
        /// Ends an interactive command using the standard cleanup path.
        /// </summary>
        protected InteractiveCommandResult EndCommand(IInteractiveCommandHost host, string message, bool returnToSelectionMode = true)
        {
            ResetRubberObject(host);
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: returnToSelectionMode);
        }

        /// <summary>
        /// Updates the snap engine with nearby entities and refreshes the rubber object's snap indicator.
        /// </summary>
        protected void UpdateSnap(IInteractiveCommandHost host, Point rawPoint)
        {
            if (host?.CurrentStep?.InputMode == CommandInputMode.Selection)
            {
                host.ToolService?.Viewport?.GetRubberObject().SnapPoint = null;
                return;
            }

            var snapEngine = host.ToolService.GetService<ISnapEngine>();
            if (snapEngine == null)
                return;

            var spatial = host.ToolService.GetService<ISpatialQueryService>();
            var candidates = GetSnapCandidates(host, rawPoint, snapEngine.ToleranceWorld, spatial);
            var descriptorService = host.ToolService.GetService<ISnapDescriptorService>();
            var descriptors = descriptorService?.GetEntityAndSelectedGripDescriptors(candidates)
                ?? GetAllEntities(host).OfType<ISnappable>().SelectMany(entity => entity.GetSnapDescriptors());
            snapEngine.Update(rawPoint, descriptors);
            host.ToolService.Viewport.GetRubberObject().SnapPoint = snapEngine.CurrentSnap;
        }

        private IEnumerable<Entity> GetSnapCandidates(IInteractiveCommandHost host, Point rawPoint, double toleranceWorld, ISpatialQueryService spatial)
        {
            return spatial?.QueryNearby(rawPoint, toleranceWorld) ?? GetAllEntities(host);
        }

        /// <summary>
        /// Returns all entities in the document as snap candidates when no spatial query is available.
        /// </summary>
        protected IEnumerable<Entity> GetAllEntities(IInteractiveCommandHost host)
        {
            var document = host.ToolService.GetService<ICadDocumentService>();
            if (document == null)
                yield break;

            foreach (var entity in document.Entities)
                yield return entity;
        }
    }
}
