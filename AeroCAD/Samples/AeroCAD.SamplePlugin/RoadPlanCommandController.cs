using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanCommandController : PointSequenceCommandControllerBase
    {
        private static readonly CommandKeywordOption UndoKeyword = new CommandKeywordOption("UNDO", new[] { "U" }, "Remove the last alignment point.");
        private static readonly CommandKeywordOption RadiusKeyword = new CommandKeywordOption("RADIUS", new[] { "R" }, "Set curve radius for the previous vertex.");
        private static readonly CommandKeywordOption CloseKeyword = new CommandKeywordOption("CLOSE", new[] { "C" }, "Close the alignment.");

        private static readonly CommandStep RoadPlanFirstPointStep = new CommandStep("FirstPoint", "Specify first alignment point:");
        private static readonly CommandStep RoadPlanNextPointStep = new CommandStep(
            "NextPoint",
            "Specify next alignment point:",
            keywords: new[] { UndoKeyword, RadiusKeyword, CloseKeyword });
        private static readonly CommandStep RadiusStep = new CommandStep("Radius", "Specify curve radius for previous vertex <0>:");

        private readonly Func<Layer> activeLayerResolver;
        private readonly List<RoadPlanVertexDraft> vertices = new List<RoadPlanVertexDraft>();
        private bool closeOnCreate;

        public RoadPlanCommandController()
            : this(null)
        {
        }

        public RoadPlanCommandController(Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "ROADPLAN";

        protected override CommandStep FirstPointStep => RoadPlanFirstPointStep;

        protected override CommandStep NextPointStep => RoadPlanNextPointStep;

        protected override string MinimumPointCountMessage => "At least two alignment points are required.";

        protected override string CanceledMessage => "ROADPLAN canceled.";

        protected override string CreatedMessage => "ROADPLAN created.";

        protected override string FirstPointRemovedMessage => "First alignment point removed.";

        protected override string LastPointRemovedMessage => "Last alignment point removed.";

        protected override bool TrySubmitCustomToken(IInteractiveCommandHost host, CommandInputToken token, out InteractiveCommandResult result)
        {
            if (host?.CurrentStep == RadiusStep)
            {
                result = SubmitRadius(host, token);
                return true;
            }

            if (TryResolveKeyword(host, token, out var keyword))
            {
                if (keyword == UndoKeyword)
                {
                    result = UndoLastPoint(host);
                    return true;
                }

                if (keyword == RadiusKeyword)
                {
                    result = BeginRadius(host);
                    return true;
                }

                if (keyword == CloseKeyword)
                {
                    result = CloseAndCreate(host);
                    return true;
                }
            }

            result = null;
            return false;
        }

        protected override bool TryCompleteCustom(IInteractiveCommandHost host, out InteractiveCommandResult result)
        {
            if (host?.CurrentStep == RadiusStep)
            {
                result = SetPreviousVertexRadius(host, 0d, logInput: true);
                return true;
            }

            result = null;
            return false;
        }

        protected override void OnPointAdded(Point point)
        {
            vertices.Add(new RoadPlanVertexDraft(point));
        }

        protected override void OnPointRemoved(Point point)
        {
            if (vertices.Count > 0)
                vertices.RemoveAt(vertices.Count - 1);

            if (vertices.Count == 0)
                closeOnCreate = false;
        }

        protected override Entity CreateEntity()
        {
            var source = vertices.Select(vertex => vertex.ToVertex()).ToList();
            if (closeOnCreate && source.Count > 0)
                source.Add(new RoadPlanVertex(source[0].Location, source[0].Radius, source[0].InTransition, source[0].OutTransition));

            closeOnCreate = false;
            return new RoadPlanEntity(source);
        }

        protected override GripPreview CreatePreview(Point? previewPoint)
        {
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

            var tangentGeometry = RoadPlanGeometryBuilder.BuildTangentGeometry(
                RoadPlanGeometryBuilder.BuildControlSegments(previewVertices.Select(vertex => vertex.Location).ToList()));
            if (tangentGeometry != null && !tangentGeometry.IsEmpty())
                strokes.Add(GripPreviewStroke.CreateScreenConstant(tangentGeometry, Colors.White, 1.0d, DashStyles.Dash));

            return new GripPreview(strokes);
        }

        protected override Layer ResolveActiveLayer(IInteractiveCommandHost host)
        {
            return activeLayerResolver?.Invoke() ?? base.ResolveActiveLayer(host);
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
            SetSequencePreview(host);
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

            closeOnCreate = true;
            return CompleteSequence(host, "ROADPLAN closed and created.");
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
