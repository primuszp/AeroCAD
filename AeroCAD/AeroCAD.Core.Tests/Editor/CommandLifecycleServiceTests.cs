using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class CommandLifecycleServiceTests
    {
        [Fact]
        public void BlankSubmit_WithActiveTool_CompletesAndRemembersCommand()
        {
            var repeat = new CommandRepeatCoordinator();
            var service = new CommandLifecycleService(repeat);
            var activeTool = new StubInteractiveTool { CompleteResult = true };
            var refreshCount = 0;

            var handled = service.TryHandleCommandLineInput(
                string.Empty,
                activeTool,
                token => CommandInputToken.Text(token, token),
                token => false,
                command => false,
                () => "LINE",
                () => refreshCount++,
                message => { });

            Assert.True(handled);
            Assert.Equal(1, refreshCount);
            Assert.Equal("LINE", repeat.LastExecutedCommand);
            Assert.True(activeTool.CompleteCalls > 0);
        }

        [Fact]
        public void BlankSubmit_WithoutLastCommand_IsStillHandled()
        {
            var repeat = new CommandRepeatCoordinator();
            var service = new CommandLifecycleService(repeat);

            var handled = service.TryHandleCommandLineInput(
                string.Empty,
                null,
                token => CommandInputToken.Text(token, token),
                token => false,
                command => false,
                () => string.Empty,
                () => { },
                message => { });

            Assert.True(handled);
            Assert.Equal(string.Empty, repeat.LastExecutedCommand);
        }

        [Fact]
        public void ExecutedCommand_IsRemembered()
        {
            var repeat = new CommandRepeatCoordinator();
            var service = new CommandLifecycleService(repeat);

            var handled = service.TryHandleCommandLineInput(
                "line",
                null,
                token => CommandInputToken.Text(token, token),
                token => false,
                command => command == "LINE",
                () => string.Empty,
                () => { },
                message => { });

            Assert.True(handled);
            Assert.Equal("LINE", repeat.LastExecutedCommand);
        }

        private sealed class StubInteractiveTool : ICommandInteractiveTool
        {
            public int CompleteCalls { get; private set; }

            public bool TrySubmitToken(CommandInputToken token) => false;

            public bool TrySubmitText(string input) => false;

            public bool TrySubmitPoint(Point point) => false;

            public bool TryComplete()
            {
                CompleteCalls++;
                return CompleteResult;
            }

            public bool TryCancel() => false;

            public bool CompleteResult { get; set; }
        }
    }
}
