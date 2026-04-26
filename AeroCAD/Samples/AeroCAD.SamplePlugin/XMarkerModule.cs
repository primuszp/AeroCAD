using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class XMarkerModule : CadModuleBase
    {
        public override string Name => "AeroCAD.SamplePlugin.XMarker";

        public override string Version => "1.0.0";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get
            {
                yield return EntityPluginBuilder
                    .Create("AeroCAD.SamplePlugin.XMarker")
                    .WithRenderStrategy(new XMarkerRenderStrategy())
                    .WithBoundsStrategy(new XMarkerBoundsStrategy())
                    .WithGripPreviewStrategy(new XMarkerGripPreviewStrategy())
                    .WithInteractiveCommand(CreateXMarkerCommand())
                    .BuildPlugin();
            }
        }

        private static InteractiveCommandRegistration CreateXMarkerCommand()
        {
            var centerStep = new CommandStep("Center", "Specify X marker center:");

            return InteractiveCommandRegistrationBuilder
                .Create("XMARK")
                .WithAliases("XM")
                .WithDescription("Draw a sample external X marker.")
                .WithInitialStep(centerStep)
                .InMenu("Draw", "_X Marker")
                .CreateEntityOnPoint((context, point) => new XMarkerEntity(point, 10d), "XMARK created.")
                .Build();
        }
    }
}
