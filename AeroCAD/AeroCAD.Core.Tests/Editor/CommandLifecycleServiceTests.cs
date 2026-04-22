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
                "Command:",
                activeTool,
                token => CommandInputToken.Text(token, token),
                token => false,
                command => false,
                () => "LINE",
                input => input?.ToUpperInvariant(),
                () => refreshCount++,
                message => { },
                (_, __, ___) => { });

            Assert.True(handled);
            Assert.Equal(1, refreshCount);
            Assert.Equal("LINE", repeat.LastExecutedCommand);
            Assert.True(activeTool.CompleteCalls > 0);
        }

        [Fact]
        public void BlankSubmit_WithActiveTool_UsesEmptyTokenBeforeComplete()
        {
            var repeat = new CommandRepeatCoordinator();
            var service = new CommandLifecycleService(repeat);
            var activeTool = new StubInteractiveTool { SubmitEmptyResult = true };
            var refreshCount = 0;

            var handled = service.TryHandleCommandLineInput(
                string.Empty,
                "Command:",
                activeTool,
                token => CommandInputToken.Empty(),
                token => activeTool.TrySubmitToken(token),
                command => false,
                () => "POLYGON",
                input => input?.ToUpperInvariant(),
                () => refreshCount++,
                message => { },
                (_, __, ___) => { });

            Assert.True(handled);
            Assert.Equal(1, refreshCount);
            Assert.Equal("POLYGON", repeat.LastExecutedCommand);
            Assert.Equal(0, activeTool.CompleteCalls);
            Assert.Equal(1, activeTool.EmptySubmitCalls);
        }

        [Fact]
        public void BlankSubmit_WithoutLastCommand_IsStillHandled()
        {
            var repeat = new CommandRepeatCoordinator();
            var service = new CommandLifecycleService(repeat);

            var handled = service.TryHandleCommandLineInput(
                string.Empty,
                "Command:",
                null,
                token => CommandInputToken.Text(token, token),
                token => false,
                command => false,
                () => string.Empty,
                input => input?.ToUpperInvariant(),
                () => { },
                message => { },
                (_, __, ___) => { });

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
                "Command:",
                null,
                token => CommandInputToken.Text(token, token),
                token => false,
                command => command == "LINE",
                () => string.Empty,
                input => input?.ToUpperInvariant(),
                () => { },
                message => { },
                (_, __, ___) => { });

            Assert.True(handled);
            Assert.Equal("LINE", repeat.LastExecutedCommand);
        }

        private sealed class StubInteractiveTool : ICommandInteractiveTool
        {
            public int CompleteCalls { get; private set; }
            public int EmptySubmitCalls { get; private set; }
            public bool SubmitEmptyResult { get; set; }

            public bool TrySubmitToken(CommandInputToken token)
            {
                if (token != null && token.IsEmpty)
                {
                    EmptySubmitCalls++;
                    return SubmitEmptyResult;
                }

                return false;
            }

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
