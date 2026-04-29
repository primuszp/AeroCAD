using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class CommandFeedbackServiceTests
    {
        [Fact]
        public void BeginCommand_LogsCommandNameOnce()
        {
            var service = new CommandFeedbackService();
            var messages = new List<string>();
            service.MessageLogged += (_, e) => messages.Add(e.Message);

            service.BeginCommand("polygon", "Specify number of sides [3-1024] <4>:");

            Assert.Equal("Specify number of sides [3-1024] <4>:", service.Prompt);
            Assert.Equal(new[] { "POLYGON" }, messages);
        }

        [Fact]
        public void Prompt_KeepsOriginalDisplayTextAfterPromptUpdate()
        {
            var service = new CommandFeedbackService();

            service.BeginCommand("offset", "Specify offset distance:");
            service.SetPrompt("Specify object to offset:");

            Assert.Equal("Specify object to offset:", service.Prompt);
        }

        [Fact]
        public void ParseInput_ParsesRelativePoint()
        {
            var service = new CommandFeedbackService();

            var token = service.ParseInput("@10,20");

            Assert.Equal(CommandInputTokenKind.Point, token.Kind);
            Assert.Equal("@10,20", token.RawText);
            Assert.Equal(new Point(10, 20), token.PointValue);
        }

        [Fact]
        public void TryResolvePointInput_AppliesRelativePointToBasePoint()
        {
            var tool = new TestTool();
            var token = CommandInputToken.Point("@10,20", new Point(10, 20));

            var resolved = tool.ResolvePoint(token, new Point(5, 7), out var point);

            Assert.True(resolved);
            Assert.Equal(new Point(15, 27), point);
        }

        private sealed class TestTool : BaseTool
        {
            public TestTool()
                : base("Test")
            {
            }

            public bool ResolvePoint(CommandInputToken token, Point? basePoint, out Point point)
            {
                return TryResolvePointInput(token, basePoint, out point);
            }
        }
    }
}
