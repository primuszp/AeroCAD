using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class InteractiveShapeDefinitionTests
    {
        [Fact]
        public void CreateCommandRegistration_PreservesCommandMetadata()
        {
            var step = new CommandStep("Sides", "Enter number of sides <4>:");
            var definition = new InteractiveShapeDefinition(
                name: "MyCompany.Polygon",
                commandName: "polygon",
                controllerFactory: () => new StubController(),
                steps: new[] { step },
                aliases: new[] { "POL" },
                description: "Draw a polygon.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Polygon");

            var registration = definition.CreateCommandRegistration();

            Assert.Equal("POLYGON", registration.CommandName);
            Assert.Equal("Draw a polygon.", registration.Description);
            Assert.True(registration.AssignActiveLayer);
            Assert.Equal("Draw", registration.MenuGroup);
            Assert.Equal("_Polygon", registration.MenuLabel);
            Assert.Equal("POLYGON", definition.CommandName);
            Assert.Equal(step, definition.InitialStep);
        }

        private sealed class StubController : IInteractiveCommandController
        {
            public string CommandName => "POLYGON";
            public CommandStep InitialStep => null;
            public EditorMode EditorMode => EditorMode.CommandInput;
            public void OnActivated(IInteractiveCommandHost host) { }
            public void OnPointerMove(IInteractiveCommandHost host, System.Windows.Point rawPoint) { }
            public InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, System.Windows.Point rawPoint) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult TryComplete(IInteractiveCommandHost host) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult TryCancel(IInteractiveCommandHost host) => InteractiveCommandResult.Unhandled();
        }
    }
}
