using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;
using Primusz.AeroCAD.View.Editor;
using Xunit;

namespace Primusz.AeroCAD.View.Tests.Editor
{
    public class EditorCommandRuntimeTests
    {
        [Fact]
        public void Execute_WhenInteractiveCommandIsActive_DoesNotStartAnotherCommand()
        {
            var activeTool = new FakeInteractiveTool { IsActive = true };
            var toolRuntime = new FakeToolRuntime(activeTool);
            var runtime = new EditorCommandRuntime(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                toolRuntime,
                null,
                null,
                null,
                _ => { });

            var executed = runtime.Execute("MOVE");

            Assert.False(executed);
        }

        private sealed class FakeInteractiveTool : ICommandInteractiveTool, ITool
        {
            public bool IsActive { get; set; }
            public Guid Id { get; } = Guid.NewGuid();
            public string Name { get; } = "Fake";
            public IToolService ToolService { get; set; }
            public CadCursorType CursorType => CadCursorType.CrosshairOnly;
            public int InputPriority => 0;
            public bool CanActivate => true;
            public bool Enabled { get; set; } = true;
            public bool IsSuspended { get; set; }
            public bool Activate() => true;
            public bool Deactivate() => true;
            public bool TrySubmitToken(CommandInputToken token) => false;
            public bool TrySubmitText(string input) => false;
            public bool TrySubmitPoint(Point point) => false;
            public bool TryComplete() => false;
            public bool TryCancel() => false;
        }

        private sealed class FakeToolRuntime : IEditorToolRuntime
        {
            private readonly ICommandInteractiveTool activeTool;

            public FakeToolRuntime(ICommandInteractiveTool activeTool)
            {
                this.activeTool = activeTool;
            }

            public ICommandInteractiveTool GetActiveInteractiveTool() => activeTool;
            public bool CancelActiveInteractiveTool() => false;
            public bool ActivateSelectionMode() => true;
            public bool ActivateModalTool<TTool>(Layer activeLayer = null) where TTool : class, ITool => true;
            public bool ActivateModalTool(Type toolType, Layer activeLayer = null) => true;
            public bool ActivateModalTool(string toolName, Layer activeLayer = null) => true;
        }
    }
}
