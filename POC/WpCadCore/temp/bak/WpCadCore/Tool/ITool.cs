using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpCadCore.Tool
{
    interface ITool
    {
        #region Properties

        /// <summary>
        /// Unique id of the tool.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Get user friendly name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the tool service responsible for maintaining this tool.
        /// </summary>
        IToolService ToolService { get; set; }

        /// <summary>
        /// Gets/Sets a value indicating whether the tool is active. True value means the tool is actually performing an activity.
        /// If Enabled property is set to false the tool can never be activated.
        /// If IsSuspeneded property is set to true the tool's activity was suspended by another tool.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Get a value indicating whether the tool can be activated.
        /// </summary>
        bool CanActivate { get; }

        /// <summary>
        /// Get/Set a value indicating whether the tool is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Get/Set a value indicating the tool is suspended.
        /// A tool enters this mode when another tool being activated disallows it to continue normal activity.
        /// The suspended state is independent on the IsActive and Enabled states.
        /// </summary>
        bool IsSuspended { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Activate the tool.
        /// </summary>
        /// <returns>Returns true if the operation finished successfully otherwise false.</returns>
        bool Activate();

        /// <summary>
        /// Deactivate the tool.
        /// </summary>
        /// <returns>Returns true if the operation finished successfully otherwise false.</returns>
        bool Deactivate();

        #endregion

    }
}
