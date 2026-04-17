using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Tools
{
    public abstract class BaseTool : ITool
    {
        #region Members

        /// <summary>
        /// Keeps a reference to the previous cursor
        /// </summary>
        private Cursor prevCursor;

        private bool enabled = true;

        #endregion

        protected BaseTool(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        #region ITool Members

        public Guid Id { get; protected set; }

        public string Name { get; private set; }

        public CadCursorType CursorType { get; set; } = CadCursorType.CrosshairAndPickbox;

        public virtual int InputPriority => 0;

        public IToolService ToolService { get; set; }

        public bool IsActive { get; set; }

        public virtual bool CanActivate
        {
            get { return enabled && !IsActive; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                // Disable the tool first if it is active
                if (!value && IsActive) Deactivate();
                enabled = value;
            }
        }

        public bool IsSuspended { get; set; }

        public virtual bool Activate()
        {
            if (ToolService != null)
                ToolService.SuspendAll(this);

            IsSuspended = false;

            if (Enabled && !IsActive)
            {
                prevCursor = Mouse.OverrideCursor;
                IsActive = true;
            }

            if (IsActive && ToolService?.Viewport != null)
            {
                ToolService.Viewport.ActiveCursorType = CursorType;
                ToolService.Viewport.GetRubberObject()?.InvalidateVisual();
                ToolService.Viewport.InvalidateVisual();
            }

            return IsActive;
        }

        public bool Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                RestoreCursor();

                if (ToolService != null)
                {
                    ToolService.Viewport?.GetRubberObject()?.Cancel();
                    if (ToolService.Viewport?.GetRubberObject() != null)
                    {
                        ToolService.Viewport.GetRubberObject().SnapPoint = null;
                    }

                    ToolService.UnsuspendAll();
                }

                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        protected void RestoreCursor()
        {
            if (prevCursor != null)
            {
                Mouse.OverrideCursor = prevCursor;
                prevCursor = null;
            }
        }

        /// <summary>
        /// Resolves the final world point, applying snap then ortho (snap wins if active).
        /// When no basePoint applies (e.g. first click), pass <see langword="null"/> to skip ortho.
        /// </summary>
        protected Point GetFinalPoint(Point? basePoint, Point rawPos)
        {
            // Snap takes priority â€” if a snap candidate is active, use it directly
            var snapEngine = ToolService.GetService<ISnapEngine>();
            if (snapEngine?.CurrentSnap != null)
                return snapEngine.CurrentSnap.Point;

            // No snap: apply ortho if enabled and a base point is known
            if (basePoint.HasValue)
            {
                var ortho = ToolService.GetService<IOrthoService>();
                if (ortho != null)
                    return ortho.Apply(basePoint.Value, rawPos);
            }

            return rawPos;
        }

        protected bool TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point)
        {
            point = new Point();
            if (token == null || token.IsEmpty)
                return false;

            if (token.Kind == CommandInputTokenKind.Point && token.PointValue.HasValue)
            {
                point = token.PointValue.Value;
                return true;
            }

            if (!basePoint.HasValue)
                return false;

            if (token.Kind != CommandInputTokenKind.Scalar || !token.ScalarValue.HasValue)
                return false;

            double distance = token.ScalarValue.Value;

            var ortho = ToolService.GetService<IOrthoService>();
            if (ortho == null || !ortho.IsEnabled || ToolService?.Viewport == null)
                return false;

            var rawCursorPoint = ToolService.Viewport.Position;
            var orthoPoint = ortho.Apply(basePoint.Value, rawCursorPoint);
            var delta = orthoPoint - basePoint.Value;
            if (Math.Abs(delta.X) < double.Epsilon && Math.Abs(delta.Y) < double.Epsilon)
                return false;

            double magnitude = Math.Abs(distance);
            if (Math.Abs(delta.X) >= Math.Abs(delta.Y))
            {
                double signedDistance = delta.X >= 0 ? magnitude : -magnitude;
                point = new Point(basePoint.Value.X + signedDistance, basePoint.Value.Y);
                return true;
            }

            double signedVerticalDistance = delta.Y >= 0 ? magnitude : -magnitude;
            point = new Point(basePoint.Value.X, basePoint.Value.Y + signedVerticalDistance);
            return true;
        }

        protected bool TryResolveScalarInput(CommandInputToken token, out double scalar)
        {
            scalar = 0d;
            if (token == null || token.IsEmpty)
                return false;

            if (token.Kind != CommandInputTokenKind.Scalar || !token.ScalarValue.HasValue)
                return false;

            scalar = token.ScalarValue.Value;
            return true;
        }

        protected static string FormatPoint(Point point)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.###},{1:0.###}", point.X, point.Y);
        }

        protected void ReturnToSelectionMode()
        {
            if (ToolService == null)
                return;

            ToolService.GetService<IEditorStateService>()?.SetMode(EditorMode.Idle);

            var selectionTool = ToolService.GetTool<SelectionTool>();

            if (selectionTool == null)
                return;

            selectionTool.IsSuspended = false;

            if (!selectionTool.IsActive)
            {
                selectionTool.Activate();
            }
            else if (ToolService.Viewport != null)
            {
                ToolService.Viewport.ActiveCursorType = selectionTool.CursorType;
                ToolService.Viewport.GetRubberObject()?.InvalidateVisual();
                ToolService.Viewport.InvalidateVisual();
            }
        }

        #endregion
    }
}

