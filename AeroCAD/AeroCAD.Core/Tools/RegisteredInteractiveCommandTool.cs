using System;

namespace Primusz.AeroCAD.Core.Tools
{
    public class RegisteredInteractiveCommandTool : InteractiveCommandTool<IInteractiveCommandController>
    {
        public RegisteredInteractiveCommandTool(
            Func<IInteractiveCommandController> controllerFactory,
            string toolName)
            : base(_ => controllerFactory(), toolName)
        {
        }
    }
}
