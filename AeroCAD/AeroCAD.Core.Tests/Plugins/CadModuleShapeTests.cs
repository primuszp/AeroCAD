using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class CadModuleShapeTests
    {
        [Fact]
        public void Module_CanExposeInteractiveShapes_AndRegistryCanResolveThem()
        {
            var step = new CommandStep("Sides", "Enter number of sides <4>:");
            var shape = new InteractiveShapeDefinition(
                name: "MyCompany.Polygon",
                commandName: "POLYGON",
                controllerFactory: layerProvider => new StubController(),
                steps: new[] { step },
                aliases: new[] { "POL" },
                description: "Draw a polygon.");

            var module = new TestModule(new[] { shape });

            var shapes = module.Shapes.ToArray();
            Assert.Single(shapes);
            Assert.Same(shape, shapes[0]);

            var registry = new InteractiveShapeRegistry(shapes);
            Assert.Same(shape, registry.Find("MyCompany.Polygon"));
            Assert.Same(shape, registry.Find("POLYGON"));
        }

        private sealed class TestModule : CadModuleBase
        {
            private readonly IEnumerable<IInteractiveShapeDefinition> shapes;

            public TestModule(IEnumerable<IInteractiveShapeDefinition> shapes)
            {
                this.shapes = shapes;
            }

            public override string Name => "MyCompany.TestModule";
            public override IEnumerable<IEntityPlugin> Plugins => System.Linq.Enumerable.Empty<IEntityPlugin>();
            public override IEnumerable<IInteractiveShapeDefinition> Shapes => shapes;
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
