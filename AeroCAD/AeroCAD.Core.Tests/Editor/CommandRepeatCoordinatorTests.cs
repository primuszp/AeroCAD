using System;
using System.Windows;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class CommandRepeatCoordinatorTests
    {
        [Fact]
        public void BlankSubmit_WithActiveTool_CompletesCurrentCommand()
        {
            var coordinator = new CommandRepeatCoordinator();
            coordinator.RememberExecutedCommand("LINE");
            var tool = new StubInteractiveTool { CompleteResult = true };
            bool refreshed = false;

            var handled = coordinator.HandleBlankSubmit(tool, _ => false, () => "PLINE", () => refreshed = true);

            Assert.True(handled);
            Assert.True(tool.CompleteCalled);
            Assert.True(refreshed);
            Assert.Equal("PLINE", coordinator.LastExecutedCommand);
        }

        [Fact]
        public void BlankSubmit_WithoutActiveTool_RepeatsLastCommand()
        {
            var coordinator = new CommandRepeatCoordinator();
            coordinator.RememberExecutedCommand("LINE");
            bool refreshed = false;
            string executed = null;

            var handled = coordinator.HandleBlankSubmit(null, command =>
            {
                executed = command;
                return true;
            }, () => null, () => refreshed = true);

            Assert.True(handled);
            Assert.Equal("LINE", executed);
            Assert.True(refreshed);
        }

        [Fact]
        public void BlankSubmit_WithoutLastCommand_DoesNothing()
        {
            var coordinator = new CommandRepeatCoordinator();
            bool refreshed = false;
            bool executed = false;

            var handled = coordinator.HandleBlankSubmit(null, _ =>
            {
                executed = true;
                return true;
            }, () => null, () => refreshed = true);

            Assert.False(handled);
            Assert.False(executed);
            Assert.False(refreshed);
        }

        [Fact]
        public void RememberExecutedCommand_StoresNormalizedCommandName()
        {
            var coordinator = new CommandRepeatCoordinator();

            coordinator.RememberExecutedCommand("  pline  ");

            Assert.Equal("pline", coordinator.LastExecutedCommand);
        }

        private sealed class StubInteractiveTool : ICommandInteractiveTool
        {
            public bool CompleteCalled { get; private set; }
            public bool CompleteResult { get; set; }

            public bool TrySubmitToken(CommandInputToken token) => false;
            public bool TrySubmitText(string input) => false;
            public bool TrySubmitPoint(Point point) => false;

            public bool TryComplete()
            {
                CompleteCalled = true;
                return CompleteResult;
            }

            public bool TryCancel() => false;
        }
    }
}
