using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Drawing.Entities;
using System.Windows;
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

        [Fact]
        public void CreateEntityOnPoint_ConfiguresPointAndTokenHandlers()
        {
            var registration = InteractiveCommandRegistrationBuilder
                .Create("POINTENTITY")
                .CreateEntityOnPoint((context, point) => new TestEntity(point))
                .Build();

            var controller = Assert.IsType<DelegateInteractiveCommandController>(registration.ControllerFactory());

            Assert.Equal("POINTENTITY", controller.CommandName);
        }

        private sealed class TestEntity : Entity
        {
            public TestEntity(Point point)
            {
                Point = point;
            }

            public Point Point { get; }
            public override int GripCount => 0;
            public override Point GetGripPoint(int index) => Point;
            public override void MoveGrip(int index, Point newPosition) { }
            public override Entity Clone() => new TestEntity(Point);
            public override Entity Duplicate() => new TestEntity(Point);
            public override void RestoreState(Entity sourceState) { }
            public override void Translate(Vector delta) { }
        }
    }
}
