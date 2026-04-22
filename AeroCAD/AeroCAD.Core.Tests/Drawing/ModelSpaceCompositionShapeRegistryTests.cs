using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Drawing
{
    public class ModelSpaceCompositionShapeRegistryTests
    {
        [Fact]
        public void BuildServices_RegistersModuleShapesInInteractiveShapeRegistry()
        {
            InteractiveShapeRegistry registry = null;

            var thread = new Thread(() =>
            {
                var viewport = new Viewport();
                var composition = new ModelSpaceComposition(viewport);
                var shape = new InteractiveShapeDefinition(
                    "MyCompany.Polygon",
                    "POLYGON",
                    () => new StubController(),
                    new[] { new CommandStep("Sides", "Enter number of sides <4>:") },
                    aliases: new[] { "POL" });

                composition.RegisterModule(new TestModule(new[] { shape }));

                var services = composition.BuildServices();
                registry = Assert.IsType<InteractiveShapeRegistry>(services[typeof(IInteractiveShapeRegistry)]);

                Assert.Single(registry.Definitions);
                Assert.Same(shape, registry.Find("POLYGON"));
                Assert.Same(shape, registry.Find("MyCompany.Polygon"));
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.NotNull(registry);
        }

        private sealed class TestModule : CadModuleBase
        {
            private readonly IEnumerable<IInteractiveShapeDefinition> shapes;

            public TestModule(IEnumerable<IInteractiveShapeDefinition> shapes)
            {
                this.shapes = shapes;
            }

            public override string Name => "MyCompany.TestModule";
            public override IEnumerable<IEntityPlugin> Plugins => Enumerable.Empty<IEntityPlugin>();
            public override IEnumerable<IInteractiveShapeDefinition> Shapes => shapes;
        }

        private sealed class StubController : IInteractiveCommandController
        {
            public string CommandName => "POLYGON";
            public CommandStep InitialStep => null;
            public EditorMode EditorMode => EditorMode.CommandInput;
            public void OnActivated(IInteractiveCommandHost host) { }
            public void OnPointerMove(IInteractiveCommandHost host, Point rawPoint) { }
            public InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult TryComplete(IInteractiveCommandHost host) => InteractiveCommandResult.Unhandled();
            public InteractiveCommandResult TryCancel(IInteractiveCommandHost host) => InteractiveCommandResult.Unhandled();
        }
    }
}
