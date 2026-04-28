using System.Collections.Generic;
using System.Windows;
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
                    .WithInteractiveCommand(CreateRoadPlanCommand())
                    .BuildPlugin();
            }
        }

        private static InteractiveCommandRegistration CreateRoadPlanCommand()
        {
            return InteractiveCommandRegistrationBuilder
                .Create("ROADPLAN")
                .WithAliases("RP")
                .WithDescription("Create sample road alignment.")
                .InMenu("Draw", "_Road Plan")
                .CreateEntityOnPoint((context, insertionPoint) => CreateDemoRoadPlan(insertionPoint), "ROADPLAN created.")
                .Build();
        }

        private static RoadPlanEntity CreateDemoRoadPlan(Point origin)
        {
            return new RoadPlanEntity(new[]
            {
                new RoadPlanVertex(origin + new Vector(0, 120)),
                new RoadPlanVertex(origin + new Vector(200, 0), 100, 50, 50),
                new RoadPlanVertex(origin + new Vector(500, 420), 250, 70, 70),
                new RoadPlanVertex(origin + new Vector(830, 450), 100, 50, 50),
                new RoadPlanVertex(origin + new Vector(1000, 100), 500, 0, 0),
                new RoadPlanVertex(origin + new Vector(1300, 0))
            });
        }
    }
}
