using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class InteractiveShapeRegistryTests
    {
        [Fact]
        public void Find_ByNameOrCommandName_ReturnsDefinition()
        {
            var definition = new InteractiveShapeDefinition(
                "MyCompany.Polygon",
                "POLYGON",
                layerProvider => new StubController(),
                new[] { new CommandStep("Sides", "Sides") });

            var registry = new InteractiveShapeRegistry(new[] { definition });

            Assert.Same(definition, registry.Find("MyCompany.Polygon"));
            Assert.Same(definition, registry.Find("polygon"));
            Assert.Same(definition, registry.Find("POLYGON"));
            Assert.Null(registry.Find("missing"));
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
