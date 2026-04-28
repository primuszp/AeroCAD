using System.Windows;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class InteractiveCommandPromptBuilderTests
    {
        [Fact]
        public void PromptDistance_ResolvesPointTokenRelativeToBasePoint()
        {
            double captured = 0d;
            var registration = InteractiveCommandRegistrationBuilder
                .Create("DISTTEST")
                .PromptDistance(
                    new CommandStep("Distance", "Specify distance:"),
                    (context, distance) =>
                    {
                        captured = distance;
                        return context.Handled();
                    },
                    _ => new Point(0, 0))
                .Build();
            var controller = registration.ControllerFactory();
            var host = new TestHost();

            var result = controller.TrySubmitToken(host, CommandInputToken.Point("3,4", new Point(3, 4)));

            Assert.True(result.Handled);
            Assert.Equal(5d, captured);
        }

        private sealed class TestHost : IInteractiveCommandHost
        {
            public IToolService ToolService => null;
            public CommandStep CurrentStep => null;
            public bool TryResolveScalarInput(CommandInputToken token, out double scalar)
            {
                scalar = token.ScalarValue ?? 0d;
                return token.Kind == CommandInputTokenKind.Scalar;
            }

            public bool TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point)
            {
                point = token.PointValue ?? default;
                return token.Kind == CommandInputTokenKind.Point;
            }

            public Point ResolveFinalPoint(Point? basePoint, Point rawPos) => rawPos;
            public void MoveToStep(CommandStep step) { }
            public void EndSession(string closingMessage = null) { }
            public void DeactivateTool() { }
            public void ReturnToSelectionMode() { }
            public bool ApplyResult(InteractiveCommandResult result) => result?.Handled == true;
        }
    }
}
