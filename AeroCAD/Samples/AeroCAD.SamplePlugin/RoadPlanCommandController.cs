using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption UndoKeyword = new CommandKeywordOption("UNDO", new[] { "U" }, "Remove the last alignment point.");
        private static readonly CommandKeywordOption RadiusKeyword = new CommandKeywordOption("RADIUS", new[] { "R" }, "Set curve radius for the previous vertex.");
        private static readonly CommandKeywordOption CloseKeyword = new CommandKeywordOption("CLOSE", new[] { "C" }, "Close the alignment.");

        private static readonly CommandStep FirstPointStep = new CommandStep("FirstPoint", "Specify first alignment point:");
        private static readonly CommandStep NextPointStep = new CommandStep(
            "NextPoint",
            "Specify next alignment point:",
            keywords: new[] { UndoKeyword, RadiusKeyword, CloseKeyword });
        private static readonly CommandStep RadiusStep = new CommandStep("Radius", "Specify curve radius for previous vertex <0>:");

        private readonly Func<Layer> activeLayerResolver;
        private readonly List<RoadPlanVertexDraft> vertices = new List<RoadPlanVertexDraft>();

        public RoadPlanCommandController()
            : this(null)
        {
        }

        public RoadPlanCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "ROADPLAN";

        public override CommandStep InitialStep => FirstPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            vertices.Clear();
            ClearRubberPreview(host);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (vertices.Count == 0)
                return;

            var previewPoint = host.ResolveFinalPoint(vertices[vertices.Count - 1].Location, rawPoint);
            SetPreview(host, previewPoint);
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            var basePoint = vertices.Count > 0 ? vertices[vertices.Count - 1].Location : (Point?)null;
            return SubmitPoint(host, host.ResolveFinalPoint(basePoint, rawPoint), logInput: true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (host?.CurrentStep == RadiusStep)
                return SubmitRadius(host, token);

            if (TryResolveKeyword(host, token, out var keyword))
            {
                if (keyword == UndoKeyword)
                    return UndoLastPoint(host);

                if (keyword == RadiusKeyword)
                    return BeginRadius(host);

                if (keyword == CloseKeyword)
                    return CloseAndCreate(host);
            }

            var basePoint = vertices.Count > 0 ? vertices[vertices.Count - 1].Location : (Point?)null;
            if (!host.TryResolvePointInput(token, basePoint, out var point))
                return InteractiveCommandResult.Unhandled();

            return SubmitPoint(host, point, logInput: true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            if (host?.CurrentStep == RadiusStep)
                return CompleteRadiusWithDefault(host);

            if (vertices.Count == 0)
                return EndRoadPlan(host, "ROADPLAN canceled.");

            if (vertices.Count < 2)
            {
                LogMessage(host, "At least two alignment points are required.");
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            return CreateRoadPlan(host, close: false, "ROADPLAN created.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return EndRoadPlan(host, "ROADPLAN canceled.");
        }

        private InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            vertices.Add(new RoadPlanVertexDraft(point));

            if (logInput)
                LogInput(host, InteractiveCommandToolBase.FormatPoint(point));

            SetPreview(host);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult BeginRadius(IInteractiveCommandHost host)
        {
            LogInput(host, "Radius");

            if (vertices.Count < 3)
            {
                LogMessage(host, "At least three points are required before setting a curve radius.");
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            return InteractiveCommandResult.MoveToStep(RadiusStep);
        }

        private InteractiveCommandResult SubmitRadius(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (!host.TryResolveScalarInput(token, out var radius))
                return InteractiveCommandResult.Unhandled();

            return SetPreviousVertexRadius(host, Math.Max(0d, radius), logInput: true);
        }

        private InteractiveCommandResult CompleteRadiusWithDefault(IInteractiveCommandHost host)
        {
            return SetPreviousVertexRadius(host, 0d, logInput: true);
        }

        private InteractiveCommandResult SetPreviousVertexRadius(IInteractiveCommandHost host, double radius, bool logInput)
        {
            if (vertices.Count < 3)
            {
                LogMessage(host, "At least three points are required before setting a curve radius.");
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            int previousVertexIndex = vertices.Count - 2;
            vertices[previousVertexIndex] = vertices[previousVertexIndex].WithRadius(radius);

            if (logInput)
                LogInput(host, radius.ToString("0.###", CultureInfo.InvariantCulture));

            LogMessage(host, string.Format(CultureInfo.InvariantCulture, "Curve radius set to {0:0.###}.", radius));
            SetPreview(host);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult UndoLastPoint(IInteractiveCommandHost host)
        {
            LogInput(host, "Undo");

            if (vertices.Count == 0)
            {
                LogMessage(host, "Nothing to undo.");
                ClearRubberPreview(host);
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            vertices.RemoveAt(vertices.Count - 1);

            if (vertices.Count == 0)
            {
                LogMessage(host, "First alignment point removed.");
                ClearRubberPreview(host);
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            LogMessage(host, "Last alignment point removed.");
            SetPreview(host);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult CloseAndCreate(IInteractiveCommandHost host)
        {
            LogInput(host, "Close");

            if (vertices.Count < 3)
            {
                LogMessage(host, "At least three points are required to close the alignment.");
                return InteractiveCommandResult.MoveToStep(vertices.Count == 0 ? FirstPointStep : NextPointStep);
            }

            return CreateRoadPlan(host, close: true, "ROADPLAN closed and created.");
        }

        private InteractiveCommandResult CreateRoadPlan(IInteractiveCommandHost host, bool close, string message)
        {
            var layer = ResolveActiveLayer(host);
            var document = host?.ToolService?.GetService<ICadDocumentService>();
            if (layer == null || document == null)
            {
                LogMessage(host, "No active layer is available.");
                return InteractiveCommandResult.MoveToStep(vertices.Count == 0 ? FirstPointStep : NextPointStep);
            }

            var entity = CreateEntity(close);
            var command = new AddEntityCommand(document, layer.Id, entity);
            var undoRedo = host.ToolService.GetService<IUndoRedoService>();
            if (undoRedo != null)
                undoRedo.Execute(command);
            else
                command.Execute();

            return EndRoadPlan(host, message);
        }

        private RoadPlanEntity CreateEntity(bool close)
        {
            var source = vertices.Select(vertex => vertex.ToVertex()).ToList();
            if (close && source.Count > 0)
                source.Add(new RoadPlanVertex(source[0].Location, source[0].Radius, source[0].InTransition, source[0].OutTransition));

            return new RoadPlanEntity(source);
        }

        private void SetPreview(IInteractiveCommandHost host, Point? previewPoint = null)
        {
            var rubberObject = host?.ToolService?.Viewport?.GetRubberObject();
            if (rubberObject == null)
                return;

            var previewVertices = vertices.Select(vertex => vertex.ToVertex()).ToList();
            if (previewPoint.HasValue)
                previewVertices.Add(new RoadPlanVertex(previewPoint.Value));

            var strokes = new List<GripPreviewStroke>();
            if (previewVertices.Count >= 2)
            {
                var axisGeometry = RoadPlanGeometryBuilder.BuildGeometry(previewVertices);
                if (axisGeometry != null && !axisGeometry.IsEmpty())
                    strokes.Add(GripPreviewStroke.CreateScreenConstant(axisGeometry, Colors.LightGray, 1.25d));
            }

            var tangentGeometry = RoadPlanGeometryBuilder.BuildTangentGeometry(RoadPlanGeometryBuilder.BuildControlSegments(previewVertices.Select(vertex => vertex.Location).ToList()));
            if (tangentGeometry != null && !tangentGeometry.IsEmpty())
                strokes.Add(GripPreviewStroke.CreateScreenConstant(tangentGeometry, Colors.White, 1.0d, DashStyles.Dash));

            rubberObject.Preview = new GripPreview(strokes);
        }

        private Layer ResolveActiveLayer(IInteractiveCommandHost host)
        {
            if (activeLayerResolver != null)
                return activeLayerResolver();

            var editorState = host?.ToolService?.GetService<IEditorStateService>();
            if (editorState?.ActiveLayer != null)
                return editorState.ActiveLayer;

            var document = host?.ToolService?.GetService<ICadDocumentService>();
            return document?.Layers?.Count > 0 ? document.Layers[0] : null;
        }

        private InteractiveCommandResult EndRoadPlan(IInteractiveCommandHost host, string message)
        {
            vertices.Clear();
            return EndCommand(host, message);
        }

        private static void LogInput(IInteractiveCommandHost host, string input)
        {
            host?.ToolService?.GetService<ICommandFeedbackService>()?.LogInput(input);
        }

        private static void LogMessage(IInteractiveCommandHost host, string message)
        {
            host?.ToolService?.GetService<ICommandFeedbackService>()?.LogMessage(message);
        }

        private readonly struct RoadPlanVertexDraft
        {
            public RoadPlanVertexDraft(Point location, double radius = 0d, double inTransition = 0d, double outTransition = 0d)
            {
                Location = location;
                Radius = radius;
                InTransition = inTransition;
                OutTransition = outTransition;
            }

            public Point Location { get; }

            public double Radius { get; }

            public double InTransition { get; }

            public double OutTransition { get; }

            public RoadPlanVertexDraft WithRadius(double radius)
            {
                return new RoadPlanVertexDraft(Location, radius, InTransition, OutTransition);
            }

            public RoadPlanVertex ToVertex()
            {
                return new RoadPlanVertex(Location, Radius, InTransition, OutTransition);
            }
        }
    }
}
