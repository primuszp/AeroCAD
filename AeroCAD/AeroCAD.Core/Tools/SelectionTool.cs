using System.Windows;
using System.Windows.Input;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Tools
{
    public class SelectionTool : BaseTool, IMouseListener, IKeyboardListener
    {
        #region Members

        private Point start;
        private Point end;

        #endregion

        #region Constructors

        public SelectionTool()
            : base("SelectionTool")
        { }

        public override int InputPriority => 100;

        #endregion

        #region IMouseListener

        public void MouseButtonDown(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended) return;

            var viewport = ToolService.Viewport;
            var rbo = ToolService.Viewport.GetRubberObject();
            var selMgr = ToolService.GetService<ISelectionManager>();
            var overlay = ToolService.GetService<Overlay>();
            Point wpos = viewport.Position;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Check if a grip was clicked first
                var grip = overlay?.HitTestGrip(wpos);
                if (grip != null)
                {
                    var gripDragTool = ToolService.GetTool("GripDragTool") as GripDragTool;
                    gripDragTool?.BeginDrag(grip);
                    e.Handled = true;
                    return;
                }

                rbo.CurrentStyle = RubberStyle.Select;

                if (rbo.CurrentState == RubberState.Rubber)
                {
                    // Finish rectangle selection
                    rbo.SetStop(wpos);
                    end = rbo.End;

                    bool isWindowSelection = (end.X > start.X); // Left to right = Window
                    Rect selectionRect = new Rect(start, end);
                    var spatial = ToolService.GetService<ISpatialQueryService>();
                    var candidates = spatial?.QueryIntersecting(selectionRect);
                    var hits = ToolService.Viewport.QueryHitEntities(selectionRect, isWindowSelection, candidates);
                    bool removeFromSelection = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                    if (removeFromSelection)
                    {
                        foreach (var hit in hits)
                            selMgr.Deselect(hit);
                    }
                    else
                    {
                        selMgr.SelectRange(hits);
                    }
                    SetEditorMode(EditorMode.Idle);
                }
                else
                {
                    // First try point selection
                    var spatial = ToolService.GetService<ISpatialQueryService>();
                    var pickSettings = ToolService.GetService<IPickSettingsService>();
                    double pickRadius = pickSettings?.GetPickRadiusWorld(ToolService.Viewport.Zoom) ?? (4.0d / ToolService.Viewport.Zoom);
                    var candidates = spatial?.QueryNearby(wpos, pickRadius);
                    var hits = ToolService.Viewport.QueryHitEntities(wpos, pickRadius, candidates);

                    if (hits.Count > 0)
                    {
                        var pickResolver = ToolService.GetService<IPickResolutionService>();
                        var primaryHit = pickResolver?.ResolvePrimary(hits) ?? ResolvePrimaryHit(hits);
                        bool removeFromSelection = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                        if (removeFromSelection && selMgr.IsSelected(primaryHit))
                            selMgr.Deselect(primaryHit);
                        else
                            selMgr.Select(primaryHit);
                        SetEditorMode(EditorMode.Idle);
                    }
                    else
                    {
                        // Start rectangle selection
                        rbo.SetStart(wpos);
                        rbo.SetMove(wpos);
                        start = rbo.Start;
                        end = start;
                        SetEditorMode(EditorMode.SelectionWindow);
                    }
                }
            }

            e.Handled = false;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended) return;

            var rbo = ToolService.Viewport.GetRubberObject();
            if (rbo.CurrentState == RubberState.Rubber)
            {
                rbo.SnapPoint = null;
                rbo.SetMove(ToolService.Viewport.Position);
                SetEditorMode(EditorMode.SelectionWindow);
            }
            else
            {
                UpdateGripPreview();
                SetEditorMode(EditorMode.Idle);
            }

            e.Handled = true;
        }

        public void MouseButtonUp(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended) return;
            var rbo = ToolService.Viewport.GetRubberObject();
            if (rbo.CurrentState != RubberState.Rubber)
                ClearSnapPreview();
            SetEditorMode(rbo.CurrentState == RubberState.Rubber
                ? EditorMode.SelectionWindow
                : EditorMode.Idle);
            e.Handled = true;
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive || IsSuspended) return;

            ToolService.Viewport.GetRubberObject().InvalidateVisual();
            e.Handled = true;
        }

        #endregion

        #region IKeyboardListener

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var rubberObject = ToolService.Viewport.GetRubberObject();
                rubberObject.SnapPoint = null;
                if (rubberObject.CurrentState == RubberState.Rubber)
                    rubberObject.Cancel();
                SetEditorMode(EditorMode.Idle);
                e.Handled = true;
            }
        }

        public void KeyUp(KeyEventArgs e)
        {
            if (!IsActive) return;
            e.Handled = true;
        }

        public override bool Activate()
        {
            var activated = base.Activate();
            if (activated)
                ClearSnapPreview();
            if (activated)
                SetEditorMode(EditorMode.Idle);
            return activated;
        }

        private void SetEditorMode(EditorMode mode)
        {
            ToolService.GetService<IEditorStateService>()?.SetMode(mode);
        }

        private void ClearSnapPreview()
        {
            var rubberObject = ToolService.Viewport.GetRubberObject();

            if (rubberObject == null)
                return;

            rubberObject.SnapPoint = null;
            rubberObject.InvalidateVisual();
        }

        private void UpdateGripPreview()
        {
            var rubberObject = ToolService.Viewport.GetRubberObject();
            var snapEngine = ToolService.GetService<ISnapEngine>();
            var descriptorService = ToolService.GetService<ISnapDescriptorService>();

            if (rubberObject == null || snapEngine == null || descriptorService == null)
                return;

            var descriptors = descriptorService.GetSelectedGripDescriptors();
            snapEngine.Update(ToolService.Viewport.Position, descriptors);
            if (snapEngine.CurrentSnap == null)
            {
                ClearSnapPreview();
                return;
            }

            rubberObject.SnapPoint = snapEngine.CurrentSnap;
            rubberObject.InvalidateVisual();
        }

        private static Drawing.Entities.Entity ResolvePrimaryHit(System.Collections.Generic.IList<Drawing.Entities.Entity> hits)
        {
            return hits != null && hits.Count > 0 ? hits[hits.Count - 1] : null;
        }

        #endregion
    }
}

