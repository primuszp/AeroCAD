using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class InteractiveCommandRegistrationBuilderTests
    {
        [Fact]
        public void Build_CreatesCommandDefinitionAndDelegateController()
        {
            var step = new CommandStep("Point", "Specify point:");
            var registration = InteractiveCommandRegistrationBuilder
                .Create("TESTCMD")
                .WithAliases("TC")
                .WithDescription("Test command.")
                .WithInitialStep(step)
                .InMenu("Draw", "_Test")
                .OnPoint((context, point) => context.End("Done."))
                .Build();

            var definition = registration.CreateCommandDefinition();
            var controller = registration.ControllerFactory();

            Assert.Equal("TESTCMD", definition.Name);
            Assert.Contains("TC", definition.Aliases);
            Assert.Equal("Draw", definition.MenuGroup);
            Assert.Equal("_Test", definition.MenuLabel);
            Assert.IsType<DelegateInteractiveCommandController>(controller);
            Assert.Same(step, controller.InitialStep);
        }
    }
}
