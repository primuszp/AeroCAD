using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class GripEditInteractiveShapeSession
    {
        public Grip ActiveGrip { get; private set; }
        public Entity StateBeforeDrag { get; private set; }
        public bool IgnoreNextMouseUp { get; private set; }
        public Point PreviewPosition { get; private set; }
        public Point DragBasePoint { get; private set; }

        public bool HasGrip => ActiveGrip != null;

        public void BeginDrag(Grip grip)
        {
            ActiveGrip = grip;
            StateBeforeDrag = grip.Owner.Clone();
            IgnoreNextMouseUp = true;
            PreviewPosition = grip.Owner.GetGripPoint(grip.Index);
            DragBasePoint = PreviewPosition;
            ActiveGrip.Select();
        }

        public void UpdatePreview(Point previewPosition)
        {
            PreviewPosition = previewPosition;
        }

        public void ConsumeInitialMouseUp()
        {
            IgnoreNextMouseUp = false;
        }

        public void Reset()
        {
            ActiveGrip = null;
            StateBeforeDrag = null;
            IgnoreNextMouseUp = false;
            PreviewPosition = default(Point);
            DragBasePoint = default(Point);
        }

        public GripPreview BuildPreview(IGripPreviewService gripPreviewService)
        {
            return gripPreviewService?.CreatePreview(StateBeforeDrag, ActiveGrip.Index, PreviewPosition) ?? GripPreview.Empty;
        }
    }
}
