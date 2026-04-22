using System;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveShapeRuntime : IInteractiveShapeRuntime
    {
        private readonly InteractiveCommandRegistration registration;

        public InteractiveShapeRuntime(InteractiveCommandRegistration registration)
        {
            this.registration = registration ?? throw new ArgumentNullException(nameof(registration));
            CommandName = this.registration.CommandName;
            ToolName = this.registration.ToolName;
        }

        public string CommandName { get; }
        public string ToolName { get; }

        public ITool CreateTool()
        {
            return new ShapeInteractiveTool(registration, ToolName);
        }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return registration;
        }

        private sealed class ShapeInteractiveTool : InteractiveCommandTool<IInteractiveCommandController>
        {
            public ShapeInteractiveTool(InteractiveCommandRegistration registration, string toolName)
                : base(layer => registration.ControllerFactory(), toolName)
            {
            }
        }
    }
}
