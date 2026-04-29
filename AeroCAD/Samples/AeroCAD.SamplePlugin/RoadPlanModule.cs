using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanModule : CadModuleBase
    {
        public override string Name => "AeroCAD.SamplePlugin.RoadPlan";

        public override string Version => "1.0.0";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get
            {
                yield return EntityPluginBuilder
                    .Create("AeroCAD.SamplePlugin.RoadPlan")
                    .WithRenderStrategy(new RoadPlanRenderStrategy())
                    .WithBoundsStrategy(new RoadPlanBoundsStrategy())
                    .WithGripPreviewStrategy(new RoadPlanGripPreviewStrategy())
                    .WithInteractiveCommand(CreateRoadPlanCommand())
                    .BuildPlugin();
            }
        }

        private static InteractiveCommandRegistration CreateRoadPlanCommand()
        {
            return new InteractiveCommandRegistration(
                "ROADPLAN",
                () => new RoadPlanCommandController(),
                aliases: new[] { "RP" },
                description: "Create road alignment.",
                policy: EditorCommandPolicy.Default,
                assignActiveLayer: true,
                menuGroup: "Draw",
                menuLabel: "_Road Plan");
        }

    }
}
