using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class InteractiveShapePipelineTests
    {
        [Fact]
        public void CreateRuntime_PreservesMetadata_AndRegistration()
        {
            var step = new CommandStep("Sides", "Enter number of sides <4>:");
            var pipeline = new InteractiveShapePipeline(
                name: "MyCompany.Polygon",
                commandName: "polygon",
                controllerFactory: () => new StubController(),
                steps: new[] { step },
                aliases: new[] { "POL" },
                description: "Draw a polygon.",
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Polygon");

            var runtime = pipeline.CreateRuntime();
            var registration = runtime.CreateCommandRegistration();
            var tool = runtime.CreateTool();

            Assert.Equal("MyCompany.Polygon", pipeline.Name);
            Assert.Equal("POLYGON", pipeline.CommandName);
            Assert.Equal(step, pipeline.InitialStep);
            Assert.Single(pipeline.Steps);
            Assert.Equal("POLYGON", runtime.CommandName);
            Assert.Equal("POLYGONTool", runtime.ToolName);
            Assert.Equal("POLYGONTool", tool.Name);
            Assert.Equal("POLYGON", registration.CommandName);
            Assert.Equal("Draw a polygon.", registration.Description);
            Assert.True(registration.AssignActiveLayer);
            Assert.Equal("Draw", registration.MenuGroup);
            Assert.Equal("_Polygon", registration.MenuLabel);
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
