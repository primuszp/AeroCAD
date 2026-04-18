using System;

namespace Primusz.AeroCAD.Core.Tools
{
    public class RegisteredInteractiveCommandTool : InteractiveCommandTool<IInteractiveCommandController>
    {
        public RegisteredInteractiveCommandTool(
            Func<Func<Drawing.Layers.Layer>, IInteractiveCommandController> controllerFactory,
            string toolName)
            : base(controllerFactory, toolName)
        {
        }
    }
}
