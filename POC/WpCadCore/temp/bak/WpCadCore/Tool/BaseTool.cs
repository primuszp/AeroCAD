using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace WpCadCore.Tool
{
    abstract class BaseTool : ITool
    {
        #region Fields

        /// <summary>
        /// Keeps a reference to the previous cursor
        /// </summary>
        private Cursor prev_cursor;

        private bool enabled = true;

        #endregion

        public BaseTool(string name)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
        }

        #region ITool Members

        public Guid Id { get; protected set; }

        public string Name { get; private set; }

        public IToolService ToolService { get; set; }

        public bool IsActive { get; set; }

        public virtual bool CanActivate
        {
            get { return (enabled) ? !this.IsActive : false; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                // disable the tool first if it is active
                if (!value && IsActive) this.Deactivate();
                enabled = true;
            }
        }

        public bool IsSuspended { get; set; }

        public bool Activate()
        {
            if (this.ToolService != null) this.ToolService.SuspendAll(this);

            if (Enabled && !IsActive)
            {
                prev_cursor = Mouse.OverrideCursor;
                IsActive = true;
            }

            return IsActive;
        }

        public bool Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                this.RestoreCursor();
                if (ToolService != null) ToolService.UnsuspendAll();
                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        protected void RestoreCursor()
        {
            if (prev_cursor != null)
            {
                Mouse.OverrideCursor = prev_cursor;
                prev_cursor = null;
            }
        }

        #endregion
    }
}
