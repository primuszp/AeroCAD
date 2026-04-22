using System;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveShapeRuntime : IInteractiveShapeRuntime
    {
        private readonly Func<Func<Layer>, IInteractiveCommandController> controllerFactory;
        private readonly InteractiveCommandRegistration registration;

        public InteractiveShapeRuntime(
            string commandName,
            string toolName,
            Func<Func<Layer>, IInteractiveCommandController> controllerFactory,
            InteractiveCommandRegistration registration)
        {
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
            this.controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            this.registration = registration ?? throw new ArgumentNullException(nameof(registration));
        }

        public string CommandName { get; }
        public string ToolName { get; }

        public IInteractiveCommandController CreateController(Func<Layer> activeLayerResolver)
        {
            return controllerFactory(activeLayerResolver);
        }

        public InteractiveCommandRegistration CreateCommandRegistration()
        {
            return registration;
        }
    }
}
