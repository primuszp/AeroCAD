using System;
using System.Windows.Input;

namespace Primusz.Cadves.Core.Tools
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
                enabled = true;
            }
        }

        public bool IsSuspended { get; set; }

        public bool Activate()
        {
            if (ToolService != null)
                ToolService.SuspendAll(this);

            if (Enabled && !IsActive)
            {
                prevCursor = Mouse.OverrideCursor;
                IsActive = true;
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
                    ToolService.UnsuspendAll();

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

        #endregion
    }
}
