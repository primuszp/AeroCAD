using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Drawing.Markers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Snapping;
using Primusz.AeroCAD.Core.Tools;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Tools
{
    public class PickboxSnapBehaviorTests
    {
        [Fact]
        public void ResolveFinalPoint_IgnoresSnap_WhenPickboxOnly()
        {
            RunOnStaThread(() =>
            {
                var viewport = CreateViewport();
                viewport.ActiveCursorType = CadCursorType.PickboxOnly;

                var snapEngine = new TestSnapEngine
                {
                    CurrentSnapValue = new SnapResult(new Point(100, 100), SnapType.Endpoint)
                };

                var tool = new TestBaseTool
                {
                    ToolService = new TestToolService(viewport, snapEngine)
                };

                var resolved = tool.ResolvePoint(null, new Point(10, 20));

                Assert.Equal(new Point(10, 20), resolved);
            });
        }

        [Fact]
        public void UpdateSnap_ClearsSnapAndSkipsEngine_WhenSelectionStep()
        {
            RunOnStaThread(() =>
            {
                var viewport = CreateViewport();
                var rubber = viewport.GetRubberObject();
                rubber.SnapPoint = new SnapResult(new Point(5, 5), SnapType.Endpoint);

                var snapEngine = new TestSnapEngine
                {
                    CurrentSnapValue = new SnapResult(new Point(100, 100), SnapType.Endpoint)
                };

                var controller = new TestCommandController();
                var host = new TestInteractiveCommandHost(
                    new TestToolService(viewport, snapEngine),
                    new CommandStep("Pick", "Pick:", inputMode: CommandInputMode.Selection));

                controller.InvokeUpdateSnap(host, new Point(1, 1));

                Assert.Equal(0, snapEngine.UpdateCalls);
                Assert.Null(rubber.SnapPoint);
            });
        }

        private static Viewport CreateViewport()
        {
            var viewport = new Viewport();
            _ = new RubberObject(viewport, new TestMarkerAppearanceService());
            return viewport;
        }

        private static void RunOnStaThread(Action action)
        {
            Exception exception = null;
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;
        }

        private sealed class TestBaseTool : BaseTool
        {
            public TestBaseTool() : base("Test") { }
            public Point ResolvePoint(Point? basePoint, Point rawPos) => GetFinalPoint(basePoint, rawPos);
        }

        private sealed class TestCommandController : CommandControllerBase
        {
            public override string CommandName => "TEST";
            public override CommandStep InitialStep => new CommandStep("Test", "Test:");
            public override EditorMode EditorMode => EditorMode.CommandInput;
            public override void OnActivated(IInteractiveCommandHost host) { }
            public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint) { }
            public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint) => InteractiveCommandResult.HandledOnly();
            public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token) => InteractiveCommandResult.HandledOnly();
            public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host) => InteractiveCommandResult.HandledOnly();
            public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host) => InteractiveCommandResult.HandledOnly();
            public void InvokeUpdateSnap(IInteractiveCommandHost host, Point rawPoint) => UpdateSnap(host, rawPoint);
        }

        private sealed class TestInteractiveCommandHost : IInteractiveCommandHost
        {
            public TestInteractiveCommandHost(IToolService toolService, CommandStep currentStep)
            {
                ToolService = toolService;
                CurrentStep = currentStep;
            }

            public IToolService ToolService { get; }
            public CommandStep CurrentStep { get; }
            public bool TryResolveScalarInput(CommandInputToken token, out double scalar) { scalar = 0; return false; }
            public bool TryResolvePointInput(CommandInputToken token, Point? basePoint, out Point point) { point = default; return false; }
            public Point ResolveFinalPoint(Point? basePoint, Point rawPos) => rawPos;
            public void MoveToStep(CommandStep step) { }
            public void EndSession(string closingMessage = null) { }
            public void DeactivateTool() { }
            public void ReturnToSelectionMode() { }
            public bool ApplyResult(InteractiveCommandResult result) => false;
        }

        private sealed class TestToolService : IToolService
        {
            private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

            public TestToolService(Viewport viewport, ISnapEngine snapEngine)
            {
                Viewport = viewport;
                services[typeof(ISnapEngine)] = snapEngine;
            }

            public IViewport Viewport { get; }
            public IReadOnlyCollection<ITool> Tools => Array.Empty<ITool>();
            public object GetService(Type serviceType) => services.TryGetValue(serviceType, out var value) ? value : null;
            public T GetService<T>() where T : class => GetService(typeof(T)) as T;
            public void RegisterTool(ITool tool) { }
            public void UnregisterTool(ITool tool) { }
            public void SuspendAll() { }
            public void SuspendAll(ITool exclude) { }
            public void UnsuspendAll() { }
            public ITool GetTool(Guid id) => null;
            public ITool GetTool(string name) => null;
            public TTool GetTool<TTool>() where TTool : class, ITool => null;
            public bool ActivateTool(Guid id) => false;
            public bool ActivateTool(ITool tool) => false;
            public void DeactivateAll() { }
            public bool DeactivateTool(ITool tool) => false;
        }

        private sealed class TestSnapEngine : ISnapEngine
        {
            public int UpdateCalls { get; private set; }
            public double ToleranceWorld { get; set; }
            public ISnapModePolicy ModePolicy => null;
            public SnapResult CurrentSnap => CurrentSnapValue;
            public SnapResult CurrentSnapValue { get; set; }
            public void Update(Point worldPos, IEnumerable<Primusz.AeroCAD.Core.Drawing.Entities.Entity> candidates) { UpdateCalls++; }
            public void Update(Point worldPos, IEnumerable<ISnapDescriptor> descriptors) { UpdateCalls++; }
            public Point Snap(Point rawPos) => CurrentSnap?.Point ?? rawPos;
        }

        private sealed class TestMarkerAppearanceService : IMarkerAppearanceService
        {
            public event EventHandler AppearanceChanged { add { } remove { } }
            public double MarkerSize { get; set; } = 10;
            public double MarkerStrokeThickness { get; set; } = 1;
            public Color GripEndpointColor { get; set; } = Colors.Lime;
            public Color GripMidpointColor { get; set; } = Colors.Cyan;
            public Color GripActiveColor { get; set; } = Colors.Yellow;
            public Color GripBorderColor { get; set; } = Colors.Black;
            public Color SnapStrokeColor { get; set; } = Colors.White;
            public Color SnapHoverColor { get; set; } = Colors.White;
        }
    }
}
